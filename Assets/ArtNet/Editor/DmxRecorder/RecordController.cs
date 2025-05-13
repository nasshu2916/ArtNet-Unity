using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using ArtNet.Common;
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

        private List<(long, DmxPacket)> _recordedDmx = new();

        private long _recordStartTime;

        public RecordControllerSettings ControllerSettings { get; }
        public long RecordedTime { get; private set; }

        public Action OnStartRecording, OnStopRecording, OnPauseRecording, OnResumeRecording;

        public RecordController(RecordControllerSettings controllerSettings)
        {
            ControllerSettings = controllerSettings;
            _receiver.OnReceivedPacket = OnReceivedPacket;
        }

        public RecordingStatus Status { get; private set; } = RecordingStatus.None;

        public int GetRecordedCount() => _recordedDmx.Count;

        public void StartRecording()
        {
            if (Status != RecordingStatus.None)
            {
                ArtNetLogger.DevLogError("DmxRecorder is already recording");
                return;
            }

            _recordedDmx = new List<(long, DmxPacket)>();
            RecordedTime = 0;
            Status = RecordingStatus.Recording;
            _recordStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _receiver.StartReceive();
            OnStartRecording?.Invoke();
        }

        public void StopRecording()
        {
            if (Status == RecordingStatus.None)
            {
                ArtNetLogger.DevLogError("DmxRecorder is not recording");
                return;
            }

            var time = GetRecordingTime();
            Status = RecordingStatus.None;
            RecordedTime = time;

            _receiver.StopReceive();
            StoreDmxPacket();
            OnStopRecording?.Invoke();
        }

        public void PauseRecording()
        {
            if (Status != RecordingStatus.Recording)
            {
                ArtNetLogger.DevLogError("DmxRecorder is not recording");
                return;
            }

            var time = GetRecordingTime();
            Status = RecordingStatus.Paused;
            RecordedTime = time;
            _recordStartTime = 0;
            OnPauseRecording?.Invoke();
        }

        public void ResumeRecording()
        {
            if (Status != RecordingStatus.Paused)
            {
                ArtNetLogger.DevLogError("DmxRecorder is not paused");
                return;
            }

            _recordStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Status = RecordingStatus.Recording;
            OnResumeRecording?.Invoke();
        }

        public long GetRecordingTime()
        {
            if (Status != RecordingStatus.Recording)
            {
                return RecordedTime;
            }

            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var currentRecordTime = now - _recordStartTime;
            return currentRecordTime + RecordedTime;
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
                ArtNetLogger.DevLogInfo("ArtNet Recorder: No data to store");
                return;
            }

            var recorderSettings = ControllerSettings.RecorderSettings.Where(x => x.Enabled && !x.HasErrors());
            foreach (var setting in recorderSettings)
            {
                var universeData = setting.UniverseFilter.Filter(_recordedDmx, frame => frame.Item2.Universe)
                    .Select(x => new UniverseData(x.Item1, x.Item2.Universe, x.Item2.Dmx));

                setting.StoreUniverseData(universeData);
                setting.Take++;
            }
        }
    }
}
