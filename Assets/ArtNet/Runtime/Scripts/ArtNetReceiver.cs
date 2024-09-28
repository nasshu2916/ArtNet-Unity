using System;
using System.Net;
using ArtNet.Enums;
using ArtNet.IO;
using ArtNet.Packets;
using UnityEngine;
using UnityEngine.Events;

namespace ArtNet
{
    [Serializable]
    internal class OnReceivedDmxEvent : UnityEvent<ReceivedData<DmxPacket>>
    {
    }

    [Serializable]
    internal class OnReceivedPollEvent : UnityEvent<ReceivedData<PollPacket>>
    {
    }

    [Serializable]
    internal class OnReceivedPollReplyEvent : UnityEvent<ReceivedData<PollReplyPacket>>
    {
    }

    public class ArtNetReceiver : MonoBehaviour
    {
        public const int ArtNetPort = 6454;

        [SerializeField] private bool _autoStart = true;
        [SerializeField] private OnReceivedDmxEvent _onReceivedDmxEvent;
        [SerializeField] private OnReceivedPollEvent _onReceivedPollEvent;
        [SerializeField] private OnReceivedPollReplyEvent _onReceivedPollReplyEvent;

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
            var packet = ArtNetPacket.Create(receiveBuffer);
            if (packet == null) return;
            LastReceivedAt = DateTime.Now;

            switch (packet.OpCode)
            {
                case OpCode.Dmx:
                    _onReceivedDmxEvent?.Invoke(ReceivedData<DmxPacket>(packet, remoteEp));
                    break;
                case OpCode.Poll:
                    _onReceivedPollEvent.Invoke(ReceivedData<PollPacket>(packet, remoteEp));
                    break;
                case OpCode.PollReply:
                    _onReceivedPollReplyEvent.Invoke(ReceivedData<PollReplyPacket>(packet, remoteEp));
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
