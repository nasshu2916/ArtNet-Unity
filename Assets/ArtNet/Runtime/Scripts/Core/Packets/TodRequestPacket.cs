using ArtNet.Enums;
using ArtNet.IO;

namespace ArtNet.Packets
{
    public class TodRequestPacket : ArtNetPacket
    {
        public override OpCode OpCode => OpCode.TodRequest;
        protected override int MinimumBodyLength => 37;

        public byte RdmVersion { get; set; }
        public byte Filler1 { get; set; }
        public byte Net { get; set; }
        public byte Command { get; set; }
        public byte AddressCount { get; set; }
        public byte[] Addresses { get; set; } = new byte[32];

        protected override bool DeserializeBody(ArtNetReader artNetReader)
        {
            RdmVersion = artNetReader.ReadByte();
            Filler1 = artNetReader.ReadByte();
            Net = artNetReader.ReadByte();
            Command = artNetReader.ReadByte();
            AddressCount = artNetReader.ReadByte();
            Addresses = artNetReader.ReadBytes(32);
            return true;
        }

        protected override void SerializeBody(ArtNetWriter artNetWriter)
        {
            artNetWriter.Write(RdmVersion);
            artNetWriter.Write(Filler1);
            artNetWriter.Write(Net);
            artNetWriter.Write(Command);
            artNetWriter.Write(AddressCount);
            artNetWriter.Write(Addresses);
        }

        protected override bool Validate()
        {
            return Addresses?.Length == 32;
        }
    }
}
