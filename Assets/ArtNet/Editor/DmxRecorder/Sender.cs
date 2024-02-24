using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArtNet.Packets;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    public class Sender
    {
        public delegate void OnChangedPlaying(bool isPlaying);

        public delegate void OnTimeChanged(int time);
        private readonly Dictionary<int, byte> _sequenceMap = new();
        private readonly UdpSender _udpSender = new(ArtNetReceiver.ArtNetPort);
        private CancellationTokenSource _cancellationTokenSource;
        private Task _task;

        public Sender()
        {
            ChangedPlaying += IsPlayingChanged;
        }

        private bool IsRunning => _task is { IsCanceled: false, IsCompleted: false };

        public SenderConfig Config { get; } = new();
        private List<(int, DmxPacket)> DmxPackets { get; set; } = new();

        public bool IsPlaying { get; private set; }
        private int LastTime { get; set; }
        public int MaxTime => DmxPackets.Count > 0 ? DmxPackets[^1].Item1 : 0;
        public event OnTimeChanged TimeChanged;
        public event OnChangedPlaying ChangedPlaying;

        ~Sender()
        {
            StopTask();
        }

        public void Load(string path)
        {
            if (!System.IO.File.Exists(path)) return;

            var data = System.IO.File.ReadAllBytes(path);
            DmxPackets = RecordData.Deserialize(data);
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
            LastTime += deltaTime;
            var isReset = false;
            if (LastTime > MaxTime)
            {
                if (!Config.IsLoop)
                {
                    ChangedPlaying?.Invoke(false);
                }

                isReset = true;
                LastTime = MaxTime;
            }

            var dmx = DmxPackets.FindLast(packet =>
                packet.Item1 <= LastTime && packet.Item1 > oldTime);
            if (dmx != default) SendDmx(dmx.Item2);

            TimeChanged?.Invoke(LastTime);

            if (isReset) LastTime = 0;
        }

        private void SendDmx(DmxPacket packet)
        {
            var universe = packet.Universe;
            if (!Config.IsRecordSequence)
            {
                var sequence = _sequenceMap.GetValueOrDefault(universe, (byte) 0);
                sequence = sequence == byte.MaxValue ? (byte) 0 : (byte) (sequence + 1);
                packet.Sequence = sequence;
                _sequenceMap[universe] = sequence;
            }
            else
            {
                _sequenceMap[universe] = packet.Sequence;
            }

            var data = packet.ToByteArray();
            _udpSender.Send(data, Config.Ip);
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
    }
}
