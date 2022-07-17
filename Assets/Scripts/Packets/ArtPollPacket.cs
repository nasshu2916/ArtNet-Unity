using ArtNet.Enums;
using ArtNet.IO;
using ArtNet.Sockets;

namespace ArtNet.Packets
{
    public class ArtPollPacket : ArtPacket
    {
        public ArtPollPacket() : base(OpCode.Poll)
        {
        }

        public ArtPollPacket(ReceivedData data) : base(data)
        {
        }

        public byte Flags { get; private set; }
        public byte Priority { get; private set; }


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
