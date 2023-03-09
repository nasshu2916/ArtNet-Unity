using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ArtNet.IO;
using ArtNet.Sockets;
using JetBrains.Annotations;

namespace ArtNet.Packets
{
    public abstract class ArtPacket
    {
        private const string ArtNetId = "Art-Net\0";
        private const byte FixedArtNetPacketLength = 10;
        private static readonly byte[] IdentificationIds = Encoding.ASCII.GetBytes(ArtNetId);

        protected ArtPacket(Enums.OpCode opCode)
        {
            OpCode = opCode;
        }

        protected ArtPacket(ReceivedData data)
        {
            OpCode = data.OpCode;
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
            writer.WriteNetwork(ArtNetId, 8);
            writer.Write((ushort) OpCode);
        }

        [CanBeNull]
        public static ArtPacket Create(ReceivedData data)
        {
            if (!Validate(data.Buffer)) return null;

            return data.OpCode switch
            {
                Enums.OpCode.Poll => new ArtPollPacket(data),
                Enums.OpCode.PollReply => new ArtPollReplyPacket(data),
                Enums.OpCode.Dmx => new ArtDmxPacket(data),
                _ => null
            };
        }

        private static bool Validate(IReadOnlyList<byte> buffer)
        {
            if (buffer.Count < FixedArtNetPacketLength) return false;
            return !IdentificationIds.Where((t, i) => buffer[i] != t).Any();
        }
    }
}
