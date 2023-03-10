using System;
using System.Net;
using ArtNet.Enums;
using ArtNet.Packets;
using UnityEngine;
using UnityEngine.Events;

namespace ArtNet
{
    [Serializable]
    public class OnReceivedArtDmxEvent : UnityEvent<ReceivedData<ArtDmxPacket>>
    {
    }

    [Serializable]
    public class OnReceivedArtPollEvent : UnityEvent<ReceivedData<ArtPollPacket>>
    {
    }

    [Serializable]
    public class OnReceivedArtPollReplyEvent : UnityEvent<ReceivedData<ArtPollReplyPacket>>
    {
    }

    public class ArtNetReceiver : UdpReceiver
    {
        private const int ArtNetPort = 6454;

        [SerializeField] private bool autoStart = true;
        [SerializeField] private OnReceivedArtDmxEvent onReceivedDmxEvent;
        [SerializeField] private OnReceivedArtPollEvent onReceivedPollEvent;
        [SerializeField] private OnReceivedArtPollReplyEvent onReceivedPollReplyEvent;

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
            var packet = ArtPacket.Create(receiveBuffer);
            if (packet == null) return;
            LastReceivedAt = DateTime.Now;

            switch (packet.OpCode)
            {
                case OpCode.Dmx:
                    onReceivedDmxEvent?.Invoke(ReceivedData<ArtDmxPacket>(packet, remoteEp));
                    break;
                case OpCode.Poll:
                    onReceivedPollEvent.Invoke(ReceivedData<ArtPollPacket>(packet, remoteEp));
                    break;
                case OpCode.PollReply:
                    onReceivedPollReplyEvent.Invoke(ReceivedData<ArtPollReplyPacket>(packet, remoteEp));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static ReceivedData<TPacket> ReceivedData<TPacket>(ArtPacket packet, EndPoint endPoint)
            where TPacket : ArtPacket
        {
            return new ReceivedData<TPacket>(packet as TPacket, endPoint);
        }
    }
}
