using System;
using System.Net;
using ArtNet.Enums;
using ArtNet.Packets;
using UnityEngine;
using UnityEngine.Events;

namespace ArtNet
{
    [Serializable]
    public class OnReceivedDmxEvent : UnityEvent<ReceivedData<DmxPacket>>
    {
    }

    [Serializable]
    public class OnReceivedPollEvent : UnityEvent<ReceivedData<PollPacket>>
    {
    }

    [Serializable]
    public class OnReceivedPollReplyEvent : UnityEvent<ReceivedData<PollReplyPacket>>
    {
    }

    public class ArtNetReceiver : UdpReceiver
    {
        private const int ArtNetPort = 6454;

        [SerializeField] private bool autoStart = true;
        [SerializeField] private OnReceivedDmxEvent onReceivedDmxEvent;
        [SerializeField] private OnReceivedPollEvent onReceivedPollEvent;
        [SerializeField] private OnReceivedPollReplyEvent onReceivedPollReplyEvent;

        public DateTime LastReceivedAt { get; private set; }
        public bool IsConnected => LastReceivedAt.AddSeconds(1) > DateTime.Now;

        private void Awake()
        {
            Port = ArtNetPort;
        }

        private void OnEnable()
        {
            if (autoStart) UdpStart();
        }

        private void OnDisable()
        {
            UdpStop();
        }

        protected override void OnReceivedPacket(byte[] receiveBuffer, int length, EndPoint remoteEp)
        {
            var packet = ArtNetPacket.Create(receiveBuffer);
            if (packet == null) return;
            LastReceivedAt = DateTime.Now;

            switch (packet.OpCode)
            {
                case OpCode.Dmx:
                    onReceivedDmxEvent?.Invoke(ReceivedData<DmxPacket>(packet, remoteEp));
                    break;
                case OpCode.Poll:
                    onReceivedPollEvent.Invoke(ReceivedData<PollPacket>(packet, remoteEp));
                    break;
                case OpCode.PollReply:
                    onReceivedPollReplyEvent.Invoke(ReceivedData<PollReplyPacket>(packet, remoteEp));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static ReceivedData<TPacket> ReceivedData<TPacket>(ArtNetPacket netPacket, EndPoint endPoint)
            where TPacket : ArtNetPacket
        {
            return new ReceivedData<TPacket>(netPacket as TPacket, endPoint);
        }
    }
}
