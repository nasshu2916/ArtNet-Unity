using System.Collections.Generic;
using System.Linq;
using ArtNet.Packets;
using UnityEngine;

namespace ArtNet
{
    public class DmxDataManager : MonoBehaviour
    {
        private Dictionary<int, byte[]> DmxDictionary { get; } = new();
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

        public int[] Universes()
        {
            return DmxDictionary.Keys.ToArray();
        }

        public byte[] DmxValues(int universe)
        {
            return DmxDictionary.TryGetValue(universe, out var data) ? data : new byte[512];
        }

        private void ReceiveArtDmxPacket(ArtDmxPacket packet)
        {
            if (DmxDictionary.ContainsKey(packet.Universe))
            {
                DmxDictionary[packet.Universe] = packet.Dmx;
            }
            else
            {
                DmxDictionary.Add(packet.Universe, packet.Dmx);
            }
        }
    }
}
