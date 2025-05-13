using ArtNet.Enums;
using ArtNet.IO;

namespace ArtNet.Packets
{
    public class DmxPacket : ArtNetPacket
    {
        public override OpCode OpCode => OpCode.Dmx;
        protected override int MinimumBodyLength => 7;

        public byte Sequence { get; set; }
        public byte Physical { get; set; }
        public ushort Universe { get; set; }

        public ushort Length => Dmx == null ? (ushort) 0 : (ushort) Dmx.Length;

        public byte[] Dmx { get; set; }

        protected override bool DeserializeBody(ArtNetReader artNetReader)
        {
            Sequence = artNetReader.ReadByte();
            Physical = artNetReader.ReadByte();
            Universe = artNetReader.ReadUInt16();
            int length = artNetReader.ReadNetworkUInt16();
            if (length > 512) return false;
            if (artNetReader.RemainingLength < length) return false;
            Dmx = artNetReader.ReadBytes(length);

            return true;
        }

        protected override void SerializeBody(ArtNetWriter artNetWriter)
        {
            artNetWriter.Write(Sequence);
            artNetWriter.Write(Physical);
            artNetWriter.Write(Universe);
            artNetWriter.WriteNetwork(Length);
            artNetWriter.Write(Dmx);
        }

        protected override bool Validate()
        {
            return Length is <= 512 and > 0;
        }
    }
}
