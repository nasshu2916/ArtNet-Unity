using System;
using System.IO;
using System.Text;
using ArtNet.Enums;
using ArtNet.IO;
using JetBrains.Annotations;

namespace ArtNet.Packets
{
    public abstract class ArtNetPacket
    {
        private const string ArtNetId = "Art-Net\0";
        private const byte FixedArtNetPacketLength = 10;
        private static readonly byte[] IdentificationIds = Encoding.ASCII.GetBytes(ArtNetId);
        private static readonly byte IdentificationIdsLength = (byte) IdentificationIds.Length;

        protected ArtNetPacket(OpCode opCode)
        {
            OpCode = opCode;
        }

        protected ArtNetPacket(ReadOnlySpan<byte> buffer, OpCode opCode) : this(opCode)
        {
            var artReader = new ArtNetReader(buffer[FixedArtNetPacketLength..]);
            Deserialize(artReader);
        }

        public OpCode OpCode { get; }
        public ushort ProtocolVersion { get; protected set; } = 14;

        public byte[] ToByteArray()
        {
            using var memoryStream = new MemoryStream();
            Serialize(new ArtNetWriter(memoryStream));
            return memoryStream.ToArray();
        }

        protected virtual void Deserialize(ArtNetReader artNetReader)
        {
        }

        protected virtual void Serialize(ArtNetWriter artNetWriter)
        {
            artNetWriter.WriteNetwork(ArtNetId, 8);
            artNetWriter.Write((ushort) OpCode);
        }

        [CanBeNull]
        public static ArtNetPacket Create(ReadOnlySpan<byte> buffer)
        {
            if (!Validate(buffer)) return null;

            return GetOpCode(buffer.Slice(IdentificationIdsLength, 2)) switch
            {
                OpCode.Poll => new PollPacket(buffer),
                OpCode.PollReply => new PollReplyPacket(buffer),
                OpCode.Dmx => new DmxPacket(buffer),
                _ => null
            };
        }

        private static bool Validate(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length < FixedArtNetPacketLength) return false;
            for (var i = 0; i < IdentificationIdsLength; i++)
            {
                if (buffer[i] != IdentificationIds[i]) return false;
            }

            return true;
        }

        private static OpCode GetOpCode(ReadOnlySpan<byte> buffer) =>
            (OpCode) (buffer[0] + (buffer[1] << 8));
    }
}
