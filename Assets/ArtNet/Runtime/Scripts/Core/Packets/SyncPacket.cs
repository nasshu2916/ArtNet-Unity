using ArtNet.Enums;
using ArtNet.IO;

namespace ArtNet.Packets
{
    public class SyncPacket : ArtNetPacket
    {
        public override OpCode OpCode => OpCode.Sync;
        protected override int MinimumBodyLength => 2;

        public byte Aux1 { get; private set; }
        public byte Aux2 { get; private set; }

        protected override bool DeserializeBody(ArtNetReader artNetReader)
        {
            Aux1 = artNetReader.ReadByte();
            Aux2 = artNetReader.ReadByte();
            return true;
        }

        protected override void SerializeBody(ArtNetWriter artNetWriter)
        {
            // ArtSync の Aux フィールドは仕様上 0 送信とする。
            artNetWriter.Write((byte) 0);
            artNetWriter.Write((byte) 0);
        }

        protected override bool Validate()
        {
            return true;
        }
    }
}
