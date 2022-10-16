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

        public byte Sequence { get; set; }
        public byte Physical { get; set; }
        public ushort Universe { get; set; }

        public ushort Length => Dmx == null ? (ushort)0 : (ushort)Dmx.Length;

        public byte[] Dmx { get; set; }

        protected override void ReadData(ArtReader reader)
        {
            ProtocolVersion = reader.ReadNetworkUInt16();
            Sequence = reader.ReadByte();
            Physical = reader.ReadByte();
            Universe = reader.ReadUInt16();
            int length = reader.ReadNetworkUInt16();
            Dmx = reader.ReadBytes(length);
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
