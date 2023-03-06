using System.Collections.Generic;
using ArtNet.Packets;
using UnityEngine;

namespace ArtNet
{
    public class DmxDataManager : MonoBehaviour
    {
        public Dictionary<int, byte[]> DmxMap { get; } = new();
        [SerializeField] private ArtNetReceiver artNetReceiver;

        public void Start()
        {
            if (artNetReceiver == null)
            {
                Debug.LogError("[DmxDataManager] Require ArtNetReceiver");
            }
            else
            {
                artNetReceiver.OnReceiveDmxPacket += ReceiveArtDmxPacket;
            }
        }

        public byte[] GetDmx(int universe)
        {
            return DmxMap.TryGetValue(universe, out var data) ? data : new byte[512];
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
