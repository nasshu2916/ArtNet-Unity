using System;
using System.Buffers.Binary;
using ArtNet.Enums;
using ArtNet.IO;

namespace ArtNet.Packets
{
    public class DmxPacket : ArtNetPacket
    {
        private const int DmxHeaderOffset = 12;
        private const int MinimumDmxPacketLength = 19;
        private readonly byte[] _dmxBuffer = new byte[512];
        private int _dmxLength = -1;

        public override OpCode OpCode => OpCode.Dmx;
        protected override int MinimumBodyLength => 7;

        public byte Sequence { get; set; }
        public byte Physical { get; set; }
        public ushort Universe { get; set; }

        public ushort Length => _dmxLength > 0 ? (ushort) _dmxLength : (ushort) 0;
        public ReadOnlySpan<byte> DmxSpan => _dmxLength > 0 ? _dmxBuffer.AsSpan(0, Math.Min(_dmxLength, _dmxBuffer.Length)) : ReadOnlySpan<byte>.Empty;

        public byte[] Dmx
        {
            get => _dmxLength < 0 ? null : DmxSpan.ToArray();
            set
            {
                if (value == null)
                {
                    _dmxLength = -1;
                    return;
                }

                _dmxLength = value.Length;
                var copyLength = Math.Min(value.Length, _dmxBuffer.Length);
                value.AsSpan(0, copyLength).CopyTo(_dmxBuffer);
            }
        }

        public static bool TryParse(ReadOnlySpan<byte> buffer, out DmxPacket packet)
        {
            packet = null;
            if (buffer.Length < MinimumDmxPacketLength) return false;

            var protocolVersion = BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(10, 2));
            if (protocolVersion != ProtocolVersion) return false;

            var sequence = buffer[DmxHeaderOffset];
            var physical = buffer[DmxHeaderOffset + 1];
            var universe = BinaryPrimitives.ReadUInt16LittleEndian(buffer.Slice(DmxHeaderOffset + 2, 2));
            var dmxLength = BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(DmxHeaderOffset + 4, 2));
            if (512 < dmxLength) return false;
            if (buffer.Length < DmxHeaderOffset + 6 + dmxLength) return false;

            packet = new DmxPacket
            {
                Sequence = sequence,
                Physical = physical,
                Universe = universe,
                _dmxLength = dmxLength
            };
            buffer.Slice(DmxHeaderOffset + 6, dmxLength).CopyTo(packet._dmxBuffer);
            return true;
        }

        protected override bool DeserializeBody(ArtNetReader artNetReader)
        {
            Sequence = artNetReader.ReadByte();
            Physical = artNetReader.ReadByte();
            Universe = artNetReader.ReadUInt16();
            int length = artNetReader.ReadNetworkUInt16();
            if (length > 512) return false;
            if (artNetReader.RemainingLength < length) return false;
            artNetReader.ReadBytesTo(_dmxBuffer, length);
            _dmxLength = length;

            return true;
        }

        protected override void SerializeBody(ArtNetWriter artNetWriter)
        {
            artNetWriter.Write(Sequence);
            artNetWriter.Write(Physical);
            artNetWriter.Write(Universe);
            artNetWriter.WriteNetwork(Length);
            artNetWriter.Write(_dmxBuffer, 0, _dmxLength);
        }

        protected override bool Validate()
        {
            return Length is <= 512 and > 0;
        }
    }
}
