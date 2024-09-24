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
        private readonly Queue<ushort> _updatedUniverses = new();
        private Dictionary<ushort, byte[]> DmxDictionary { get; } = new();
        public Dictionary<ushort, IEnumerable<IDmxDevice>> DmxDevices { get; private set; }

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

        public void OnEnable()
        {
            DmxDevices = FindDmxDevices();
        }

        private static Dictionary<ushort, IEnumerable<IDmxDevice>> FindDmxDevices()
        {
            return FindObjectsOfType<GameObject>().SelectMany(o => o.GetComponents<IDmxDevice>())
                .GroupBy(device => device.Universe).ToDictionary(g => g.Key, g => g as IEnumerable<IDmxDevice>);
        }

        public ushort[] Universes()
        {
            return DmxDictionary.Keys.ToArray();
        }

        public byte[] DmxValues(ushort universe)
        {
            return DmxDictionary.TryGetValue(universe, out var data) ? data : new byte[512];
        }

        public void ReceivedDmxPacket(ReceivedData<DmxPacket> receivedData)
        {
            var packet = receivedData.Packet;
            var universe = packet.Universe;
            if (!DmxDictionary.ContainsKey(universe)) DmxDictionary.Add(universe, packet.Dmx);
            Buffer.BlockCopy(packet.Dmx, 0, DmxDictionary[universe], 0, 512);
            lock (_updatedUniverses)
            {
                if (_updatedUniverses.Contains(universe)) return;
                _updatedUniverses.Enqueue(universe);
            }
        }
    }
}
