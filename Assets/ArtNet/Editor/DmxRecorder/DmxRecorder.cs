using System;
using System.Collections.Generic;
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

    public class DmxRecorder
    {
        private readonly UdpReceiver _receiver = new(ArtNetReceiver.ArtNetPort);
        public RecordingStatus Status { get; private set; } = RecordingStatus.None;

        private List<(int, DmxPacket)> _recordedDmx = new();

        private long _recordStartTime;
        private int _alreadyRecordedTime;

        public DmxRecorder()
        {
            _receiver.OnReceivedPacket = OnReceivedPacket;
        }

        public void StartRecording()
        {
            if (Status != RecordingStatus.None)
            {
                Debug.LogError("DmxRecorder is already recording");
                return;
            }

            _receiver.StartReceive();
            _recordedDmx = new List<(int, DmxPacket)>();
            _alreadyRecordedTime = 0;
            _recordStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Status = RecordingStatus.Recording;
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
            _alreadyRecordedTime = time;

            _receiver.StopReceive();
            StoreDmxPacket();
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
            _alreadyRecordedTime = time;
            _recordStartTime = 0;
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
        }

        public int GetRecordingTime()
        {
            if (Status != RecordingStatus.Recording)
            {
                return _alreadyRecordedTime;
            }

            var currentRecordTime = (int) (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _recordStartTime);
            return currentRecordTime + _alreadyRecordedTime;
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
            if (_recordedDmx.Count == 0) return;
            var storeData = RecordData.Serialize(_recordedDmx);
            var path = $"Assets/ArtNet/RecordedData/{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.dmx";
            System.IO.File.WriteAllBytes(path, storeData);
        }
    }
}
