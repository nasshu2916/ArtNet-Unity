using ArtNet.Enums;
using ArtNet.IO;

namespace ArtNet.Packets
{
    public class DmxPacket : ArtNetPacket
    {
        public DmxPacket() : base(OpCode.Dmx)
        {
        }

        public DmxPacket(byte[] buffer) : base(buffer, OpCode.Dmx)
        {
        }

        public byte Sequence { get; set; }
        public byte Physical { get; set; }
        public ushort Universe { get; set; }

        public ushort Length => Dmx == null ? (ushort) 0 : (ushort) Dmx.Length;

        public byte[] Dmx { get; set; }

        protected override void ReadData(ArtNetReaderOld netReader)
        {
            ProtocolVersion = netReader.ReadNetworkUInt16();
            Sequence = netReader.ReadByte();
            Physical = netReader.ReadByte();
            Universe = netReader.ReadUInt16();
            int length = netReader.ReadNetworkUInt16();
            Dmx = netReader.ReadBytes(length);
        }

        protected override void WriteData(ArtNetWriter netWriter)
        {
            base.WriteData(netWriter);
            netWriter.WriteNetwork(ProtocolVersion);
            netWriter.Write(Sequence);
            netWriter.Write(Physical);
            netWriter.Write(Universe);
            netWriter.WriteNetwork(Length);
            netWriter.Write(Dmx);
        }
    }
}
