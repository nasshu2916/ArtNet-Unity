using System.Collections.Generic;
using System.Linq;
using ArtNet.Packets;
using NUnit.Framework;

namespace ArtNet.Tests.Core.Packets
{
    public class ArtNetPacketTest
    {
        [Test]
        public void TestFromByteArray()
        {
            var bytes = new byte[]
            {
                0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00, // "Art-Net\0"
                0x00, 0x50, // OpCode(Dmx)
                0x00, 0x0E, // Protocol Version
                0xFF, // Sequence
                0x00, // Physical
                0x01, 0x00, // Universe
                0x00, 0x01, // Length
                0xFF // DMX data
            };

            var dmxPacket = ArtNetPacket.FromByteArray<DmxPacket>(bytes);

            Assert.IsNotNull(dmxPacket);
            Assert.AreEqual(dmxPacket.GetType(), typeof(DmxPacket));

            Assert.AreEqual(dmxPacket.OpCode, Enums.OpCode.Dmx);
            Assert.AreEqual(dmxPacket.Sequence, 255);
            Assert.AreEqual(dmxPacket.Physical, 0);
            Assert.AreEqual(dmxPacket.Universe, 1);
            Assert.AreEqual(dmxPacket.Length, 1);
            CollectionAssert.AreEqual(dmxPacket.Dmx, new byte[] { 0xFF });

            bytes = new byte[]
            {
                0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00, // "Art-Net\0"
                0x00, 0x20, // OpCode(Poll)
                0x00, 0x0E,
                0x00,
                0x00
            };

            dmxPacket = ArtNetPacket.FromByteArray<DmxPacket>(bytes);
            Assert.IsNull(dmxPacket);

            var pollPacket = ArtNetPacket.FromByteArray<PollPacket>(bytes);
            Assert.IsNotNull(pollPacket);
            Assert.AreEqual(pollPacket.GetType(), typeof(PollPacket));

            bytes = new byte[]
            {
                0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00, // "Art-Net\0"
                0x00, 0x52, // OpCode(Sync)
                0x00, 0x0E,
                0x00, 0x00
            };

            dmxPacket = ArtNetPacket.FromByteArray<DmxPacket>(bytes);
            Assert.IsNull(dmxPacket);

            var syncPacket = ArtNetPacket.FromByteArray<SyncPacket>(bytes);
            Assert.IsNotNull(syncPacket);
            Assert.AreEqual(syncPacket.GetType(), typeof(SyncPacket));

            bytes = new byte[]
            {
                0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00, // "Art-Net\0"
                0x00, 0x97, // OpCode(TimeCode)
                0x00, 0x0E, // Protocol Version
                0x18, // Frames
                0x38, // Seconds
                0x2A, // Minutes
                0x0D, // Hours
                0x01 // Type
            };

            dmxPacket = ArtNetPacket.FromByteArray<DmxPacket>(bytes);
            Assert.IsNull(dmxPacket);

            var timeCodePacket = ArtNetPacket.FromByteArray<TimeCodePacket>(bytes);
            Assert.IsNotNull(timeCodePacket);
            Assert.AreEqual(timeCodePacket.GetType(), typeof(TimeCodePacket));

            bytes = new byte[]
            {
                0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00, // "Art-Net\0"
                0x00, 0x60, // OpCode(Address)
                0x00, 0x0E, // Protocol Version
            };
            bytes = bytes.Concat(new byte[96]).ToArray();

            dmxPacket = ArtNetPacket.FromByteArray<DmxPacket>(bytes);
            Assert.IsNull(dmxPacket);

            var addressPacket = ArtNetPacket.FromByteArray<AddressPacket>(bytes);
            Assert.IsNotNull(addressPacket);
            Assert.AreEqual(addressPacket.GetType(), typeof(AddressPacket));

            bytes = new byte[]
            {
                0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00, // "Art-Net\0"
                0x00, 0x80, // OpCode(TodRequest)
                0x00, 0x0E, // Protocol Version
            };
            bytes = bytes.Concat(new byte[37]).ToArray();

            dmxPacket = ArtNetPacket.FromByteArray<DmxPacket>(bytes);
            Assert.IsNull(dmxPacket);

            var todRequestPacket = ArtNetPacket.FromByteArray<TodRequestPacket>(bytes);
            Assert.IsNotNull(todRequestPacket);
            Assert.AreEqual(todRequestPacket.GetType(), typeof(TodRequestPacket));
        }

        private static IEnumerable<TestCaseData> InvalidBytesFromByteArrayTestCases
        {
            get
            {
                yield return new TestCaseData(new byte[]
                    {
                        0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x01,
                        0x00, 0x50,
                        0x00, 0x0E,
                        0x7B,
                        0x00,
                        0x00, 0x00,
                        0x00, 0x01,
                        0xFF
                    })
                    .SetName("Invalid Art-Ne ID");
                yield return new TestCaseData(new byte[]
                    {
                        0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00,
                        0x50, 0x00, // Invalid OpCode
                        0x00, 0x0E,
                        0x7B,
                        0x00,
                        0x00, 0x00,
                        0x00, 0x01,
                        0xFF
                    })
                    .SetName("Invalid OpCode");
                yield return new TestCaseData(new byte[]
                    {
                        0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00,
                        0x00, 0x50,
                        0x00, 0x0F, // Invalid Protocol Version
                        0x7B,
                        0x00,
                        0x01, 0x00,
                        0x00, 0x03,
                        0x01, 0x02, 0x03
                    })
                    .SetName("Invalid Protocol Version");
            }
        }

        [TestCaseSource(nameof(InvalidBytesFromByteArrayTestCases))]
        public void TestInvalidBytesFromByteArray(byte[] invalidData)
        {
            var deserializedPacket = ArtNetPacket.FromByteArray<DmxPacket>(invalidData);
            Assert.IsNull(deserializedPacket);
        }

        [Test]
        public void TestToByteArray()
        {
            var packet = new DmxPacket
            {
                Sequence = 0x00,
                Physical = 0x00,
                Universe = 0x01,
                Dmx = new byte[] { 0xFF }
            };

            var serializedData = packet.ToByteArray();
            Assert.IsNotNull(serializedData);
            var expected = new byte[]
            {
                0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00,
                0x00, 0x50,
                0x00, 0x0E,
                0x00,
                0x00,
                0x01, 0x00,
                0x00, 0x01,
                0xFF
            };
            CollectionAssert.AreEqual(expected, serializedData);

            packet = new DmxPacket
            {
                Sequence = 0x00,
                Physical = 0x00,
                Universe = 0x01,
                Dmx = null
            };

            serializedData = packet.ToByteArray();
            Assert.IsNull(serializedData);
        }

        [Test]
        public void TestCreate()
        {
            var bytes = new byte[]
            {
                0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00,
                0x00, 0x50,
                0x00, 0x0E,
                0x00,
                0x00,
                0x01, 0x00,
                0x00, 0x01,
                0xFF
            };

            var packet = ArtNetPacket.Create(bytes);

            Assert.IsNotNull(packet);
            Assert.AreEqual(packet.GetType(), typeof(DmxPacket));
            Assert.AreEqual(packet.OpCode, Enums.OpCode.Dmx);

            bytes = new byte[]
            {
                0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00,
                0x00, 0x20,
                0x00, 0x0E,
                0x00,
                0x00
            };
            packet = ArtNetPacket.Create(bytes);
            Assert.IsNotNull(packet);
            Assert.AreEqual(packet.GetType(), typeof(PollPacket));
            Assert.AreEqual(packet.OpCode, Enums.OpCode.Poll);

            bytes = new byte[]
            {
                0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00,
                0x00, 0x52,
                0x00, 0x0E,
                0x00, 0x00
            };
            packet = ArtNetPacket.Create(bytes);
            Assert.IsNotNull(packet);
            Assert.AreEqual(packet.GetType(), typeof(SyncPacket));
            Assert.AreEqual(packet.OpCode, Enums.OpCode.Sync);

            bytes = new byte[]
            {
                0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00,
                0x00, 0x97,
                0x00, 0x0E,
                0x18,
                0x38,
                0x2A,
                0x0D,
                0x01
            };
            packet = ArtNetPacket.Create(bytes);
            Assert.IsNotNull(packet);
            Assert.AreEqual(packet.GetType(), typeof(TimeCodePacket));
            Assert.AreEqual(packet.OpCode, Enums.OpCode.TimeCode);

            bytes = new byte[]
            {
                0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00,
                0x00, 0x60,
                0x00, 0x0E
            };
            bytes = bytes.Concat(new byte[96]).ToArray();
            packet = ArtNetPacket.Create(bytes);
            Assert.IsNotNull(packet);
            Assert.AreEqual(packet.GetType(), typeof(AddressPacket));
            Assert.AreEqual(packet.OpCode, Enums.OpCode.Address);

            bytes = new byte[]
            {
                0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00,
                0x00, 0x80,
                0x00, 0x0E
            };
            bytes = bytes.Concat(new byte[37]).ToArray();
            packet = ArtNetPacket.Create(bytes);
            Assert.IsNotNull(packet);
            Assert.AreEqual(packet.GetType(), typeof(TodRequestPacket));
            Assert.AreEqual(packet.OpCode, Enums.OpCode.TodRequest);

            bytes = new byte[]
            {
                0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00,
                0xFF, 0xFF,
                0x00, 0x0E,
                0x00
            };

            packet = ArtNetPacket.Create(bytes);
            Assert.IsNull(packet);

            bytes = new byte[]
            {
                0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x01,
                0x00, 0x20,
                0x00, 0x0E,
                0x00,
                0x00
            };

            packet = ArtNetPacket.Create(bytes);
            Assert.IsNull(packet);
        }
    }
}
