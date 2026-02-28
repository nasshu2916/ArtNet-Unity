using System;
using System.Net;
using ArtNet.Enums;
using ArtNet.Packets;
using UnityEngine;
using UnityEngine.Events;

namespace ArtNet
{
    [Serializable]
    internal class OnReceivedPollEvent : UnityEvent<ReceivedData<PollPacket>>
    {
    }

    [Serializable]
    internal class OnReceivedPollReplyEvent : UnityEvent<ReceivedData<PollReplyPacket>>
    {
    }

    [Serializable]
    internal class OnReceivedDmxEvent : UnityEvent<ReceivedData<DmxPacket>>
    {
    }

    [Serializable]
    internal class OnReceivedSyncEvent : UnityEvent<ReceivedData<SyncPacket>>
    {
    }

    [Serializable]
    internal class OnReceivedTimeCodeEvent : UnityEvent<ReceivedData<TimeCodePacket>>
    {
    }

    [Serializable]
    internal class OnReceivedAddressEvent : UnityEvent<ReceivedData<AddressPacket>>
    {
    }

    [Serializable]
    internal class OnReceivedTodRequestEvent : UnityEvent<ReceivedData<TodRequestPacket>>
    {
    }

    [Serializable]
    internal class OnReceivedTodDataEvent : UnityEvent<ReceivedData<TodDataPacket>>
    {
    }

    [Serializable]
    internal class OnReceivedTodControlEvent : UnityEvent<ReceivedData<TodControlPacket>>
    {
    }

    [Serializable]
    internal class OnReceivedRdmEvent : UnityEvent<ReceivedData<RdmPacket>>
    {
    }

    public class ArtNetReceiver : MonoBehaviour
    {
        public const int ArtNetPort = 6454;

        [SerializeField] private bool _autoStart = true;
        [SerializeField] private OnReceivedPollEvent _onReceivedPollEvent;
        [SerializeField] private OnReceivedPollReplyEvent _onReceivedPollReplyEvent;
        [SerializeField] private OnReceivedDmxEvent _onReceivedDmxEvent;
        [SerializeField] private OnReceivedSyncEvent _onReceivedSyncEvent;
        [SerializeField] private OnReceivedTimeCodeEvent _onReceivedTimeCodeEvent;
        [SerializeField] private OnReceivedAddressEvent _onReceivedAddressEvent;
        [SerializeField] private OnReceivedTodRequestEvent _onReceivedTodRequestEvent;
        [SerializeField] private OnReceivedTodDataEvent _onReceivedTodDataEvent;
        [SerializeField] private OnReceivedTodControlEvent _onReceivedTodControlEvent;
        [SerializeField] private OnReceivedRdmEvent _onReceivedRdmEvent;

        private UdpReceiver UdpReceiver { get; } = new(ArtNetPort);
        public DateTime LastReceivedAt { get; private set; }
        public bool IsConnected => LastReceivedAt.AddSeconds(1) > DateTime.Now;

        private void Awake()
        {
            UdpReceiver.OnReceivedPacket = OnReceivedPacket;
        }

        private void OnEnable()
        {
            if (_autoStart) UdpReceiver.StartReceive();
        }

        private void OnDisable()
        {
            UdpReceiver.StopReceive();
        }

        private void OnReceivedPacket(byte[] receiveBuffer, int length, EndPoint remoteEp)
        {
            var buffer = receiveBuffer.AsSpan(0, length);
            if (!ArtNetPacket.TryGetOpCode(buffer, out var opCode)) return;
            LastReceivedAt = DateTime.Now;

            switch (opCode)
            {
                case OpCode.Dmx:
                    if (!DmxPacket.TryParse(buffer, out var dmxPacket)) return;
                    _onReceivedDmxEvent?.Invoke(new ReceivedData<DmxPacket>(dmxPacket, remoteEp));
                    break;
                case OpCode.Poll:
                    var pollPacket = ArtNetPacket.FromByteArray<PollPacket>(buffer, false);
                    if (pollPacket == null) return;
                    _onReceivedPollEvent.Invoke(new ReceivedData<PollPacket>(pollPacket, remoteEp));
                    break;
                case OpCode.PollReply:
                    var pollReplyPacket = ArtNetPacket.FromByteArray<PollReplyPacket>(buffer, false);
                    if (pollReplyPacket == null) return;
                    _onReceivedPollReplyEvent.Invoke(new ReceivedData<PollReplyPacket>(pollReplyPacket, remoteEp));
                    break;
                case OpCode.Sync:
                    var syncPacket = ArtNetPacket.FromByteArray<SyncPacket>(buffer, false);
                    if (syncPacket == null) return;
                    _onReceivedSyncEvent?.Invoke(new ReceivedData<SyncPacket>(syncPacket, remoteEp));
                    break;
                case OpCode.TimeCode:
                    _onReceivedTimeCodeEvent?.Invoke(ReceivedData<TimeCodePacket>(packet, remoteEp));
                    break;
                case OpCode.Address:
                    _onReceivedAddressEvent?.Invoke(ReceivedData<AddressPacket>(packet, remoteEp));
                    break;
                case OpCode.TodRequest:
                    _onReceivedTodRequestEvent?.Invoke(ReceivedData<TodRequestPacket>(packet, remoteEp));
                    break;
                case OpCode.TodData:
                    _onReceivedTodDataEvent?.Invoke(ReceivedData<TodDataPacket>(packet, remoteEp));
                    break;
                case OpCode.TodControl:
                    _onReceivedTodControlEvent?.Invoke(ReceivedData<TodControlPacket>(packet, remoteEp));
                    break;
                case OpCode.Rdm:
                    _onReceivedRdmEvent?.Invoke(ReceivedData<RdmPacket>(packet, remoteEp));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
