using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArtNet.Packets;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    public static class RecordData
    {
        private const byte IdentifierLength = 4;
        private static readonly byte[] Identifiers = { 0xFF, 0x44, 0x4D, 0x58 };
        private static readonly byte[] ReservedBuffer = new byte[11];
        private const byte Version = 0x02;

        public static byte[] SerializePackets(IEnumerable<(int time, DmxPacket packet)> dmxPackets)
        {
            var universeData = dmxPackets.Select(packet =>
                new UniverseData(packet.time / 1000f, packet.packet.Universe, packet.packet.Dmx));

            return SerializeUniverseData(universeData);
        }

        public static byte[] SerializeUniverseData(IEnumerable<UniverseData> universeData)
        {
            var sortedData = universeData.OrderBy(x => x.Time).ToList();
            var startTime = sortedData.First().Time;
            using var memoryStream = new MemoryStream();
            memoryStream.Write(Identifiers);
            memoryStream.WriteByte(Version);
            memoryStream.Write(ReservedBuffer);

            foreach (var data in sortedData)
            {
                memoryStream.Write(BitConverter.GetBytes((float) data.Time - startTime));
                memoryStream.Write(BitConverter.GetBytes(data.Universe));
                var length = data.Length;
                memoryStream.Write(BitConverter.GetBytes(length));
                memoryStream.Write(data.Values[..length]);
            }

            return memoryStream.ToArray();
        }

        public static List<UniverseData> Deserialize(ReadOnlySpan<byte> data)
        {
            var dataLength = data.Length;
            if (dataLength < Identifiers.Length || !data[..Identifiers.Length].SequenceEqual(Identifiers))
                return null;
            var dataVersion = data[IdentifierLength];
            if (dataVersion != Version)
            {
                Debug.LogError($"ArtNet Recorder: Version mismatch. Required: {Version}, Found: {dataVersion}");
                return null;
            }

            var position = IdentifierLength + 1 + ReservedBuffer.Length;
            var result = new List<UniverseData>();
            while (position < dataLength - 12)
            {
                var time = BitConverter.ToSingle(data[position..]);
                position += 8;
                var universe = BitConverter.ToUInt16(data[position..]);
                position += 2;
                var length = BitConverter.ToUInt16(data[position..]);
                position += 2;
                if (position + length > dataLength || length > 512)
                {
                    Debug.LogError("ArtNet Recorder: Invalid data length");
                    return null;
                }

                var dmx = data[position..(position + length)];
                position += length;
                result.Add(new UniverseData(time, universe, dmx));
            }

            return result;
        }
    }
}
