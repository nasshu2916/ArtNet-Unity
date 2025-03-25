using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace ArtNet.Editor.DmxRecorder.IO
{
    public static class BinaryDmx
    {
        private const byte IdentifierLength = 4;
        [NotNull] private static readonly byte[] Identifiers = { 0xFF, 0x44, 0x4D, 0x58 };
        [NotNull] private static readonly byte[] ReservedBuffer = new byte[10];
        private const byte Version = 0x02;

        public static void Export(IEnumerable<UniverseData> universeData, string path)
        {
            var binary = SerializeUniverseData(universeData, true);
            File.WriteAllBytes(path, binary);
        }


        public static byte[] SerializeUniverseData(IEnumerable<UniverseData> universeData, bool isCompress = false)
        {
            var sortedData = universeData.Where(x => x != null).OrderBy(x => x.Time).ToList();
            var startTime = sortedData.First().Time;
            using var headerMemoryStream = new MemoryStream();
            headerMemoryStream.Write(Identifiers);
            headerMemoryStream.WriteByte(Version);
            headerMemoryStream.WriteByte((byte) (isCompress ? 1 : 0));
            headerMemoryStream.Write(ReservedBuffer);

            var header = headerMemoryStream.ToArray();
            var body = SerializeBody(sortedData, startTime);

            if (isCompress)
            {
                using var memoryStream = new MemoryStream();
                using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
                {
                    deflateStream.Write(body, 0, body.Length);
                }
                body = memoryStream.ToArray();
            }

            var result = new byte[header.Length + body.Length];

            Buffer.BlockCopy(header, 0, result, 0, header.Length);
            Buffer.BlockCopy(body, 0, result, header.Length, body.Length);

            return result;
        }

        public static List<UniverseData> Deserialize(ReadOnlySpan<byte> data)
        {
            var dataLength = data.Length;
            if (dataLength < Identifiers.Length || !data[..Identifiers.Length].SequenceEqual(Identifiers))
                return null;
            var position = (int) IdentifierLength;
            var dataVersion = data[position++];
            if (dataVersion != Version)
            {
                Debug.LogError($"ArtNet Recorder: Version mismatch. Required: {Version}, Found: {dataVersion}");
                return null;
            }

            var isCompressed = data[position++] == 1;

            position += ReservedBuffer.Length;
            var body = data[position..];

            if (isCompressed)
            {
                using var compressedStream = new MemoryStream(body.ToArray());
                using var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
                using var memoryStream = new MemoryStream();
                deflateStream.CopyTo(memoryStream);

                body = memoryStream.ToArray();
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
                    Debug.LogError("ArtNet Recorder: Invalid data length");
                    return null;
                }

                var dmx = body[position..(position + length)];
                position += length;
                result.Add(new UniverseData(time, universe, dmx));
            }

            return result;
        }
    }
}
