using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ArtNet.IO;
using JetBrains.Annotations;

namespace ArtNet.Packets
{
    public abstract class ArtNetPacket
    {
        private const string ArtNetId = "Art-Net\0";
        private const byte FixedArtNetPacketLength = 10;
        private static readonly byte[] IdentificationIds = Encoding.ASCII.GetBytes(ArtNetId);

        protected ArtNetPacket(Enums.OpCode opCode)
        {
            OpCode = opCode;
        }

        protected ArtNetPacket(byte[] buffer, Enums.OpCode opCode) : this(opCode)
        {
            using var memoryStream = new MemoryStream(buffer);
            using var artReader = new ArtNetReaderOld(memoryStream);
            memoryStream.Position = FixedArtNetPacketLength;
            ReadData(artReader);
        }

        public Enums.OpCode OpCode { get; }
        public ushort ProtocolVersion { get; protected set; } = 14;

        public byte[] ToByteArray()
        {
            using var memoryStream = new MemoryStream();
            WriteData(new ArtNetWriter(memoryStream));
            return memoryStream.ToArray();
        }

        protected virtual void ReadData(ArtNetReaderOld netReader)
        {
        }

        protected virtual void WriteData(ArtNetWriter netWriter)
        {
            netWriter.WriteNetwork(ArtNetId, 8);
            netWriter.Write((ushort) OpCode);
        }

        [CanBeNull]
        public static ArtNetPacket Create(byte[] buffer)
        {
            if (!Validate(buffer)) return null;

            return GetOpCode(buffer) switch
            {
                Enums.OpCode.Poll => new PollPacket(buffer),
                Enums.OpCode.PollReply => new PollReplyPacket(buffer),
                Enums.OpCode.Dmx => new DmxPacket(buffer),
                _ => null
            };
        }

        private static bool Validate(IReadOnlyList<byte> buffer)
        {
            if (buffer.Count < FixedArtNetPacketLength) return false;
            return !IdentificationIds.Where((t, i) => buffer[i] != t).Any();
        }

        private static Enums.OpCode GetOpCode(IReadOnlyList<byte> buffer) =>
            (Enums.OpCode) (buffer[8] + (buffer[9] << 8));
    }
}
