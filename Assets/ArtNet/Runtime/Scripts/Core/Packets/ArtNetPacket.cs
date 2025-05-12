using System;
using System.IO;
using System.Text;
using ArtNet.Common;
using ArtNet.Enums;
using ArtNet.IO;

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

        private int HeaderLength => FixedArtNetPacketLength + (IsNeedProtocolVersion ? 2 : 0);
        protected abstract int MinimumBodyLength { get; }

        public int MinimumPacketLength => HeaderLength + MinimumBodyLength;

        /// <summary>
        /// Creates an instance of the packet from a byte array.
        /// If the packet is not valid, it returns null.
        /// </summary>
        public static T FromByteArray<T>(ReadOnlySpan<byte> buffer, bool validateOpCode = true)
            where T : ArtNetPacket, new()
        {
            var packet = new T();

            if (validateOpCode)
            {
                var opCode = ArtNetOpCode(buffer);
                if (opCode != packet.OpCode) return null;
            }

            var result = packet.Deserialize(buffer);
            return result ? packet : null;
        }

        public byte[] ToByteArray()
        {
            if (Validate() == false) return null;

            using var memoryStream = new MemoryStream();
            Serialize(new ArtNetWriter(memoryStream));
            return memoryStream.ToArray();
        }

        private bool Deserialize(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length < MinimumPacketLength) return false;

            var artReader = new ArtNetReader(buffer[FixedArtNetPacketLength..]);
            if (IsNeedProtocolVersion)
            {
                var protocolVersion = artReader.ReadNetworkUInt16();
                if (protocolVersion != ProtocolVersion) return false;
            }

            return DeserializeBody(artReader);
        }

        protected abstract bool DeserializeBody(ArtNetReader artNetReader);

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
        protected abstract bool Validate();


        /// <summary>
        /// Creates an instance of the packet from a byte array.
        /// If the packet is not valid, it returns null.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns>An instance of the packet or null if the packet is not valid.</returns>
        public static ArtNetPacket Create(ReadOnlySpan<byte> buffer)
        {
            var opCode = ArtNetOpCode(buffer);
            if (opCode == null) return null;
            if (Enum.IsDefined(typeof(OpCode), opCode) == false) return null;

            return opCode switch
            {
                OpCode.Poll => FromByteArray<PollPacket>(buffer, false),
                OpCode.PollReply => FromByteArray<PollReplyPacket>(buffer, false),
                OpCode.Dmx => FromByteArray<DmxPacket>(buffer, false),
                _ => throw new ArgumentOutOfRangeException(nameof(opCode), opCode, "OpCode not supported")
            };
        }

        /// <summary>
        /// Get Art-Net OpCode from the buffer.
        /// If the buffer is not a valid Art-Net packet or not support the OpCode, return null.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private static OpCode? ArtNetOpCode(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length < FixedArtNetPacketLength) return null;
            for (var i = 0; i < IdentificationIdsLength; i++)
            {
                if (buffer[i] != IdentificationIds[i]) return null;
            }

            var pos = IdentificationIdsLength;
            return (OpCode) (buffer[pos++] + (buffer[pos] << 8));
        }
    }
}
