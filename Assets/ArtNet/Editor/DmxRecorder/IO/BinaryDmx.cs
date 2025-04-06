using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder.IO
{
    public static class BinaryDmx
    {
        public enum CompressType
        {
            None = 0,
            Deflate = 1
        }

        public class Header
        {
            [NotNull] private static readonly byte[] Identifiers = { 0xFF, 0x44, 0x4D, 0x58 };
            private const byte ReservedBufferLength = 10;
            private const byte Version = 0x02;

            public CompressType BodyCompressType { get; }

            private static int IdentifierLength => Identifiers.Length;
            public static int Length => IdentifierLength + 2 + ReservedBufferLength;

            public Header(CompressType compressType = CompressType.None)
            {
                BodyCompressType = compressType;
            }

            [NotNull]
            public byte[] SerializeHeader()
            {
                using var memoryStream = new MemoryStream();
                memoryStream.Write(Identifiers);
                memoryStream.WriteByte(Version);
                memoryStream.WriteByte((byte) BodyCompressType);
                memoryStream.Write(new byte[ReservedBufferLength]);
                return memoryStream.ToArray();
            }

            public static Header DeserializeHeader(ReadOnlySpan<byte> data)
            {
                if (data.Length < Length) return null;
                if (!data[..Identifiers.Length].SequenceEqual(Identifiers)) return null;
                var position = IdentifierLength;
                var dataVersion = data[position++];
                if (dataVersion != Version) return null;

                var compressType = (CompressType) data[position++];
                if (typeof(CompressType).IsEnumDefined(compressType) == false) return null;

                return new Header(compressType: compressType);
            }
        }

        public static void Export(IEnumerable<UniverseData> universeData, string path, bool isCompress)
        {
            var binary = SerializeUniverseData(universeData, isCompress);
            File.WriteAllBytes(path, binary);
        }

        public static byte[] SerializeUniverseData(IEnumerable<UniverseData> universeData, bool isCompress)
        {
            var sortedData = universeData.Where(x => x != null).OrderBy(x => (x.Time, x.Universe)).ToList();
            var startTime = sortedData.First().Time;

            var compressType = isCompress ? CompressType.Deflate : CompressType.None;
            var header = new Header(compressType: compressType);
            var headerArray = header.SerializeHeader();
            var bodyArray = SerializeBody(sortedData, startTime);

            if (isCompress)
            {
                using var memoryStream = new MemoryStream();
                using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
                {
                    deflateStream.Write(bodyArray, 0, bodyArray.Length);
                }

                bodyArray = memoryStream.ToArray();
            }

            var result = new byte[headerArray.Length + bodyArray.Length];

            Buffer.BlockCopy(headerArray, 0, result, 0, headerArray.Length);
            Buffer.BlockCopy(bodyArray, 0, result, headerArray.Length, bodyArray.Length);

            return result;
        }

        public static List<UniverseData> Deserialize(ReadOnlySpan<byte> data)
        {
            if (data.Length < Header.Length)
            {
                Debug.LogError("ArtNet Recorder: Invalid data length");
                return null;
            }

            var header = Header.DeserializeHeader(data[..Header.Length]);
            if (header == null) return null;

            var body = data[Header.Length..];

            switch (header.BodyCompressType)
            {
                case CompressType.None:
                    break;
                case CompressType.Deflate:
                    {
                        using var compressedStream = new MemoryStream(body.ToArray()!);
                        using var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
                        using var memoryStream = new MemoryStream();
                        deflateStream.CopyTo(memoryStream);

                        body = memoryStream.ToArray();
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return DeserializeBody(body);
        }

        [NotNull]
        private static byte[] SerializeBody([NotNull] IEnumerable<UniverseData> universeData, long startTime)
        {
            using var memoryStream = new MemoryStream();

            foreach (var data in universeData)
            {
                var time = data!.Time - startTime;
                memoryStream.Write(BitConverter.GetBytes(time));
                memoryStream.Write(BitConverter.GetBytes(data.Universe));
                var length = data.Length;
                memoryStream.Write(BitConverter.GetBytes(length));
                memoryStream.Write(data.Values![..length]);
            }

            return memoryStream.ToArray();
        }

        private static List<UniverseData> DeserializeBody(ReadOnlySpan<byte> body)
        {
            var position = 0;
            var dataLength = body.Length;
            var result = new List<UniverseData>();
            while (position < dataLength - 12)
            {
                var time = BitConverter.ToInt64(body[position..]);
                position += 8;
                var universe = BitConverter.ToUInt16(body[position..]);
                position += 2;
                var length = BitConverter.ToUInt16(body[position..]);
                position += 2;
                if (position + length > dataLength || length > 512)
                {
                    return null;
                }

                var dmx = body[position..(position + length)];
                position += length;
                result.Add(new UniverseData(time, universe, dmx));
            }

            return result.OrderBy(x => (x!.Time, x!.Universe)).ToList();
        }
    }
}
