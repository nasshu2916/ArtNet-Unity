using System.Collections;
using ArtNet.Packets;
using JetBrains.Annotations;
using NUnit.Framework;

namespace ArtNet.Tests.Core.Packets
{
    public class TodControlPacketTest
    {
        [Test]
        public void TestTodControlPacketSerialization()
        {
            var packet = new TodControlPacket
            {
                RdmVersion = 0x01,
                Filler1 = 0x00,
                Net = 0x00,
                Command = 0x01,
                Address = 0x02
            };

            var serializedData = packet.ToByteArray();
            Assert.IsNotNull(serializedData);

            AssertTodControlPacket(packet, serializedData);
        }

        private static void AssertTodControlPacket([NotNull] TodControlPacket packet, IEnumerable expected)
        {
            var serializedData = packet.ToByteArray();
            Assert.IsNotNull(serializedData);

            CollectionAssert.AreEqual(expected, serializedData);
            var deserializedPacket = ArtNetPacket.FromByteArray<TodControlPacket>(serializedData);
            Assert.IsNotNull(deserializedPacket);

            Assert.AreEqual(packet.OpCode, deserializedPacket.OpCode);
            Assert.AreEqual(packet.IsNeedProtocolVersion, deserializedPacket.IsNeedProtocolVersion);
            Assert.AreEqual(packet.RdmVersion, deserializedPacket.RdmVersion);
            Assert.AreEqual(packet.Filler1, deserializedPacket.Filler1);
            Assert.AreEqual(packet.Net, deserializedPacket.Net);
            Assert.AreEqual(packet.Command, deserializedPacket.Command);
            Assert.AreEqual(packet.Address, deserializedPacket.Address);
        }

        [Test]
        public void TestInvalidTodControlPacketBytes()
        {
            var invalidData = new byte[]
            {
                0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00,
                0x00, 0x82,
                0x00, 0x0E,
                0x01
            };

            var deserializedPacket = ArtNetPacket.FromByteArray<TodControlPacket>(invalidData);
            Assert.IsNull(deserializedPacket);
        }
    }
}
