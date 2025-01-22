using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArtNet.Editor.DmxRecorder.IO;
using ArtNet.Packets;
using UnityEngine;
using Random = System.Random;

namespace ArtNet.Editor.DmxRecorder
{
    public class Sender
    {
        public delegate void OnChangedPlaying(bool isPlaying);
        public delegate void OnTimeChanged(int time);
        private readonly Dictionary<int, byte> _sequenceMap = new();
        private readonly UdpSender _udpSender = new();
        private CancellationTokenSource _cancellationTokenSource;
        private Task _task;

        public Sender()
        {
            ChangedPlaying += IsPlayingChanged;
        }

        private bool IsRunning => _task is { IsCanceled: false, IsCompleted: false };

        public SenderSettings SenderSettings { get; set; }
        private List<(int time, DmxPacket packet)> DmxPackets { get; set; } = new();

        public bool IsPlaying { get; private set; }
        private int LastTime { get; set; }
        public int MaxTime { get; private set; }
        public event OnTimeChanged TimeChanged;
        public event OnChangedPlaying ChangedPlaying;

        ~Sender()
        {
            StopTask();
        }

        public void Load(string path)
        {
            if (!File.Exists(path)) return;

            SenderSettings.LoadFilePath = path;
            var data = File.ReadAllBytes(path);
            var universeData = BinaryDmx.Deserialize(data).OrderBy(x => x.Time).ToList();
            foreach (var dataPacket in universeData)
            {
                var packet = new DmxPacket
                {
                    Universe = dataPacket.Universe,
                    Dmx = dataPacket.Values
                };
                DmxPackets.Add((Mathf.RoundToInt((float) (dataPacket.Time * 1000f)), packet));
            }
            MaxTime = DmxPackets.Max(x => x.time);
        }

        public void Play()
        {
            if (IsPlaying) return;
            ChangedPlaying?.Invoke(true);
        }

        public void Stop()
        {
            if (!IsPlaying) return;
            ChangedPlaying?.Invoke(false);
        }

        public void ChangePlayTime(int time)
        {
            LastTime = time;
        }

        private void Update(int deltaTime)
        {
            if (!IsPlaying) return;
            var oldTime = LastTime;
            LastTime += CalcAddTime(deltaTime);
            var isReset = false;
            if (LastTime > MaxTime)
            {
                if (!SenderSettings.IsLoop)
                {
                    ChangedPlaying?.Invoke(false);
                }

                isReset = true;
                LastTime = MaxTime;
            }

            var dmxPackets = DmxPackets.Where(x => x.time >= oldTime && x.time < LastTime).Select(x => x.packet);
            foreach (var packet in dmxPackets)
            {
                SendDmx(packet);
            }

            TimeChanged?.Invoke(LastTime);

            if (isReset) LastTime = 0;
        }

        private void SendDmx(DmxPacket packet)
        {
            var universe = packet.Universe;
            var sequence = _sequenceMap.GetValueOrDefault(universe, (byte) 0);
            sequence = sequence == byte.MaxValue ? (byte) 0 : (byte) (sequence + 1);
            packet.Sequence = sequence;
            _sequenceMap[universe] = sequence;

            var data = packet.ToByteArray();
            _udpSender.Send(data, SenderSettings.Ip, ArtNetReceiver.ArtNetPort);
        }

        private void IsPlayingChanged(bool isPlaying)
        {
            if (isPlaying)
            {
                StartTask();
            }
            else
            {
                StopTask();
            }

            IsPlaying = isPlaying;
        }

        private void StartTask()
        {
            if (IsRunning) return;

            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;
            _task = Task.Run(() => DmxSendTaskAsync(token), token);
        }

        private void StopTask()
        {
            if (!IsRunning) return;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            _task = null;
        }

        private async Task DmxSendTaskAsync(CancellationToken token)
        {
            var lastTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    Update((int) (now - lastTime));
                    lastTime = now;
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat($"[DmxSendTask] {e.GetType()} : {e.Message}");
                }

                await Task.Delay(1, token);
            }
        }

        private int CalcAddTime(int time)
        {
            var addTime = time * SenderSettings.Speed;
            var addTimeInt = (int) addTime;
            if (addTime - addTimeInt > new Random().NextDouble()) addTimeInt++;

            return addTimeInt;
        }
    }
}
