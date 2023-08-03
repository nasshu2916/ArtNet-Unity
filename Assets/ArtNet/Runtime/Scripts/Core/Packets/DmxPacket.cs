using System;
using ArtNet.Enums;

namespace ArtNet.Packets
{
    public class DmxPacket : ArtNetPacket
    {
        public DmxPacket() : base(OpCode.Dmx)
        {
        }

        public DmxPacket(ReadOnlySpan<byte> buffer) : base(buffer, OpCode.Dmx)
        {
        }

        public byte Sequence { get; set; }
        public byte Physical { get; set; }
        public ushort Universe { get; set; }

        public ushort Length => Dmx == null ? (ushort) 0 : (ushort) Dmx.Length;

        public byte[] Dmx { get; set; }

        protected override void Deserialize(ArtNetReader artNetReader)
        {
            ProtocolVersion = artNetReader.ReadNetworkUInt16();
            Sequence = artNetReader.ReadByte();
            Physical = artNetReader.ReadByte();
            Universe = artNetReader.ReadUInt16();
            int length = artNetReader.ReadNetworkUInt16();
            Dmx = artNetReader.ReadBytes(length);
        }

        protected override void Serialize(ArtNetWriter artNetWriter)
        {
            base.Serialize(artNetWriter);
            artNetWriter.WriteNetwork(ProtocolVersion);
            artNetWriter.Write(Sequence);
            artNetWriter.Write(Physical);
            artNetWriter.Write(Universe);
            artNetWriter.WriteNetwork(Length);
            artNetWriter.Write(Dmx);
        }
    }
}
