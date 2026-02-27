using ArtNet.Enums;
using ArtNet.IO;

namespace ArtNet.Packets
{
    public class TodDataPacket : ArtNetPacket
    {
        public override OpCode OpCode => OpCode.TodData;
        protected override int MinimumBodyLength => 10;

        public byte RdmVersion { get; set; }
        public byte Port { get; set; }
        public byte BindIndex { get; set; }
        public byte Net { get; set; }
        public byte CommandResponse { get; set; }
        public byte Address { get; set; }
        public byte UidTotalHigh { get; set; }
        public byte UidTotalLow { get; set; }
        public byte BlockCount { get; set; }
        public byte UidCount { get; set; }
        public byte[] Tod { get; set; } = System.Array.Empty<byte>();

        protected override bool DeserializeBody(ArtNetReader artNetReader)
        {
            RdmVersion = artNetReader.ReadByte();
            Port = artNetReader.ReadByte();
            BindIndex = artNetReader.ReadByte();
            Net = artNetReader.ReadByte();
            CommandResponse = artNetReader.ReadByte();
            Address = artNetReader.ReadByte();
            UidTotalHigh = artNetReader.ReadByte();
            UidTotalLow = artNetReader.ReadByte();
            BlockCount = artNetReader.ReadByte();
            UidCount = artNetReader.ReadByte();

            var todLength = UidCount * 6;
            if (artNetReader.RemainingLength < todLength) return false;
            Tod = artNetReader.ReadBytes(todLength);
            return true;
        }

        protected override void SerializeBody(ArtNetWriter artNetWriter)
        {
            artNetWriter.Write(RdmVersion);
            artNetWriter.Write(Port);
            artNetWriter.Write(BindIndex);
            artNetWriter.Write(Net);
            artNetWriter.Write(CommandResponse);
            artNetWriter.Write(Address);
            artNetWriter.Write(UidTotalHigh);
            artNetWriter.Write(UidTotalLow);
            artNetWriter.Write(BlockCount);
            artNetWriter.Write(UidCount);
            artNetWriter.Write(Tod);
        }

        protected override bool Validate()
        {
            if (Tod == null) return false;
            if (Tod.Length % 6 != 0) return false;
            if (Tod.Length > 192) return false;
            return UidCount == Tod.Length / 6;
        }
    }
}
