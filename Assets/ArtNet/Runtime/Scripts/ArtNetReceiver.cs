using System;
using System.Collections;
using System.Net;
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
        [SerializeField] private string bindIpAddress = "0.0.0.0";
        public DmxDataManager dmxDataManager;
        [SerializeField] private ArtDmxReceivedEvent onReceiveDmxPacket;
        public ArtClient ArtClient { get; private set; }
        public DateTime LastReceivedTime { get; private set; }
        public bool IsConnected => LastReceivedTime.AddSeconds(1) > DateTime.Now;

        private void OnEnable()
        {
            ArtClient = new ArtClient(IPAddress.Parse(bindIpAddress));
            ArtClient.Open();

            ArtClient.ReceiveEvent += OnReceiveEvent;
        }

        private void OnDisable()
        {
            ArtClient?.Dispose();
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
