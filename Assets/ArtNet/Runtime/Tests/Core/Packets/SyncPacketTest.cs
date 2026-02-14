using System.Collections;
using ArtNet.Packets;
using JetBrains.Annotations;
using NUnit.Framework;

namespace ArtNet.Tests.Core.Packets
{
    public class SyncPacketTest
    {
        [Test]
        public void TestSyncPacketSerialization()
        {
            var packet = new SyncPacket();
            AssertSyncPacket(packet, new byte[]
            {
                0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00, // "Art-Net\0"
                0x00, 0x52, // OpCode(Sync)
                0x00, 0x0E, // Protocol Version
                0x00, // Aux1
                0x00 // Aux2
            });
        }

        private static void AssertSyncPacket([NotNull] SyncPacket packet, IEnumerable expected)
        {
            var serializedData = packet.ToByteArray();
            Assert.IsNotNull(serializedData);

            CollectionAssert.AreEqual(expected, serializedData);
            var deserializedPacket = ArtNetPacket.FromByteArray<SyncPacket>(serializedData);
            Assert.IsNotNull(deserializedPacket);

            Assert.AreEqual(packet.OpCode, deserializedPacket.OpCode);
            Assert.AreEqual(packet.IsNeedProtocolVersion, deserializedPacket.IsNeedProtocolVersion);
            Assert.AreEqual((byte) 0, deserializedPacket.Aux1);
            Assert.AreEqual((byte) 0, deserializedPacket.Aux2);
        }

        [Test]
        public void TestInvalidSyncPacketBytes()
        {
            var invalidData = new byte[]
            {
                0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00,
                0x00, 0x52,
                0x00
            };

            var deserializedPacket = ArtNetPacket.FromByteArray<SyncPacket>(invalidData);
            Assert.IsNull(deserializedPacket);
        }
    }
}
