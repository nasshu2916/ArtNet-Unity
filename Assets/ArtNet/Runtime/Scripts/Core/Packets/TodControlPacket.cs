using ArtNet.Enums;
using ArtNet.IO;

namespace ArtNet.Packets
{
    public class TodControlPacket : ArtNetPacket
    {
        public override OpCode OpCode => OpCode.TodControl;
        protected override int MinimumBodyLength => 5;

        public byte RdmVersion { get; set; }
        public byte Filler1 { get; set; }
        public byte Net { get; set; }
        public byte Command { get; set; }
        public byte Address { get; set; }

        protected override bool DeserializeBody(ArtNetReader artNetReader)
        {
            RdmVersion = artNetReader.ReadByte();
            Filler1 = artNetReader.ReadByte();
            Net = artNetReader.ReadByte();
            Command = artNetReader.ReadByte();
            Address = artNetReader.ReadByte();
            return true;
        }

        protected override void SerializeBody(ArtNetWriter artNetWriter)
        {
            artNetWriter.Write(RdmVersion);
            artNetWriter.Write(Filler1);
            artNetWriter.Write(Net);
            artNetWriter.Write(Command);
            artNetWriter.Write(Address);
        }

        protected override bool Validate()
        {
            return true;
        }
    }
}
