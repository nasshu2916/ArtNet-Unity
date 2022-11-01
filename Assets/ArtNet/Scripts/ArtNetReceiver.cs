using System.Collections.Generic;
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

        private ArtClient _artClient;
        private Dictionary<int, byte[]> _dmxMap = new();

        public byte[] GetDmx(int universe)
        {
            return _dmxMap.TryGetValue(universe, out var data) ? data : new byte[512];
        }

        private void OnEnable()
        {
            _artClient = new ArtClient(IPAddress.Parse(bindIpAddress));
            _artClient.Open();

            _artClient.ReceiveEvent += OnReceiveEvent;
        }

        private void OnDisable()
        {
            _artClient?.Dispose();
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
            if (_dmxMap.ContainsKey(packet.Universe))
            {
                _dmxMap[packet.Universe] = packet.Dmx;
            }
            else
            {
                _dmxMap.Add(packet.Universe, packet.Dmx);
            }
        }
    }
}
