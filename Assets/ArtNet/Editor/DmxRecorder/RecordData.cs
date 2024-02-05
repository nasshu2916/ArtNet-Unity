using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArtNet.Packets;

namespace ArtNet.Editor.DmxRecorder
{
    public static class RecordData
    {
        private static readonly byte[] Identifiers = { 0xFF, 0x44, 0x4D, 0x58 };
        private const byte IdentifierLength = 4;

        public static byte[] Serialize(List<(int, DmxPacket)> dmxPackets)
        {
            var startTime = dmxPackets.Select(x => x.Item1).OrderBy(x => x).First();
            using var memoryStream = new MemoryStream();
            memoryStream.Write(Identifiers);

            foreach (var (time, dmxPacket) in dmxPackets)
            {
                memoryStream.Write(BitConverter.GetBytes(time - startTime));
                memoryStream.WriteByte(dmxPacket.Sequence);
                memoryStream.Write(BitConverter.GetBytes(dmxPacket.Universe));
                memoryStream.Write(BitConverter.GetBytes(dmxPacket.Length));
                memoryStream.Write(dmxPacket.Dmx);
            }

            return memoryStream.ToArray();
        }

        public static List<(int, DmxPacket)> Deserialize(ReadOnlySpan<byte> data)
        {
            var dataLength = data.Length;
            if (dataLength < Identifiers.Length || !data[..Identifiers.Length].SequenceEqual(Identifiers))
            {
                return null;
            }

            int position = IdentifierLength;
            var result = new List<(int, DmxPacket)>();
            while (position < dataLength - 10)
            {
                var time = BitConverter.ToInt32(data[position..]);
                position += 4;
                var sequence = data[position];
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
                    Universe = universe,
                    Dmx = dmx
                }));
            }

            return result;
        }
    }
}