using System.Collections;
using ArtNet.Packets;
using JetBrains.Annotations;
using NUnit.Framework;

namespace ArtNet.Tests.Core.Packets
{
    public class RdmPacketTest
    {
        [Test]
        public void TestRdmPacketSerialization()
        {
            var packet = new RdmPacket
            {
                RdmVersion = 0x01,
                Filler1 = 0x00,
                Net = 0x00,
                Command = 0x00,
                Address = 0x01,
                Data = new byte[] { 0xCC, 0x01, 0x02 }
            };

            var serializedData = packet.ToByteArray();
            Assert.IsNotNull(serializedData);

            AssertRdmPacket(packet, serializedData);
        }

        private static void AssertRdmPacket([NotNull] RdmPacket packet, IEnumerable expected)
        {
            var serializedData = packet.ToByteArray();
            Assert.IsNotNull(serializedData);

            CollectionAssert.AreEqual(expected, serializedData);
            var deserializedPacket = ArtNetPacket.FromByteArray<RdmPacket>(serializedData);
            Assert.IsNotNull(deserializedPacket);

            Assert.AreEqual(packet.OpCode, deserializedPacket.OpCode);
            Assert.AreEqual(packet.IsNeedProtocolVersion, deserializedPacket.IsNeedProtocolVersion);
            Assert.AreEqual(packet.RdmVersion, deserializedPacket.RdmVersion);
            Assert.AreEqual(packet.Filler1, deserializedPacket.Filler1);
            Assert.AreEqual(packet.Net, deserializedPacket.Net);
            Assert.AreEqual(packet.Command, deserializedPacket.Command);
            Assert.AreEqual(packet.Address, deserializedPacket.Address);
            CollectionAssert.AreEqual(packet.Data, deserializedPacket.Data);
        }

        [Test]
        public void TestInvalidRdmPacketBytes()
        {
            var invalidData = new byte[]
            {
                0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00,
                0x00, 0x83,
                0x00, 0x0E,
                0x01
            };

            var deserializedPacket = ArtNetPacket.FromByteArray<RdmPacket>(invalidData);
            Assert.IsNull(deserializedPacket);
        }

        [Test]
        public void TestInvalidRdmPacketData()
        {
            var packet = new RdmPacket
            {
                Data = null
            };

            var serializedData = packet.ToByteArray();
            Assert.IsNull(serializedData);
        }
    }
}
