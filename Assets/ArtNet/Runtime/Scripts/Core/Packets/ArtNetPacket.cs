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

        public abstract OpCode OpCode { get; }
        public static ushort ProtocolVersion => 14;
        public bool IsNeedProtocolVersion => OpCode != OpCode.PollReply;

        /// <summary>
        /// Creates an instance of the packet from a byte array.
        /// If the packet is not valid, it returns null.
        /// </summary>
        public static T FromByteArray<T>(ReadOnlySpan<byte> buffer, bool validate = true) where T : ArtNetPacket, new()
        {
            var packet = new T();

            if (validate)
            {
                if (!Validate(buffer)) return null;
                var opCode = GetOpCode(buffer.Slice(IdentificationIdsLength, 2));
                if (opCode != packet.OpCode) return null;
            }

            var result = packet.Deserialize(buffer);
            return result ? packet : null;
        }

        public byte[] ToByteArray()
        {
            using var memoryStream = new MemoryStream();
            Serialize(new ArtNetWriter(memoryStream));
            return memoryStream.ToArray();
        }

        private bool Deserialize(ReadOnlySpan<byte> buffer)
        {
            if (!Validate(buffer)) return false;

            var artReader = new ArtNetReader(buffer[FixedArtNetPacketLength..]);
            if (IsNeedProtocolVersion)
            {
                var protocolVersion = artReader.ReadNetworkUInt16();
                if (protocolVersion != ProtocolVersion) return false;
            }

            DeserializeBody(artReader);
            return true;
        }

        protected virtual void DeserializeBody(ArtNetReader artNetReader)
        {
        }

        private void Serialize(ArtNetWriter artNetWriter)
        {
            SerializeHeader(artNetWriter);
            SerializeBody(artNetWriter);
        }

        private void SerializeHeader(ArtNetWriter artNetWriter)
        {
            artNetWriter.WriteNetwork(ArtNetId, 8);
            artNetWriter.Write((ushort) OpCode);
            if (IsNeedProtocolVersion)
            {
                artNetWriter.WriteNetwork(ProtocolVersion);
            }
        }

        protected abstract void SerializeBody(ArtNetWriter artNetWriter);


        [CanBeNull]
        public static ArtNetPacket Create(ReadOnlySpan<byte> buffer)
        {
            if (!Validate(buffer)) return null;

            return GetOpCode(buffer.Slice(IdentificationIdsLength, 2)) switch
            {
                OpCode.Poll => FromByteArray<PollPacket>(buffer, false),
                OpCode.PollReply => FromByteArray<PollReplyPacket>(buffer, false),
                OpCode.Dmx => FromByteArray<DmxPacket>(buffer, false),
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
