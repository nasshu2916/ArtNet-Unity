using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using ArtNet.Enums;
using ArtNet.Packets;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    public enum RecordingStatus
    {
        None,
        Recording,
        Paused,
    }

    public class RecordController
    {
        private readonly UdpReceiver _receiver = new(ArtNetReceiver.ArtNetPort);
        private int _recordedTime;

        private List<(int, DmxPacket)> _recordedDmx = new();

        private long _recordStartTime;

        public RecordControllerSettings Settings { get; }
        public Action OnStartRecording, OnStopRecording, OnPauseRecording, OnResumeRecording;

        public RecordController(RecordControllerSettings settings)
        {
            Settings = settings;
            _receiver.OnReceivedPacket = OnReceivedPacket;
        }

        public RecordingStatus Status { get; private set; } = RecordingStatus.None;

        public int GetRecordedCount() => _recordedDmx.Count;

        public void StartRecording()
        {
            if (Status != RecordingStatus.None)
            {
                Debug.LogError("DmxRecorder is already recording");
                return;
            }

            _recordedDmx = new List<(int, DmxPacket)>();
            _recordedTime = 0;
            Status = RecordingStatus.Recording;
            _recordStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _receiver.StartReceive();
            OnStartRecording?.Invoke();
        }

        public void StopRecording()
        {
            if (Status == RecordingStatus.None)
            {
                Debug.LogError("DmxRecorder is not recording");
                return;
            }

            var time = GetRecordingTime();
            Status = RecordingStatus.None;
            _recordedTime = time;

            _receiver.StopReceive();
            StoreDmxPacket();
            OnStopRecording?.Invoke();
        }

        public void PauseRecording()
        {
            if (Status != RecordingStatus.Recording)
            {
                Debug.LogError("DmxRecorder is not recording");
                return;
            }

            var time = GetRecordingTime();
            Status = RecordingStatus.Paused;
            _recordedTime = time;
            _recordStartTime = 0;
            OnPauseRecording?.Invoke();
        }

        public void ResumeRecording()
        {
            if (Status != RecordingStatus.Paused)
            {
                Debug.LogError("DmxRecorder is not paused");
                return;
            }

            _recordStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Status = RecordingStatus.Recording;
            OnResumeRecording?.Invoke();
        }

        public int GetRecordingTime()
        {
            if (Status != RecordingStatus.Recording)
            {
                return _recordedTime;
            }

            var currentRecordTime = (int) (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _recordStartTime);
            return currentRecordTime + _recordedTime;
        }

        private void OnReceivedPacket(byte[] receiveBuffer, int length, EndPoint remoteEp)
        {
            if (Status != RecordingStatus.Recording) return;
            var packet = ArtNetPacket.Create(receiveBuffer);
            if (packet is not { OpCode: OpCode.Dmx }) return;

            StackDmxPacket((DmxPacket) packet);
        }

        private void StackDmxPacket(DmxPacket packet)
        {
            var time = GetRecordingTime();
            _recordedDmx.Add((time, packet));
        }

        private void StoreDmxPacket()
        {
            if (_recordedDmx.Count == 0)
            {
                Debug.Log("ArtNet Recorder: No data to store");
                return;
            }

            var recorderSettings = Settings.RecorderSettings.Where(x => x.Enabled && !x.HasErrors());
            foreach (var setting in recorderSettings)
            {
                switch (setting)
                {
                    case BinaryRecorderSettings binarySettings:
                        StoreBinary(binarySettings);
                        break;
                    case AnimationRecorderSettings animationSettings:
                        Store(animationSettings);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                setting.Take++;
            }
        }

        private void StoreBinary(BinaryRecorderSettings settings)
        {
            settings.FileGenerator.CreateDirectory();

            var binary = RecordData.Serialize(_recordedDmx);
            var path = settings.OutputAbsolutePath;
            File.WriteAllBytes(path, binary);
        }

        private void Store(AnimationRecorderSettings settings)
        {
            settings.FileGenerator.CreateDirectory();
            var path = settings.OutputAssetPath;

            var universeData = _recordedDmx.Select(packet => new UniverseData(packet.Item1 / 1000f, packet.Item2
                .Universe, packet.Item2.Dmx));
            var timelineConverter = new TimelineConverter(universeData);
            timelineConverter.SaveDmxTimelineClips(path);
        }
    }
}
