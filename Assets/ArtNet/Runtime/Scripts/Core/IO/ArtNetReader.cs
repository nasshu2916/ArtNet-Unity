using System;
using System.Buffers.Binary;
using System.Text;

namespace ArtNet.IO
{
    public ref struct ArtNetReader
    {
        private readonly ReadOnlySpan<byte> _data;
        private int _position;

        internal ArtNetReader(ReadOnlySpan<byte> data)
        {
            _data = data;
            _position = 0;
        }

        internal int RemainingLength => _data.Length - _position;

        internal byte ReadByte()
        {
            var value = _data[_position];
            _position += 1;
            return value;
        }

        internal ushort ReadUInt16()
        {
            var value = BinaryPrimitives.ReadUInt16LittleEndian(_data[_position..]);
            _position += 2;
            return value;
        }

        internal ushort ReadNetworkUInt16()
        {
            var value = BinaryPrimitives.ReadUInt16BigEndian(_data[_position..]);
            _position += 2;
            return value;
        }

        internal byte[] ReadBytes(int length)
        {
            var value = _data.Slice(_position, length).ToArray();
            _position += length;
            return value;
        }

        internal string ReadString(int length)
        {
            var value = Encoding.ASCII.GetString(_data.Slice(_position, length));
            _position += length;
            return value;
        }
    }
}
