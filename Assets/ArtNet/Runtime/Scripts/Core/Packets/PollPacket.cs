using ArtNet.Enums;
using ArtNet.IO;

namespace ArtNet.Packets
{
    public class PollPacket : ArtNetPacket
    {
        public PollPacket() : base(OpCode.Poll)
        {
        }

        public PollPacket(byte[] buffer) : base(buffer, OpCode.Poll)
        {
        }

        public byte Flags { get; set; }
        public byte Priority { get; set; }


        protected override void ReadData(ArtNetReader netReader)
        {
            ProtocolVersion = netReader.ReadNetworkUInt16();
            Flags = netReader.ReadByte();
            Priority = netReader.ReadByte();
        }

        protected override void WriteData(ArtNetWriter netWriter)
        {
            base.WriteData(netWriter);
            netWriter.WriteNetwork(ProtocolVersion);
            netWriter.Write(Flags);
            netWriter.Write(Priority);
        }
    }
}
