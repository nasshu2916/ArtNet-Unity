using ArtNet.Enums;
using ArtNet.IO;

namespace ArtNet.Packets
{
    public class PollPacket : ArtNetPacket
    {
        public override OpCode OpCode => OpCode.Poll;

        protected override int MinimumBodyLength => 2;

        public byte Flags { get; set; }
        public byte Priority { get; set; }


        protected override bool DeserializeBody(ArtNetReader artNetReader)
        {
            Flags = artNetReader.ReadByte();
            Priority = artNetReader.ReadByte();
            return true;
        }

        protected override void SerializeBody(ArtNetWriter artNetWriter)
        {
            artNetWriter.Write(Flags);
            artNetWriter.Write(Priority);
        }

        protected override bool Validate()
        {
            return true;
        }
    }
}
