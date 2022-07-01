using System.Net;
using ArtNet.Packets;
using ArtNet.Sockets;
using ArtNet.Enums;
using UnityEngine;

namespace ArtNet
{
    public class ArtNetReceiver : MonoBehaviour
    {
        [SerializeField] private string bindIpAddress = "0.0.0.0";

        private ArtClient _artClient;
        private const byte MaxUniverse = 8;
        private byte[][] _dmx = new byte[MaxUniverse][];

        private void OnEnable()
        {
            _artClient = new ArtClient(IPAddress.Parse(bindIpAddress));
            _artClient.Open();

            _artClient.ReceiveEvent += OnReceiveEvent;
        }

        private void OnDisable()
        {
            _artClient?.Close();
        }

        private void OnReceiveEvent(object sender, ReceiveEventArgs<ArtPacket> e)
        {
            switch (e.Packet.OpCode)
            {
                case OpCode.Dmx:
                    ReceiveDmxPacket(e.Packet as DmxPacket);
                    break;
                default:
                    Debug.Log("Not support OpCode: 0x" + e.Packet.OpCode.ToString("X"));
                    break;
            }
        }

        private void ReceiveDmxPacket(DmxPacket packet)
        {
            if (packet.Universe < MaxUniverse)
            {
                _dmx[packet.Universe] = packet.Dmx;
            }
        }
    }
}
