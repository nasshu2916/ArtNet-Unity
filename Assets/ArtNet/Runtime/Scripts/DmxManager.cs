using System;
using System.Collections.Generic;
using System.Linq;
using ArtNet.Devices;
using ArtNet.Packets;
using UnityEngine;

namespace ArtNet
{
    public class DmxManager : MonoBehaviour
    {
        [SerializeField] private ArtNetReceiver artNetReceiver;

        private Dictionary<ushort, byte[]> DmxDictionary { get; } = new();
        private readonly Queue<ushort> _updatedUniverses = new();
        public Dictionary<ushort, IEnumerable<IDmxDevice>> DmxDevices { get; private set; }

        private static Dictionary<ushort, IEnumerable<IDmxDevice>> FindDmxDevices()
        {
            return FindObjectsOfType<GameObject>().SelectMany(o => o.GetComponents<IDmxDevice>())
                .GroupBy(device => device.Universe).ToDictionary(g => g.Key, g => g as IEnumerable<IDmxDevice>);
        }

        public void OnEnable()
        {
            DmxDevices = FindDmxDevices();
        }

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

        public void Update()
        {
            lock (_updatedUniverses)
            {
                while (0 < _updatedUniverses.Count)
                {
                    var universe = _updatedUniverses.Dequeue();
                    var dmx = DmxDictionary[universe];
                    DmxDevices.TryGetValue(universe, out var devices);
                    if (devices == null) continue;
                    foreach (var device in devices)
                    {
                        var deviceDmx = new byte[device.ChannelNumber];
                        Buffer.BlockCopy(dmx, device.StartAddress, deviceDmx, 0, device.ChannelNumber);
                        device.DmxUpdate(deviceDmx);
                    }
                }
            }
        }

        public ushort[] Universes()
        {
            return DmxDictionary.Keys.ToArray();
        }

        public byte[] DmxValues(ushort universe)
        {
            return DmxDictionary.TryGetValue(universe, out var data) ? data : new byte[512];
        }

        private void ReceiveArtDmxPacket(ArtDmxPacket packet)
        {
            var universe = packet.Universe;
            if (!DmxDictionary.ContainsKey(universe)) DmxDictionary.Add(universe, new byte[512]);
            Buffer.BlockCopy(packet.Dmx, 0, DmxDictionary[universe], 0, 512);
            if (!_updatedUniverses.Contains(universe))
            {
                lock (_updatedUniverses) _updatedUniverses.Enqueue(universe);
            }
        }
    }
}
