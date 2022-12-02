using System.Collections.Generic;
using ArtNet.Packets;
using JetBrains.Annotations;
using UnityEngine;

namespace ArtNet
{
    public class DmxDataManager : MonoBehaviour
    {
        public Dictionary<int, byte[]> DmxMap { get; } = new();

        public byte[] GetDmx(int universe)
        {
            return DmxMap.TryGetValue(universe, out var data) ? data : new byte[512];
        }

        [UsedImplicitly]
        public void ReceiveArtDmxPacket(ArtDmxPacket packet)
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
