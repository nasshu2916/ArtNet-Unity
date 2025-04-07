using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace ArtNet.Editor.DmxRecorder.IO
{
    public class BinaryDmxHeaderTest
    {
        [Test]
        public void SerializeHeaderTest()
        {
            var header = new BinaryDmx.Header();
            var expected = new byte[]
            {
                0xFF, 0x44, 0x4D, 0x58, // Identifiers
                0x01, // Version
                0x00, // CompressType
                0x03, // BodyEncodeType
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };
            var serializedHeader = header.SerializeHeader();
            Assert.AreEqual(expected.Length, serializedHeader.Length);
            CollectionAssert.AreEqual(expected, serializedHeader);

            header = new BinaryDmx.Header(compressType: BinaryDmx.CompressType.Deflate,
                encodeType: BinaryDmx.BodyEncodeType.UniverseAndValues);
            expected = new byte[]
            {
                0xFF, 0x44, 0x4D, 0x58, // Identifiers
                0x01, // Version
                0x01, // CompressType
                0x03, // BodyEncodeType
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 // ReservedBuffer
            };

            serializedHeader = header.SerializeHeader();
            Assert.AreEqual(expected.Length, serializedHeader.Length);
            CollectionAssert.AreEqual(expected, serializedHeader);
        }

        [Test]
        public void DeserializeHeaderTest()
        {
            var bytes = new byte[]
            {
                0xFF, 0x44, 0x4D, 0x58, // Identifiers
                0x01, // Version
                0x00, // CompressType
                0x03, // BodyEncodeType
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x05, 0x00
            };

            var header = BinaryDmx.Header.DeserializeHeader(bytes);
            Assert.IsNotNull(header);
            Assert.AreEqual(BinaryDmx.CompressType.None, header.BodyCompressType);
            Assert.AreEqual(BinaryDmx.BodyEncodeType.UniverseAndValues, header.BodyEncodeType);
        }

        private static IEnumerable<TestCaseData> InvalidDeserializeHeaderData
        {
            get
            {
                yield return new TestCaseData(new byte[]
                    {
                        0xFF, 0x44, 0x4D, 0x00,
                        0x01,
                        0x00,
                        0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x05, 0x00
                    })
                    .SetName("Invalid Identifiers");

                yield return new TestCaseData(new byte[]
                {
                    0xFF, 0x44, 0x4D, 0x58,
                    0x00,
                    0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x05, 0x00
                }).SetName("Invalid Version");

                yield return new TestCaseData(new byte[]
                {
                    0xFF, 0x44, 0x4D, 0x58,
                    0x01,
                    0x02,
                    0x03,
                    0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x05, 0x00
                }).SetName("Invalid CompressType");

                yield return new TestCaseData(new byte[]
                {
                    0xFF, 0x44, 0x4D, 0x58,
                    0x01, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                }).SetName("Header too short");
            }
        }

        [TestCaseSource(nameof(InvalidDeserializeHeaderData))]
        public void InvalidDeserializeHeaderTest(byte[] invalidData)
        {
            var header = BinaryDmx.Header.DeserializeHeader(invalidData);
            Assert.IsNull(header);
        }
    }

    public class BinaryDmxTest
    {
        private static IEnumerable<TestCaseData> TestSerializeData
        {
            get
            {
                yield return new TestCaseData(
                    new List<UniverseData>
                    {
                        new(1000, 0, Enumerable.Repeat((byte) 0xFF, 5).ToArray()),
                        new(1000, 1, Enumerable.Repeat((byte) 0xFF, 1).ToArray()),
                        new(1500, 0, Enumerable.Repeat((byte) 0xFF, 5).ToArray()),
                        new(1500, 1, Enumerable.Repeat((byte) 0xFF, 1).ToArray())
                    },
                    false
                ).Returns(new byte[]
                {
                    0xFF, 0x44, 0x4D, 0x58, 0x01, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, // Headers
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x05, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x01, 0x00, 0x01, 0x00, 0xFF,
                    0xF4, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x05, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    0xF4, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x01, 0x00, 0x01, 0x00, 0xFF
                })!.SetName("Serialize UniverseData");
                yield return new TestCaseData(
                    new List<UniverseData>
                    {
                        new(1000, 0, Enumerable.Repeat((byte) 0xFF, 5).ToArray()),
                        new(1000, 1, Enumerable.Repeat((byte) 0xFF, 1).ToArray()),
                        new(1500, 0, Enumerable.Repeat((byte) 0xFF, 5).ToArray()),
                        new(1500, 1, Enumerable.Repeat((byte) 0xFF, 1).ToArray()),
                    },
                    true
                ).Returns(new byte[]
                {
                    0xFF, 0x44, 0x4D, 0x58, 0x01, 0x01, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, // Headers
                    0x63, 0x60, 0x80, 0x01, 0x56, 0x86, 0xFF, 0x20, 0x00, 0xE3, 0x32, 0x02, 0xE1, 0xFF, 0x2F,
                    0x8C, 0x68, 0xB2, 0x30, 0x01, 0xB0, 0x2C, 0x00
                })!.SetName("Compress UniverseData");
                yield return new TestCaseData(
                    new List<UniverseData>
                    {
                        new(1500, 0, Enumerable.Repeat((byte) 0xFF, 5).ToArray()),
                        new(0, 5, Enumerable.Repeat((byte) 0xFF, 5).ToArray()),
                        new(1000, 1, Enumerable.Repeat((byte) 0xFF, 2).ToArray()),
                        new(1000, 0, Enumerable.Repeat((byte) 0xFF, 1).ToArray())
                    },
                    false
                ).Returns(new byte[]
                {
                    0xFF, 0x44, 0x4D, 0x58, 0x01, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x05, 0x00, 0x05, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    0xE8, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x01, 0x00, 0xFF,
                    0xE8, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x01, 0x00, 0x02, 0x00, 0xFF, 0xFF,
                    0xDC, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x05, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
                })!.SetName("Sort by time and universe");
            }
        }

        [TestCaseSource(nameof(TestSerializeData))]
        public byte[] SerializeUniverseDataTest(IEnumerable<UniverseData> universeData, bool isCompress)
        {
            var binary = BinaryDmx.SerializeUniverseData(universeData, isCompress);
            Assert.IsNotNull(binary);
            Assert.IsNotEmpty(binary);
            return binary;
        }

        private static IEnumerable<TestCaseData> TestDeserializeData
        {
            get
            {
                yield return new TestCaseData(
                    new byte[]
                    {
                        0xFF, 0x44, 0x4D, 0x58, 0x01, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, // Headers
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x05, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x01, 0x00, 0x01, 0x00, 0xFF,
                        0xF4, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x05, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                        0xF4, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x01, 0x00, 0x01, 0x00, 0xFF
                    },
                    new List<UniverseData>
                    {
                        new(0, 0, Enumerable.Repeat((byte) 0xFF, 5).ToArray()),
                        new(0, 1, Enumerable.Repeat((byte) 0xFF, 1).ToArray()),
                        new(500, 0, Enumerable.Repeat((byte) 0xFF, 5).ToArray()),
                        new(500, 1, Enumerable.Repeat((byte) 0xFF, 1).ToArray())
                    }
                )!.SetName("Deserialize UniverseData");
                yield return new TestCaseData(
                    new byte[]
                    {
                        0xFF, 0x44, 0x4D, 0x58, 0x01, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, // Headers
                        0xF4, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x05, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x01, 0x00, 0x01, 0x00, 0xFF,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x05, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                        0xF4, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x01, 0x00, 0x01, 0x00, 0xFF
                    },
                    new List<UniverseData>
                    {
                        new(0, 0, Enumerable.Repeat((byte) 0xFF, 5).ToArray()),
                        new(0, 1, Enumerable.Repeat((byte) 0xFF, 1).ToArray()),
                        new(500, 0, Enumerable.Repeat((byte) 0xFF, 5).ToArray()),
                        new(500, 1, Enumerable.Repeat((byte) 0xFF, 1).ToArray())
                    }
                )!.SetName("Sort by time and universe");
                yield return new TestCaseData(
                    new byte[]
                    {
                        0xFF, 0x44, 0x4D, 0x58, 0x01, 0x01, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, // Headers
                        0x63, 0x60, 0x80, 0x01, 0x56, 0x86, 0xFF, 0x20, 0x00, 0xE3, 0x32, 0x02, 0xE1, 0xFF, 0x2F,
                        0x8C, 0x68, 0xB2, 0x30, 0x01, 0xB0, 0x2C, 0x00
                    },
                    new List<UniverseData>
                    {
                        new(0, 0, Enumerable.Repeat((byte) 0xFF, 5).ToArray()),
                        new(0, 1, Enumerable.Repeat((byte) 0xFF, 1).ToArray()),
                        new(500, 0, Enumerable.Repeat((byte) 0xFF, 5).ToArray()),
                        new(500, 1, Enumerable.Repeat((byte) 0xFF, 1).ToArray()),
                    }
                )!.SetName("Compress UniverseData");
                yield return new TestCaseData(
                    new byte[]
                    {
                        0xFF, 0x44, 0x4D, 0x58, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, // Headers
                        0x00, 0x00, 0x00, 0x00,
                        0x00, 0x50, 0x01, 0x00,
                        0x00, 0x00, 0x05, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                        0x00, 0x00, 0x00, 0x00,
                        0x00, 0x50, 0x01, 0x00,
                        0x01, 0x00, 0x01, 0x00, 0xFF,
                        0xF4, 0x01, 0x00, 0x00,
                        0x00, 0x50, 0x02, 0x00,
                        0x00, 0x00, 0x05, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                        0xF4, 0x01, 0x00, 0x00,
                        0x00, 0x50, 0x02, 0x00,
                        0x01, 0x00, 0x01, 0x00, 0xFF
                    },
                    new List<UniverseData>
                    {
                        new(0, 0, Enumerable.Repeat((byte) 0xFF, 5).ToArray()),
                        new(0, 1, Enumerable.Repeat((byte) 0xFF, 1).ToArray()),
                        new(500, 0, Enumerable.Repeat((byte) 0xFF, 5).ToArray()),
                        new(500, 1, Enumerable.Repeat((byte) 0xFF, 1).ToArray())
                    }
                )!.SetName("Full Packet Data");
            }
        }

        [TestCaseSource(nameof(TestDeserializeData))]
        public void DeserializeDataTest(byte[] universeData, List<UniverseData> expected)
        {
            var data = BinaryDmx.Deserialize(universeData);
            Assert.IsNotNull(data);
            Assert.IsNotEmpty(data);
            Assert.AreEqual(expected!.Count, data.Count);
            for (var i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected[i].Time, data[i].Time);
                Assert.AreEqual(expected[i].Universe, data[i].Universe);
                CollectionAssert.AreEqual(expected[i].Values, data[i].Values);
            }
        }

        private static IEnumerable<TestCaseData> TestInvalidDeserializeData
        {
            get
            {
                yield return new TestCaseData(
                    new byte[]
                    {
                        0xF0, 0x44, 0x4D, 0x58, 0x01, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, // Headers
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x05, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
                    }
                )!.SetName("Invalid Identifiers");
                yield return new TestCaseData(
                    new byte[]
                    {
                        0xFF, 0x44, 0x4D, 0x58, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, // Headers
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x05, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
                    }
                )!.SetName("Invalid Header");
                yield return new TestCaseData(
                    new byte[]
                    {
                        0xFF, 0x44, 0x4D, 0x58, 0x01, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, // Headers
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x05, 0x00, 0xFF, 0xFF, 0xFF, 0xFF
                    }
                )!.SetName("Deserialize UniverseData with missing data");
                yield return new TestCaseData(
                    new byte[]
                    {
                        0xFF, 0x44, 0x4D, 0x58, 0x01, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, // Headers
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x05, 0x50, 0xFF
                    }
                )!.SetName("Invalid Dmx data length");

                // Full Packet Data
                yield return new TestCaseData(
                    new byte[]
                    {
                        0xFF, 0x44, 0x4D, 0x58, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, // Headers
                        0x00, 0x00, 0x00, 0x00,
                        0x00, 0x50, 0x01, 0x00,
                        0x00, 0x00, 0x05, 0x00, 0xFF, 0xFF, 0xFF, 0xFF
                    }
                )!.SetName("Invalid Full Packet Dmx data length");
            }
        }

        [TestCaseSource(nameof(TestInvalidDeserializeData))]
        public void InvalidDeserializeDataTest(byte[] universeData)
        {
            var data = BinaryDmx.Deserialize(universeData);
            Assert.IsNull(data);
        }
    }
}
