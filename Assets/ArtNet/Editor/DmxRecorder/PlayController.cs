using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArtNet.Editor.DmxRecorder.IO;
using ArtNet.Packets;
using JetBrains.Annotations;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    public enum PlaybackState
    {
        Stop,
        Play,
        Pause
    }

    public class PlayController
    {
        [NotNull] private readonly UdpSender _sender = new();

        private int _lastTime = -1;
        private PlaybackState _state = PlaybackState.Stop;

        public int LastSend
        {
            get => _lastTime;
            private set
            {
                _lastTime = value;
                TimeChanged?.Invoke(value);
            }
        }

        public PlaybackState State
        {
            get => _state;
            private set
            {
                if (_state == value) return;

                _state = value;
                StateChanged?.Invoke(_state);
            }
        }

        public int MaxTime { get; private set; }

        [NotNull] public PlayControllerSetting ControllerSetting { get; }

        [CanBeNull] private Task _task;
        [CanBeNull] private CancellationTokenSource _cancellationTokenSource;

        [NotNull] private List<(int time, DmxPacket packet)> DmxPackets { get; set; } = new();

        public event Action<int> TimeChanged;
        public event Action<PlaybackState> StateChanged;

        public PlayController([NotNull] PlayControllerSetting controllerSetting)
        {
            ControllerSetting = controllerSetting;
        }

        ~PlayController()
        {
            StopTask();
        }

        private bool IsTaskRunning => _task is { IsCanceled: false, IsCompleted: false };

        private void StartTask()
        {
            if (IsTaskRunning) return;

            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;
            _task = Task.Run(() =>
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
                            Debug.LogErrorFormat($"[DmxPlayerSendTask] {e.GetType()} : {e.Message}");
                        }

                        Thread.Sleep(1);
                    }
                },
                token);
        }

        private void StopTask()
        {
            if (!IsTaskRunning) return;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            _task = null;
        }

        public void Play()
        {
            if (State == PlaybackState.Play) return;

            State = PlaybackState.Play;
        }

        public void Pause()
        {
            if (State == PlaybackState.Pause) return;

            State = PlaybackState.Pause;
        }

        public void LoadFile([NotNull] string path)
        {
            if (!File.Exists(path)) return;

            // SenderSettings.LoadFilePath = path;
            var data = File.ReadAllBytes(path);
            var universeData = BinaryDmx.Deserialize(data)!.OrderBy(x => x!.Time).ToList();
            foreach (var dataPacket in universeData)
            {
                var packet = new DmxPacket
                {
                    Universe = dataPacket!.Universe,
                    Dmx = dataPacket.Values
                };
                DmxPackets.Add((Mathf.RoundToInt((float) (dataPacket.Time * 1000f)), packet));
            }
            MaxTime = DmxPackets.Max(x => x.time);
        }

        /// <summary>
        /// 期間内の DmxPacket を送信する
        /// </summary>
        /// <param name="deltaTime"></param>
        private void SendDmxOfSpecifiedTime(int deltaTime)
        {
            var prevSendTime = LastSend;
            var newSendTime = LastSend + ControllerSetting.CalcDeltaTime(deltaTime);
            var isReset = false;
            if (newSendTime > MaxTime)
            {
                newSendTime = MaxTime;

                if (ControllerSetting.IsLoop)
                {
                    isReset = true;
                }
            }

            // TODO: O(n) なので DmxPackets の量が多い場合速度が遅くなるので最適化が必要
            var dmxPackets = DmxPackets.Where(x => x.time > prevSendTime && x.time <= newSendTime)
                .Select(x => x.packet);
            foreach (var packet in dmxPackets)
            {
                SendDmx(packet!);
            }

            if (isReset)
            {
                LastSend = -1;
            }
            else
            {
                LastSend = newSendTime;
            }
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
