using System;
using ArtNet.Enums;
using ArtNet.Packets;
using ArtNet.Sockets;
using UnityEngine;
using UnityEngine.Events;

namespace ArtNet
{
    [Serializable]
    public class ArtDmxReceivedEvent : UnityEvent<ArtDmxPacket>
    {
    }

    [RequireComponent(typeof(DmxDataManager))]
    public class ArtNetReceiver : MonoBehaviour
    {
        public DmxDataManager dmxDataManager;
        [SerializeField] private ArtDmxReceivedEvent onReceiveDmxPacket;
        [SerializeField] private bool autoStart = true;
        public ArtClient ArtClient { get; private set; }
        public DateTime LastReceivedAt { get; private set; }
        public bool IsConnected => LastReceivedAt.AddSeconds(1) > DateTime.Now;

        private void OnEnable()
        {
            if (autoStart) ArtClientStart();
        }

        private void OnDisable()
        {
            ArtClient?.UdpStop();
        }

        private void Start()
        {
            dmxDataManager = GetComponent<DmxDataManager>();
        }

        private void ArtClientStart()
        {
            ArtClient = new ArtClient();
            ArtClient.ReceiveEvent += OnReceiveEvent;
            ArtClient.UdpStart();
        }

        private void OnReceiveEvent(object sender, ReceiveEventArgs<ArtPacket> e)
        {
            var receivedAt = e.ReceivedAt;
            if (LastReceivedAt < receivedAt) LastReceivedAt = receivedAt;
            
            switch (e.Packet.OpCode)
            {
                case OpCode.Dmx:
                    onReceiveDmxPacket?.Invoke(e.Packet as ArtDmxPacket);
                    break;
                case OpCode.Poll:
                case OpCode.PollReply:
                default:
                    break;
            }
        }
    }
}
