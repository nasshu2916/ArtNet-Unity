using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using ArtNet.Common;
using ArtNet.Editor.DmxRecorder.IO;
using ArtNet.Packets;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    public enum PlaybackState
    {
        Invalid = -1,
        Stop,
        Play,
        Pause
    }

    public class PlayController
    {
        [NotNull] private readonly UdpSender _sender = new();
        [NotNull] private readonly ConcurrentQueue<Action> _mainThreadQueue = new();

        private long _lastTime = 0;
        private PlaybackState _state = PlaybackState.Stop;

        public long LastSend
        {
            get => _lastTime;
            private set
            {
                _lastTime = value;
                EnqueueMainThreadAction(() => TimeChanged?.Invoke(value));
            }
        }

        public PlaybackState State
        {
            get => _state;
            private set
            {
                if (_state == value) return;

                _state = value;
                EnqueueMainThreadAction(() => StateChanged?.Invoke(value));
            }
        }

        public bool IsLoaded => !string.IsNullOrEmpty(LoadedFilePath);
        public long MaxTime { get; private set; }

        [NotNull] public PlayControllerSetting ControllerSetting { get; }

        [CanBeNull] private Task _task;
        [CanBeNull] private CancellationTokenSource _cancellationTokenSource;

        [NotNull] private List<(long time, DmxPacket packet)> DmxPackets { get; set; } = new();
        private string LoadedFilePath { get; set; } = string.Empty;

        public event Action<long> TimeChanged;
        public event Action<PlaybackState> StateChanged;

        private const string LastLoadedFilePathKey = "DmxPlayerLastLoadedFilePath";
        private const string LastLoadedFileDigestKey = "DmxPlayerLastLoadedFileDigest";

        public PlayController([NotNull] PlayControllerSetting controllerSetting)
        {
            ControllerSetting = controllerSetting;
        }

        ~PlayController()
        {
            StopTask();
        }

        private bool IsTaskRunning => _task is { IsCanceled: false, IsCompleted: false };

        private void EnqueueMainThreadAction(Action action)
        {
            _mainThreadQueue.Enqueue(action);
        }

        public void ProcessMainThreadUpdates()
        {
            while (_mainThreadQueue.TryDequeue(out var action))
            {
                action?.Invoke();
            }
        }

        private void StartTask()
        {
            if (IsTaskRunning) return;

            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;
            _task = Task.Run(() => RunTask(token), token);
        }

        private void StopTask()
        {
            if (!IsTaskRunning) return;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            _task = null;
        }

        private void RunTask(CancellationToken token)
        {
            var lastTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            while (!token.IsCancellationRequested)
            {
                if (State != PlaybackState.Play)
                {
                    Thread.Sleep(1);
                    continue;
                }

                try
                {
                    var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    SendDmxOfSpecifiedTime((int) (now - lastTime));
                    lastTime = now;
                }
                catch (Exception e)
                {
                    ArtNetLogger.LogError("ArtNet DmxPlayerSendTask", $"{e.GetType()} : {e.Message}");
                }

                Thread.Sleep(1);
            }
        }

        public void Play()
        {
            if (State == PlaybackState.Play) return;

            State = PlaybackState.Play;

            // 一旦 Play 開始時に Task を起動する
            StartTask();
        }

        public void Pause()
        {
            if (State == PlaybackState.Pause) return;

            State = PlaybackState.Pause;
        }

        public void ChangePlayTime(long time)
        {
            LastSend = time;
        }

        public bool LoadFile([NotNull] string path)
        {
            if (!File.Exists(path)) return false;

            var data = File.ReadAllBytes(path);
            var result = BinaryDmx.Deserialize(data);
            if (result == null)
            {
                Debug.LogError("Failed to deserialize ArtNet binary file.");
                return false;
            }

            DmxPackets = result.OrderBy(x => x!.Time).Select(dataPacket =>
            {
                var packet = new DmxPacket
                {
                    Universe = dataPacket!.Universe,
                    Dmx = dataPacket.Values
                };
                return (dataPacket.Time, packet);
            }).ToList();
            LoadedFilePath = path;

            MaxTime = DmxPackets.Max(x => x.time);
            LastSend = 0;

            // EditorUserSettings に最後に読み込んだファイルの情報を保存する
            var hash = new MD5CryptoServiceProvider().ComputeHash(data);
            var hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();
            EditorUserSettings.SetConfigValue(LastLoadedFilePathKey, path);
            EditorUserSettings.SetConfigValue(LastLoadedFileDigestKey, hashString);
            return true;
        }

        /// <summary>
        /// 最後に読み込んだファイル Path を取得する
        /// 最後に読み込んだファイルが存在しない場合や Digest が一致しない場合は null を返す
        /// </summary>
        /// <returns></returns>
        public string LastLoadedFilePath()
        {
            var path = EditorUserSettings.GetConfigValue(LastLoadedFilePathKey);
            if (string.IsNullOrEmpty(path)) return null;
            if (!File.Exists(path)) return null;

            using var md5 = MD5.Create();
            using var stream = new FileStream(path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 8192,
                useAsync: false);
            var hash = md5.ComputeHash(stream);
            var hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();

            var lastDigest = EditorUserSettings.GetConfigValue(LastLoadedFileDigestKey);
            return hashString == lastDigest ? path : null;
        }

        /// <summary>
        /// 期間内の DmxPacket を送信する
        /// </summary>
        /// <param name="deltaTime"></param>
        private void SendDmxOfSpecifiedTime(int deltaTime)
        {
            var prevSendTime = LastSend;
            deltaTime = ControllerSetting.CalcDeltaTime(deltaTime);
            var newSendTime = prevSendTime + deltaTime;
            var isReset = false;
            if (newSendTime > MaxTime)
            {
                newSendTime = MaxTime;

                if (ControllerSetting.IsLoop)
                {
                    isReset = true;
                }
                else
                {
                    State = PlaybackState.Stop;
                }
            }

            // TODO: O(n) なので DmxPackets の量が多い場合速度が遅くなるので最適化が必要
            var dmxPackets = DmxPackets.Where(x => x.time > prevSendTime && x.time <= newSendTime)
                .OrderBy(x => x.time)
                .Select(x => x.packet);
            foreach (var packet in dmxPackets)
            {
                SendDmx(packet!);
            }

            LastSend = isReset ? 0 : newSendTime;
        }

        private void SendDmx([NotNull] DmxPacket packet)
        {
            var data = packet.ToByteArray();
            foreach (var sendEndPoint in ControllerSetting.SendEndPoints())
            {
                _sender.Send(data!, sendEndPoint);
            }
        }
    }
}
