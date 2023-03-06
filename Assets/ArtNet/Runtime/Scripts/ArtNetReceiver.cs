using System;
using ArtNet.Enums;
using ArtNet.Packets;
using ArtNet.Sockets;
using UnityEngine;
using UnityEngine.Events;

namespace ArtNet
{
    public class ArtNetReceiver : MonoBehaviour
    {
        public event Action<ArtDmxPacket> OnReceiveDmxPacket;

        [SerializeField] private bool autoStart = true;

        private ArtClient _artClient;

        public DateTime LastReceivedAt { get; private set; }
        public bool IsConnected => LastReceivedAt.AddSeconds(1) > DateTime.Now;

        private void OnEnable()
        {
            ArtClientSetup();
        }

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
                    OnReceiveDmxPacket?.Invoke(e.Packet as ArtDmxPacket);
                    break;
                case OpCode.Poll:
                case OpCode.PollReply:
                default:
                    break;
            }
        }
    }
}
