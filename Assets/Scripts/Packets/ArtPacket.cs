using System.IO;
using System.Net;
using ArtNet.IO;
using ArtNet.Sockets;

namespace ArtNet.Packets
{
    public abstract class ArtPacket
    {
        private const byte FixedArtNetPacketLength = 10;

        protected ArtPacket(ReceivedData data)
        {
            RemoteAddress = data.RemoteAddress;
            OpCode = (Enums.OpCode)data.OpCode;
            using var memoryStream = new MemoryStream(data.Buffer);
            using var artReader = new ArtReader(memoryStream);
            memoryStream.Position = FixedArtNetPacketLength;
            ReadData(artReader);
        }

        public IPAddress RemoteAddress { get; }

        public Enums.OpCode OpCode { get; }
        public ushort ProtocolVersion { get; protected set; } = 14;

        protected virtual void ReadData(ArtReader reader)
        {
        }
    }
}
