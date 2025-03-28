using ArtNet.Enums;
using ArtNet.IO;

namespace ArtNet.Packets
{
    public class PollPacket : ArtNetPacket
    {
        public override OpCode OpCode => OpCode.Poll;

        public byte Flags { get; set; }
        public byte Priority { get; set; }


        protected override void DeserializeBody(ArtNetReader artNetReader)
        {
            Flags = artNetReader.ReadByte();
            Priority = artNetReader.ReadByte();
        }

        protected override void SerializeBody(ArtNetWriter artNetWriter)
        {
            artNetWriter.Write(Flags);
            artNetWriter.Write(Priority);
        }
    }
}
