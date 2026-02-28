using System.Collections;
using ArtNet.Packets;
using JetBrains.Annotations;
using NUnit.Framework;

namespace ArtNet.Tests.Core.Packets
{
    public class AddressPacketTest
    {
        [Test]
        public void TestAddressPacketSerialization()
        {
            var packet = new AddressPacket
            {
                NetSwitch = 0x01,
                BindIndex = 0x02,
                ShortName = "ShortName",
                LongName = "LongName",
                SwIn = new byte[] { 1, 2, 3, 4 },
                SwOut = new byte[] { 5, 6, 7, 8 },
                SubSwitch = 0x09,
                SwVideo = 0x0A,
                Command = 0x0B,
                Filler = 0x00
            };

            var serializedData = packet.ToByteArray();
            Assert.IsNotNull(serializedData);

            AssertAddressPacket(packet, serializedData);
        }

        private static void AssertAddressPacket([NotNull] AddressPacket packet, IEnumerable expected)
        {
            var serializedData = packet.ToByteArray();
            Assert.IsNotNull(serializedData);

            CollectionAssert.AreEqual(expected, serializedData);
            var deserializedPacket = ArtNetPacket.FromByteArray<AddressPacket>(serializedData);
            Assert.IsNotNull(deserializedPacket);

            Assert.AreEqual(packet.OpCode, deserializedPacket.OpCode);
            Assert.AreEqual(packet.IsNeedProtocolVersion, deserializedPacket.IsNeedProtocolVersion);
            Assert.AreEqual(packet.NetSwitch, deserializedPacket.NetSwitch);
            Assert.AreEqual(packet.BindIndex, deserializedPacket.BindIndex);
            Assert.AreEqual(packet.ShortName, deserializedPacket.ShortName.TrimEnd('\0'));
            Assert.AreEqual(packet.LongName, deserializedPacket.LongName.TrimEnd('\0'));
            CollectionAssert.AreEqual(packet.SwIn, deserializedPacket.SwIn);
            CollectionAssert.AreEqual(packet.SwOut, deserializedPacket.SwOut);
            Assert.AreEqual(packet.SubSwitch, deserializedPacket.SubSwitch);
            Assert.AreEqual(packet.SwVideo, deserializedPacket.SwVideo);
            Assert.AreEqual(packet.Command, deserializedPacket.Command);
            Assert.AreEqual(packet.Filler, deserializedPacket.Filler);
        }

        [Test]
        public void TestInvalidAddressPacketBytes()
        {
            var invalidData = new byte[]
            {
                0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00,
                0x00, 0x60,
                0x00, 0x0E,
                0x00
            };

            var deserializedPacket = ArtNetPacket.FromByteArray<AddressPacket>(invalidData);
            Assert.IsNull(deserializedPacket);
        }

        [Test]
        public void TestInvalidAddressPacketData()
        {
            var packet = new AddressPacket
            {
                SwIn = new byte[] { 1, 2, 3 },
                SwOut = new byte[] { 4, 5, 6, 7 }
            };

            var serializedData = packet.ToByteArray();
            Assert.IsNull(serializedData);
        }
    }
}
