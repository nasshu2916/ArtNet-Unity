using ArtNet.Enums;
using ArtNet.IO;
using ArtNet.Sockets;

namespace ArtNet.Packets
{
    public class ArtDmxPacket : ArtPacket
    {
        public ArtDmxPacket() : base(OpCode.Dmx)
        {
        }

        public ArtDmxPacket(ReceivedData data) : base(data)
        {
        }

        public byte Sequence { get; private set; }
        public byte Physical { get; private set; }
        public ushort Universe { get; private set; }
        public ushort Length { get; private set; } = 512;
        public byte[] Dmx { get; private set; } = new byte[512];

        protected override void ReadData(ArtReader reader)
        {
            ProtocolVersion = reader.ReadNetworkUInt16();
            Sequence = reader.ReadByte();
            Physical = reader.ReadByte();
            Universe = reader.ReadUInt16();
            Length = reader.ReadNetworkUInt16();
            Dmx = reader.ReadBytes(Length);
        }

        protected override void WriteData(ArtWriter writer)
        {
            base.WriteData(writer);
            writer.WriteNetwork(ProtocolVersion);
            writer.Write(Sequence);
            writer.Write(Physical);
            writer.Write(Universe);
            writer.WriteNetwork(Length);
            writer.Write(Dmx);
        }
    }
}
