using ArtNet.Enums;
using ArtNet.IO;

namespace ArtNet.Packets
{
    public class AddressPacket : ArtNetPacket
    {
        public override OpCode OpCode => OpCode.Address;
        protected override int MinimumBodyLength => 96;

        public byte NetSwitch { get; set; }
        public byte BindIndex { get; set; }
        public string ShortName { get; set; } = string.Empty;
        public string LongName { get; set; } = string.Empty;
        public byte[] SwIn { get; set; } = new byte[4];
        public byte[] SwOut { get; set; } = new byte[4];
        public byte SubSwitch { get; set; }
        public byte SwVideo { get; set; }
        public byte Command { get; set; }
        public byte Filler { get; set; }

        protected override bool DeserializeBody(ArtNetReader artNetReader)
        {
            NetSwitch = artNetReader.ReadByte();
            BindIndex = artNetReader.ReadByte();
            ShortName = artNetReader.ReadString(18);
            LongName = artNetReader.ReadString(64);
            SwIn = artNetReader.ReadBytes(4);
            SwOut = artNetReader.ReadBytes(4);
            SubSwitch = artNetReader.ReadByte();
            SwVideo = artNetReader.ReadByte();
            Command = artNetReader.ReadByte();
            Filler = artNetReader.ReadByte();
            return true;
        }

        protected override void SerializeBody(ArtNetWriter artNetWriter)
        {
            artNetWriter.Write(NetSwitch);
            artNetWriter.Write(BindIndex);
            artNetWriter.WriteNetwork(ShortName, 18);
            artNetWriter.WriteNetwork(LongName, 64);
            artNetWriter.Write(SwIn);
            artNetWriter.Write(SwOut);
            artNetWriter.Write(SubSwitch);
            artNetWriter.Write(SwVideo);
            artNetWriter.Write(Command);
            artNetWriter.Write(Filler);
        }

        protected override bool Validate()
        {
            return SwIn?.Length == 4 && SwOut?.Length == 4;
        }
    }
}
