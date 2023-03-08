using System;
using ArtNet.Enums;
using ArtNet.Packets;
using ArtNet.Sockets;
using UnityEngine;
using UnityEngine.Events;

namespace ArtNet
{
    [Serializable]
    public class OnReceivedArtDmxEvent : UnityEvent<ArtDmxPacket>
    {
    }

    [Serializable]
    public class OnReceivedArtPollEvent : UnityEvent<ArtPollPacket>
    {
    }

    [Serializable]
    public class OnReceivedArtPollReplyEvent : UnityEvent<ArtPollReplyPacket>
    {
    }

    public class ArtNetReceiver : MonoBehaviour
    {
        [SerializeField] private OnReceivedArtDmxEvent onReceivedDmxEvent;
        [SerializeField] private OnReceivedArtPollEvent onReceivedPollEvent;
        [SerializeField] private OnReceivedArtPollReplyEvent onReceivedPollReplyEvent;
        [SerializeField] private bool autoStart = true;

        private ArtClient _artClient;

        public DateTime LastReceivedAt { get; private set; }
        public bool IsConnected => LastReceivedAt.AddSeconds(1) > DateTime.Now;

        private void OnDisable()
        {
            ArtClientStop();
        }

        private void Start()
        {
            if (autoStart) ArtClientStart();
        }

        private void ArtClientSetup()
        {
            _artClient = new ArtClient();
            _artClient.ReceiveEvent += OnReceiveEvent;
        }

        public void ArtClientStart()
        {
            ArtClientSetup();
            _artClient?.UdpStart();
        }

        public void ArtClientStop()
        {
            _artClient?.UdpStop();
        }

        private void OnReceiveEvent(object sender, ReceiveEventArgs<ArtPacket> e)
        {
            var receivedAt = e.ReceivedAt;
            if (LastReceivedAt < receivedAt) LastReceivedAt = receivedAt;

            switch (e.Packet.OpCode)
            {
                case OpCode.Dmx:
                    onReceivedDmxEvent?.Invoke(e.Packet as ArtDmxPacket);
                    break;
                case OpCode.Poll:
                    onReceivedPollEvent.Invoke(e.Packet as ArtPollPacket);
                    break;
                case OpCode.PollReply:
                    onReceivedPollReplyEvent.Invoke(e.Packet as ArtPollReplyPacket);
                    break;
                default:
                    break;
            }
        }
    }
}
