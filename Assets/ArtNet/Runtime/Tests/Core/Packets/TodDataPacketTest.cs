using System.Collections;
using ArtNet.Packets;
using JetBrains.Annotations;
using NUnit.Framework;

namespace ArtNet.Tests.Core.Packets
{
    public class TodDataPacketTest
    {
        [Test]
        public void TestTodDataPacketSerialization()
        {
            var packet = new TodDataPacket
            {
                RdmVersion = 0x01,
                Port = 0x01,
                BindIndex = 0x01,
                Net = 0x00,
                CommandResponse = 0x00,
                Address = 0x01,
                UidTotalHigh = 0x00,
                UidTotalLow = 0x01,
                BlockCount = 0x00,
                UidCount = 0x01,
                Tod = new byte[] { 0x7A, 0x70, 0x00, 0x00, 0x00, 0x01 }
            };

            var serializedData = packet.ToByteArray();
            Assert.IsNotNull(serializedData);

            AssertTodDataPacket(packet, serializedData);
        }

        private static void AssertTodDataPacket([NotNull] TodDataPacket packet, IEnumerable expected)
        {
            var serializedData = packet.ToByteArray();
            Assert.IsNotNull(serializedData);

            CollectionAssert.AreEqual(expected, serializedData);
            var deserializedPacket = ArtNetPacket.FromByteArray<TodDataPacket>(serializedData);
            Assert.IsNotNull(deserializedPacket);

            Assert.AreEqual(packet.OpCode, deserializedPacket.OpCode);
            Assert.AreEqual(packet.IsNeedProtocolVersion, deserializedPacket.IsNeedProtocolVersion);
            Assert.AreEqual(packet.RdmVersion, deserializedPacket.RdmVersion);
            Assert.AreEqual(packet.Port, deserializedPacket.Port);
            Assert.AreEqual(packet.BindIndex, deserializedPacket.BindIndex);
            Assert.AreEqual(packet.Net, deserializedPacket.Net);
            Assert.AreEqual(packet.CommandResponse, deserializedPacket.CommandResponse);
            Assert.AreEqual(packet.Address, deserializedPacket.Address);
            Assert.AreEqual(packet.UidTotalHigh, deserializedPacket.UidTotalHigh);
            Assert.AreEqual(packet.UidTotalLow, deserializedPacket.UidTotalLow);
            Assert.AreEqual(packet.BlockCount, deserializedPacket.BlockCount);
            Assert.AreEqual(packet.UidCount, deserializedPacket.UidCount);
            CollectionAssert.AreEqual(packet.Tod, deserializedPacket.Tod);
        }

        [Test]
        public void TestInvalidTodDataPacketBytes()
        {
            var invalidData = new byte[]
            {
                0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00,
                0x00, 0x81,
                0x00, 0x0E,
                0x01
            };

            var deserializedPacket = ArtNetPacket.FromByteArray<TodDataPacket>(invalidData);
            Assert.IsNull(deserializedPacket);
        }

        [Test]
        public void TestInvalidTodDataPacketData()
        {
            var packet = new TodDataPacket
            {
                UidCount = 0x01,
                Tod = new byte[] { 0x00 }
            };

            var serializedData = packet.ToByteArray();
            Assert.IsNull(serializedData);
        }
    }
}
