using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArtNet.Enums;
using ArtNet.Packets;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    public static class RecordData
    {
        private enum DataType
        {
            ArtNet = 0,
            Dmx = 1
        }

        private const byte IdentifierLength = 4;
        private static readonly byte[] Identifiers = { 0xFF, 0x44, 0x4D, 0x58 };
        private static readonly byte[] ReservedBuffer = new byte[10];
        private const byte Version = 0x01;

        public static byte[] Serialize(IReadOnlyList<(int time, DmxPacket packet)> dmxPackets)
        {
            var startTime = dmxPackets.Select(x => x.time).OrderBy(x => x).First();
            using var memoryStream = new MemoryStream();
            memoryStream.Write(Identifiers);
            memoryStream.WriteByte(Version);
            memoryStream.WriteByte((byte) DataType.ArtNet);
            memoryStream.Write(ReservedBuffer);

            foreach (var (time, dmxPacket) in dmxPackets)
            {
                memoryStream.Write(BitConverter.GetBytes(time - startTime));
                memoryStream.Write(BitConverter.GetBytes((ushort) OpCode.Dmx));
                memoryStream.WriteByte(dmxPacket.Sequence);
                memoryStream.WriteByte(dmxPacket.Physical);
                memoryStream.Write(BitConverter.GetBytes(dmxPacket.Universe));
                memoryStream.Write(BitConverter.GetBytes(dmxPacket.Length));
                memoryStream.Write(dmxPacket.Dmx);
            }

            return memoryStream.ToArray();
        }

        public static byte[] SerializeUniverseData(List<UniverseData> universeData)
        {
            var sortedData = universeData.OrderBy(x => x.Time).ToList();
            var startTime = sortedData.First().Time;
            using var memoryStream = new MemoryStream();
            memoryStream.Write(Identifiers);
            memoryStream.WriteByte(Version);
            memoryStream.WriteByte((byte) DataType.Dmx);
            memoryStream.Write(ReservedBuffer);

            foreach (var data in sortedData)
            {
                memoryStream.Write(BitConverter.GetBytes((float) data.Time - startTime));
                memoryStream.Write(BitConverter.GetBytes(data.Universe));
                memoryStream.Write(BitConverter.GetBytes(data.Values.Length));
                memoryStream.Write(data.Values);
            }

            return memoryStream.ToArray();
        }

        public static List<(int time, DmxPacket packet)> Deserialize(ReadOnlySpan<byte> data)
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

            var dataType = (DataType) data[IdentifierLength + 1];

            var position = IdentifierLength + 2 + ReservedBuffer.Length;
            var result = new List<(int time, DmxPacket packet)>();
            while (position < dataLength - 10)
            {
                var time = BitConverter.ToInt32(data[position..]);
                position += 4;
                var opCode = (OpCode) BitConverter.ToUInt16(data[position..]);
                if (opCode != OpCode.Dmx)
                {
                    Debug.LogError($"ArtNet Recorder: OpCode mismatch. Required: {OpCode.Dmx}, Found: {opCode}");
                    continue;
                }

                position += 2;
                var sequence = data[position];
                position += 1;
                var physical = data[position];
                position += 1;
                var universe = BitConverter.ToUInt16(data[position..]);
                position += 2;
                var length = BitConverter.ToUInt16(data[position..]);
                position += 2;
                var dmx = data[position..(position + length)].ToArray();
                position += length;
                result.Add((time, new DmxPacket
                {
                    Sequence = sequence,
                    Physical = physical,
                    Universe = universe,
                    Dmx = dmx
                }));
            }

            return result;
        }
    }
}
