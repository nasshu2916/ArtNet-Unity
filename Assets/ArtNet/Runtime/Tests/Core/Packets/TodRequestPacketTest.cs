using System.Collections;
using ArtNet.Packets;
using JetBrains.Annotations;
using NUnit.Framework;

namespace ArtNet.Tests.Core.Packets
{
    public class TodRequestPacketTest
    {
        [Test]
        public void TestTodRequestPacketSerialization()
        {
            var packet = new TodRequestPacket
            {
                RdmVersion = 0x01,
                Filler1 = 0x00,
                Net = 0x00,
                Command = 0x01,
                AddressCount = 0x02,
                Addresses = new byte[]
                {
                    0x00, 0x01, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00
                }
            };

            var serializedData = packet.ToByteArray();
            Assert.IsNotNull(serializedData);

            AssertTodRequestPacket(packet, serializedData);
        }

        private static void AssertTodRequestPacket([NotNull] TodRequestPacket packet, IEnumerable expected)
        {
            var serializedData = packet.ToByteArray();
            Assert.IsNotNull(serializedData);

            CollectionAssert.AreEqual(expected, serializedData);
            var deserializedPacket = ArtNetPacket.FromByteArray<TodRequestPacket>(serializedData);
            Assert.IsNotNull(deserializedPacket);

            Assert.AreEqual(packet.OpCode, deserializedPacket.OpCode);
            Assert.AreEqual(packet.IsNeedProtocolVersion, deserializedPacket.IsNeedProtocolVersion);
            Assert.AreEqual(packet.RdmVersion, deserializedPacket.RdmVersion);
            Assert.AreEqual(packet.Filler1, deserializedPacket.Filler1);
            Assert.AreEqual(packet.Net, deserializedPacket.Net);
            Assert.AreEqual(packet.Command, deserializedPacket.Command);
            Assert.AreEqual(packet.AddressCount, deserializedPacket.AddressCount);
            CollectionAssert.AreEqual(packet.Addresses, deserializedPacket.Addresses);
        }

        [Test]
        public void TestInvalidTodRequestPacketBytes()
        {
            var invalidData = new byte[]
            {
                0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00,
                0x00, 0x80,
                0x00, 0x0E,
                0x01
            };

            var deserializedPacket = ArtNetPacket.FromByteArray<TodRequestPacket>(invalidData);
            Assert.IsNull(deserializedPacket);
        }

        [Test]
        public void TestInvalidTodRequestPacketData()
        {
            var packet = new TodRequestPacket
            {
                Addresses = new byte[] { 0x00 }
            };

            var serializedData = packet.ToByteArray();
            Assert.IsNull(serializedData);
        }
    }
}
