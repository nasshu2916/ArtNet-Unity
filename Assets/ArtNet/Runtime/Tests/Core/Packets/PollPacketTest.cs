using System.Collections;
using System.Collections.Generic;
using ArtNet.Packets;
using JetBrains.Annotations;
using NUnit.Framework;

namespace ArtNet.Tests.Core.Packets
{
    public class PollPacketTest
    {
        [Test]
        public void TestPollPacketSerialization()
        {
            var tests = new[]
            {
                new
                {
                    Packet = new PollPacket
                    {
                        Flags = 0,
                        Priority = 0
                    },
                    Expected = new byte[]
                    {
                        0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00, // "Art-Net\0"
                        0x00, 0x20, // OpCode(Poll)
                        0x00, 0x0E, // Protocol Version
                        0x00, // Flags
                        0x00 // Priority
                    }
                },
                new
                {
                    Packet = new PollPacket
                    {
                        Flags = 0x04,
                        Priority = 0x10
                    },
                    Expected = new byte[]
                    {
                        0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00, // "Art-Net\0"
                        0x00, 0x20, // OpCode(Poll)
                        0x00, 0x0E, // Protocol Version
                        0x04, // Flags
                        0x10 // Priority
                    }
                }
            };

            foreach (var test in tests)
            {
                var packet = test.Packet;
                var expected = test.Expected;

                AssertPollPacket(packet, expected);
            }
        }

        private static void AssertPollPacket([NotNull] PollPacket packet, IEnumerable expected)
        {
            var serializedData = packet.ToByteArray();
            Assert.IsNotNull(serializedData);

            CollectionAssert.AreEqual(expected, serializedData);
            var deserializedPacket = ArtNetPacket.FromByteArray<PollPacket>(serializedData);
            Assert.IsNotNull(deserializedPacket);

            Assert.AreEqual(packet.OpCode, deserializedPacket.OpCode);
            Assert.AreEqual(packet.IsNeedProtocolVersion, deserializedPacket.IsNeedProtocolVersion);
            Assert.AreEqual(packet.Flags, deserializedPacket.Flags);
            Assert.AreEqual(packet.Priority, deserializedPacket.Priority);
        }

        private static IEnumerable<TestCaseData> InvalidPollPacketBytesTestCases
        {
            get
            {
                yield return new TestCaseData(new byte[]
                    {
                        0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00,
                        0x00, 0x20,
                        0x00, 0x0E,
                        0x00
                    })
                    .SetName("Body too short");
            }
        }

        [TestCaseSource(nameof(InvalidPollPacketBytesTestCases))]
        public void TestInvalidPollPacketBytes(byte[] invalidData)
        {
            var deserializedPacket = ArtNetPacket.FromByteArray<PollPacket>(invalidData);
            Assert.IsNull(deserializedPacket);
        }
    }
}
