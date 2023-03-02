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
        public ArtClient ArtClient { get; private set; }
        public DateTime LastReceivedTime { get; private set; }
        public bool IsConnected => LastReceivedTime.AddSeconds(1) > DateTime.Now;

        private void OnEnable()
        {
            ArtClient = new ArtClient();
            ArtClient.UdpStart();

            ArtClient.ReceiveEvent += OnReceiveEvent;
        }

        private void OnDisable()
        {
            ArtClient?.UdpStop();
        }

        private void Start()
        {
            dmxDataManager = GetComponent<DmxDataManager>();
        }

        private void OnReceiveEvent(object sender, ReceiveEventArgs<ArtPacket> e)
        {
            LastReceivedTime = DateTime.Now;
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
