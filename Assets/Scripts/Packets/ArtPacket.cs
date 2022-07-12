using System.IO;
using System.Net;
using ArtNet.IO;
using ArtNet.Sockets;

namespace ArtNet.Packets
{
    public abstract class ArtPacket
    {
        private const byte FixedArtNetPacketLength = 10;

        protected ArtPacket(Enums.OpCode opCode)
        {
            OpCode = opCode;
        }

        protected ArtPacket(ReceivedData data)
        {
            OpCode = (Enums.OpCode)data.OpCode;
            using var memoryStream = new MemoryStream(data.Buffer);
            using var artReader = new ArtReader(memoryStream);
            memoryStream.Position = FixedArtNetPacketLength;
            ReadData(artReader);
        }

        public Enums.OpCode OpCode { get; }
        public ushort ProtocolVersion { get; protected set; } = 14;

        public byte[] ToByteArray()
        {
            using var memoryStream = new MemoryStream();
            WriteData(new ArtWriter(memoryStream));
            return memoryStream.ToArray();
        }

        protected virtual void ReadData(ArtReader reader)
        {
        }

        protected virtual void WriteData(ArtWriter writer)
        {
            writer.WriteNetwork("Art-Net\0", 8);
            writer.Write((ushort)OpCode);
        }
    }
}
