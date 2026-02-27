using ArtNet.Enums;
using ArtNet.IO;

namespace ArtNet.Packets
{
    public class RdmPacket : ArtNetPacket
    {
        public override OpCode OpCode => OpCode.Rdm;
        protected override int MinimumBodyLength => 5;

        public byte RdmVersion { get; set; }
        public byte Filler1 { get; set; }
        public byte Net { get; set; }
        public byte Command { get; set; }
        public byte Address { get; set; }
        public byte[] Data { get; set; } = System.Array.Empty<byte>();

        protected override bool DeserializeBody(ArtNetReader artNetReader)
        {
            RdmVersion = artNetReader.ReadByte();
            Filler1 = artNetReader.ReadByte();
            Net = artNetReader.ReadByte();
            Command = artNetReader.ReadByte();
            Address = artNetReader.ReadByte();
            Data = artNetReader.ReadBytes(artNetReader.RemainingLength);
            return true;
        }

        protected override void SerializeBody(ArtNetWriter artNetWriter)
        {
            artNetWriter.Write(RdmVersion);
            artNetWriter.Write(Filler1);
            artNetWriter.Write(Net);
            artNetWriter.Write(Command);
            artNetWriter.Write(Address);
            artNetWriter.Write(Data);
        }

        protected override bool Validate()
        {
            return Data != null;
        }
    }
}
