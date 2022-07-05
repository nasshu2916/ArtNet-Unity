using ArtNet.IO;
using ArtNet.Sockets;

namespace ArtNet.Packets
{
    public class ArtDmxPacket : ArtPacket
    {
        public ArtDmxPacket(ReceivedData data) : base(data)
        {
        }

        public byte Sequence { get; private set; }
        public byte Physical { get; private set; }
        public ushort Universe { get; private set; }
        public ushort Length { get; private set; }
        public byte[] Dmx { get; private set; }

        protected override void ReadData(ArtReader reader)
        {
            ProtocolVersion = reader.ReadNetworkUInt16();
            Sequence = reader.ReadByte();
            Physical = reader.ReadByte();
            Universe = reader.ReadUInt16();
            Length = reader.ReadNetworkUInt16();
            Dmx = reader.ReadBytes(Length);
        }
    }
}