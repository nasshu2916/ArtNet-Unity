using System;
using System.Buffers.Binary;
using ArtNet.Enums;
using ArtNet.IO;

namespace ArtNet.Packets
{
    public class DmxPacket : ArtNetPacket
    {
        private const int DmxHeaderOffset = 12;
        private const int MinimumDmxPacketLength = 19;

        public override OpCode OpCode => OpCode.Dmx;
        protected override int MinimumBodyLength => 7;

        public byte Sequence { get; set; }
        public byte Physical { get; set; }
        public ushort Universe { get; set; }

        public ushort Length => Dmx == null ? (ushort) 0 : (ushort) Dmx.Length;

        public byte[] Dmx { get; set; }

        public static bool TryParse(ReadOnlySpan<byte> buffer, out DmxPacket packet)
        {
            packet = null;
            if (buffer.Length < MinimumDmxPacketLength) return false;

            var protocolVersion = BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(10, 2));
            if (protocolVersion != ProtocolVersion) return false;

            var sequence = buffer[DmxHeaderOffset];
            var physical = buffer[DmxHeaderOffset + 1];
            var universe = BinaryPrimitives.ReadUInt16LittleEndian(buffer.Slice(DmxHeaderOffset + 2, 2));
            var dmxLength = BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(DmxHeaderOffset + 4, 2));
            if (512 < dmxLength) return false;
            if (buffer.Length < DmxHeaderOffset + 6 + dmxLength) return false;

            packet = new DmxPacket
            {
                Sequence = sequence,
                Physical = physical,
                Universe = universe,
                Dmx = buffer.Slice(DmxHeaderOffset + 6, dmxLength).ToArray()
            };
            return true;
        }

        protected override bool DeserializeBody(ArtNetReader artNetReader)
        {
            Sequence = artNetReader.ReadByte();
            Physical = artNetReader.ReadByte();
            Universe = artNetReader.ReadUInt16();
            int length = artNetReader.ReadNetworkUInt16();
            if (length > 512) return false;
            if (artNetReader.RemainingLength < length) return false;
            Dmx = artNetReader.ReadBytes(length);

            return true;
        }

        protected override void SerializeBody(ArtNetWriter artNetWriter)
        {
            artNetWriter.Write(Sequence);
            artNetWriter.Write(Physical);
            artNetWriter.Write(Universe);
            artNetWriter.WriteNetwork(Length);
            artNetWriter.Write(Dmx);
        }

        protected override bool Validate()
        {
            return Length is <= 512 and > 0;
        }
    }
}
