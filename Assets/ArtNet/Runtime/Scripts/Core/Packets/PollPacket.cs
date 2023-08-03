using System;
using ArtNet.Enums;

namespace ArtNet.Packets
{
    public class PollPacket : ArtNetPacket
    {
        public PollPacket() : base(OpCode.Poll)
        {
        }

        public PollPacket(ReadOnlySpan<byte> buffer) : base(buffer, OpCode.Poll)
        {
        }

        public byte Flags { get; set; }
        public byte Priority { get; set; }


        protected override void Deserialize(ArtNetReader artNetReader)
        {
            ProtocolVersion = artNetReader.ReadNetworkUInt16();
            Flags = artNetReader.ReadByte();
            Priority = artNetReader.ReadByte();
        }

        protected override void Serialize(ArtNetWriter artNetWriter)
        {
            base.Serialize(artNetWriter);
            artNetWriter.WriteNetwork(ProtocolVersion);
            artNetWriter.Write(Flags);
            artNetWriter.Write(Priority);
        }
    }
}
