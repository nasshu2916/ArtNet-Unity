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
        private readonly Dictionary<IDmxDevice, byte[]> _deviceDmxBufferCache = new();
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
                        // 毎フレームの配列生成を避け、デバイスごとに再利用して GC を抑える
                        var deviceDmx = GetOrCreateDeviceDmxBuffer(device);
                        Buffer.BlockCopy(dmx, device.StartAddress, deviceDmx, 0, device.ChannelNumber);
                        device.DmxUpdate(deviceDmx);
                    }
                }
            }
        }

        public void OnEnable()
        {
            DmxDevices = FindDmxDevices();
            _deviceDmxBufferCache.Clear();
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
            var targetBuffer = DmxDictionary[universe];
            var copyLength = Math.Min(packet.Dmx.Length, targetBuffer.Length);
            Buffer.BlockCopy(packet.Dmx, 0, targetBuffer, 0, copyLength);
            lock (_updatedUniverses)
            {
                if (_updatedUniverses.Contains(universe)) return;
                _updatedUniverses.Enqueue(universe);
            }
        }

        private byte[] GetOrCreateDeviceDmxBuffer(IDmxDevice device)
        {
            if (_deviceDmxBufferCache.TryGetValue(device, out var buffer))
            {
                // デバイスのチャンネル数が変わった場合は新しいバッファを作る
                if (buffer != null && buffer.Length == device.ChannelNumber) return buffer;
                buffer = new byte[device.ChannelNumber];
                _deviceDmxBufferCache[device] = buffer;
                return buffer;
            }
            var newBuffer = new byte[device.ChannelNumber];
            _deviceDmxBufferCache[device] = newBuffer;
            return newBuffer;
        }
    }
}
