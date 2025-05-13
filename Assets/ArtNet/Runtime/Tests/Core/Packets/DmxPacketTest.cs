using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ArtNet.Packets;
using JetBrains.Annotations;
using NUnit.Framework;

namespace ArtNet.Tests.Core.Packets
{
    public class DmxPacketTest
    {
        [Test]
        public void TestDmxPacket()
        {
            var packet = new DmxPacket
            {
                Sequence = 1,
                Physical = 0,
                Universe = 1,
                Dmx = new byte[] { 101, 102, 103 }
            };

            Assert.AreEqual(3, packet.Length);
        }

        [Test]
        public void TestDmxPacketSerialization()
        {
            var shortOriginalPacket = new DmxPacket
            {
                Sequence = 123,
                Physical = 0,
                Universe = 1,
                Dmx = new byte[] { 101, 102, 103 }
            };

            var expectBytes = new byte[]
            {
                0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00, // "Art-Net\0"
                0x00, 0x50, // OpCode(Dmx)
                0x00, 0x0E, // Protocol Version
                0x7B, // Sequence
                0x00, // Physical
                0x01, 0x00, // Universe
                0x00, 0x03, // Length
                0x65, 0x66, 0x67 // DMX data
            };

            AssertDmxPacket(shortOriginalPacket, expectBytes);


            var longOriginalPacket = new DmxPacket
            {
                Sequence = 255,
                Physical = 0,
                Universe = 15,
                // 0xFF を 255 個の配列に変換
                Dmx = Enumerable.Repeat((byte) 0xFF, 512).ToArray()
            };
            expectBytes = new byte[]
            {
                0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00, // "Art-Net\0"
                0x00, 0x50, // OpCode(Dmx)
                0x00, 0x0E, // Protocol Version
                0xFF, // Sequence
                0x00, // Physical
                0x0F, 0x00, // Universe
                0x02, 0x00, // Length
                // DMX data (512 bytes of 0xFF)
            }.Concat(Enumerable.Repeat((byte) 0xFF, 512)).ToArray();

            AssertDmxPacket(longOriginalPacket, expectBytes);
        }

        private static void AssertDmxPacket([NotNull] DmxPacket packet, IEnumerable expected)
        {
            var serializedData = packet.ToByteArray();
            Assert.IsNotNull(serializedData);

            CollectionAssert.AreEqual(expected, serializedData);
            var deserializedPacket = ArtNetPacket.FromByteArray<DmxPacket>(serializedData);
            Assert.IsNotNull(deserializedPacket);

            Assert.AreEqual(packet.OpCode, deserializedPacket.OpCode);
            Assert.AreEqual(packet.Sequence, deserializedPacket.Sequence);
            Assert.AreEqual(packet.Physical, deserializedPacket.Physical);
            Assert.AreEqual(packet.Universe, deserializedPacket.Universe);
            Assert.AreEqual(packet.Length, deserializedPacket.Length);
            Assert.IsNotNull(deserializedPacket.Dmx);
            CollectionAssert.AreEqual(packet.Dmx, deserializedPacket.Dmx);
        }

        private static IEnumerable<TestCaseData> InvalidDmxPacketBytesTestCases
        {
            get
            {
                yield return new TestCaseData(new byte[]
                    {
                        0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00, // Invalid Art-Net ID
                        0x00, 0x50,
                        0x00, 0x0E,
                        0x7B,
                        0x00,
                        0x01, 0x00
                    })
                    .SetName("Missing DMX Body");
                yield return new TestCaseData(new byte[]
                    {
                        0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00,
                        0x00, 0x50,
                        0x00, 0x0E,
                        0x7B,
                        0x00,
                        0x01, 0x00,
                        0x00, 0x03,
                        0x01, 0x02
                    })
                    .SetName("DMX data length mismatch");
                yield return new TestCaseData((new byte[]
                    {
                        0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00,
                        0x00, 0x50,
                        0x00, 0x0E,
                        0x7B,
                        0x00,
                        0x01, 0x00,
                        0x02, 0x01
                    }).Concat(Enumerable.Repeat((byte) 0xFF, 257)).ToArray())
                    .SetName("DMX data length too long");
            }
        }

        [TestCaseSource(nameof(InvalidDmxPacketBytesTestCases))]
        public void TestInvalidDmxPacketBytes(byte[] invalidData)
        {
            var deserializedPacket = ArtNetPacket.FromByteArray<DmxPacket>(invalidData);
            Assert.IsNull(deserializedPacket);
        }

        private static IEnumerable<TestCaseData> InvalidDmxPacketDataTestCases
        {
            get
            {
                yield return new TestCaseData(new DmxPacket
                {
                    Sequence = 1,
                    Physical = 0,
                    Universe = 1,
                    Dmx = null
                })
                    .SetName("DMX data is null");
                yield return new TestCaseData(new DmxPacket
                {
                    Sequence = 1,
                    Physical = 0,
                    Universe = 1,
                    Dmx = Array.Empty<byte>()
                }).SetName("DMX data is empty");
                yield return new TestCaseData(new DmxPacket
                {
                    Sequence = 1,
                    Physical = 0,
                    Universe = 1,
                    Dmx = new byte[513]
                }).SetName("DMX data length is too long");
            }
        }

        [TestCaseSource(nameof(InvalidDmxPacketDataTestCases))]
        public void TestInvalidDmxPacketData(DmxPacket invalidData)
        {
            var serializedData = invalidData!.ToByteArray();
            Assert.IsNull(serializedData);
        }
    }
}
