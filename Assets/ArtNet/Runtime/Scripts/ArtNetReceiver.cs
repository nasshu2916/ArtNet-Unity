using System;
using System.Net;
using ArtNet.Enums;
using ArtNet.Packets;
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
            try
            {
                var receivedData = new Sockets.ReceivedData(receiveBuffer, length, (IPEndPoint) remoteEp);
                LastReceivedAt = receivedData.ReceivedAt;
                var packet = ArtPacket.Create(receivedData);
                if (packet == null) return;

                switch (packet.OpCode)
                {
                    case OpCode.Dmx:
                        onReceivedDmxEvent?.Invoke(packet as ArtDmxPacket);
                        break;
                    case OpCode.Poll:
                        onReceivedPollEvent.Invoke(packet as ArtPollPacket);
                        break;
                    case OpCode.PollReply:
                        onReceivedPollReplyEvent.Invoke(packet as ArtPollReplyPacket);
                        break;
                    default:
                        break;
                }
            }
            catch (ArgumentException e)
            {
                Debug.Log($"[ArtReceiver] Invalid ArtNet packet: {e.Message}");
            }
        }
    }
}
