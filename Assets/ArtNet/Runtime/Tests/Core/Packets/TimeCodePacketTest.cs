using System.Collections;
using ArtNet.Packets;
using JetBrains.Annotations;
using NUnit.Framework;

namespace ArtNet.Tests.Core.Packets
{
    public class TimeCodePacketTest
    {
        [Test]
        public void TestTimeCodePacketSerialization()
        {
            var packet = new TimeCodePacket
            {
                Frames = 0x18,
                Seconds = 0x38,
                Minutes = 0x2A,
                Hours = 0x0D,
                Type = 0x01
            };
            AssertTimeCodePacket(packet, new byte[]
            {
                0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00, // "Art-Net\0"
                0x00, 0x97, // OpCode(TimeCode)
                0x00, 0x0E, // Protocol Version
                0x18, // Frames
                0x38, // Seconds
                0x2A, // Minutes
                0x0D, // Hours
                0x01 // Type
            });
        }

        private static void AssertTimeCodePacket([NotNull] TimeCodePacket packet, IEnumerable expected)
        {
            var serializedData = packet.ToByteArray();
            Assert.IsNotNull(serializedData);

            CollectionAssert.AreEqual(expected, serializedData);
            var deserializedPacket = ArtNetPacket.FromByteArray<TimeCodePacket>(serializedData);
            Assert.IsNotNull(deserializedPacket);

            Assert.AreEqual(packet.OpCode, deserializedPacket.OpCode);
            Assert.AreEqual(packet.IsNeedProtocolVersion, deserializedPacket.IsNeedProtocolVersion);
            Assert.AreEqual(packet.Frames, deserializedPacket.Frames);
            Assert.AreEqual(packet.Seconds, deserializedPacket.Seconds);
            Assert.AreEqual(packet.Minutes, deserializedPacket.Minutes);
            Assert.AreEqual(packet.Hours, deserializedPacket.Hours);
            Assert.AreEqual(packet.Type, deserializedPacket.Type);
        }

        [Test]
        public void TestInvalidTimeCodePacketBytes()
        {
            var invalidData = new byte[]
            {
                0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00,
                0x00, 0x97,
                0x00, 0x0E,
                0x18,
                0x38,
                0x2A
            };

            var deserializedPacket = ArtNetPacket.FromByteArray<TimeCodePacket>(invalidData);
            Assert.IsNull(deserializedPacket);
        }
    }
}
