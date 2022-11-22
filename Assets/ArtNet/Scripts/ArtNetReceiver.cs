using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using ArtNet.Enums;
using ArtNet.Packets;
using ArtNet.Sockets;
using UnityEngine;

namespace ArtNet
{
    public class ArtNetReceiver : MonoBehaviour
    {
        [SerializeField] private string bindIpAddress = "0.0.0.0";

        public ArtClient ArtClient { get; private set; }
        public Dictionary<int, byte[]> DmxMap { get; } = new();

        [return: NotNull]
        public byte[] GetDmx(int universe)
        {
            return DmxMap.TryGetValue(universe, out var data) ? data : new byte[512];
        }

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

        private void OnReceiveEvent(object sender, ReceiveEventArgs<ArtPacket> e)
        {
            switch (e.Packet.OpCode)
            {
                case OpCode.Dmx:
                    ReceiveArtDmxPacket(e.Packet as ArtDmxPacket);
                    break;
                case OpCode.Poll:
                case OpCode.PollReply:
                default:
                    break;
            }
        }

        private void ReceiveArtDmxPacket(ArtDmxPacket packet)
        {
            if (DmxMap.ContainsKey(packet.Universe))
            {
                DmxMap[packet.Universe] = packet.Dmx;
            }
            else
            {
                DmxMap.Add(packet.Universe, packet.Dmx);
            }
        }
    }
}
