using ArtNet.Enums;
using ArtNet.IO;

namespace ArtNet.Packets
{
    public class ArtPollPacket : ArtPacket
    {
        public ArtPollPacket() : base(OpCode.Poll)
        {
        }

        public ArtPollPacket(byte[] buffer) : base(buffer, OpCode.Poll)
        {
        }

        public byte Flags { get; set; }
        public byte Priority { get; set; }


        protected override void ReadData(ArtReader reader)
        {
            ProtocolVersion = reader.ReadNetworkUInt16();
            Flags = reader.ReadByte();
            Priority = reader.ReadByte();
        }

        protected override void WriteData(ArtWriter writer)
        {
            base.WriteData(writer);
            writer.WriteNetwork(ProtocolVersion);
            writer.Write(Flags);
            writer.Write(Priority);
        }
    }
}
