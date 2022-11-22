using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using ArtNet.Samples.Devices;
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
        private IDmxDevice[] _dmxDevices;

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

        private void Start()
        {
            _dmxDevices = FindObjectsOfType<GameObject>().SelectMany(o => o.GetComponents<IDmxDevice>()).ToArray();
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

        private void Update()
        {
            foreach (var device in _dmxDevices)
            {
                device.DmxUpdate(GetDmx(device.Universe).Skip(device.StartAddress).Take(device.ChannelNumber).ToArray());
            }
        }
    }
}
