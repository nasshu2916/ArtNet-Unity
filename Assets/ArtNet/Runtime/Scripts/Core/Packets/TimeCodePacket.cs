using ArtNet.Enums;
using ArtNet.IO;

namespace ArtNet.Packets
{
    public class TimeCodePacket : ArtNetPacket
    {
        public override OpCode OpCode => OpCode.TimeCode;
        protected override int MinimumBodyLength => 5;

        public byte Frames { get; set; }
        public byte Seconds { get; set; }
        public byte Minutes { get; set; }
        public byte Hours { get; set; }
        public byte Type { get; set; }

        protected override bool DeserializeBody(ArtNetReader artNetReader)
        {
            Frames = artNetReader.ReadByte();
            Seconds = artNetReader.ReadByte();
            Minutes = artNetReader.ReadByte();
            Hours = artNetReader.ReadByte();
            Type = artNetReader.ReadByte();
            return true;
        }

        protected override void SerializeBody(ArtNetWriter artNetWriter)
        {
            artNetWriter.Write(Frames);
            artNetWriter.Write(Seconds);
            artNetWriter.Write(Minutes);
            artNetWriter.Write(Hours);
            artNetWriter.Write(Type);
        }

        protected override bool Validate()
        {
            return true;
        }
    }
}
