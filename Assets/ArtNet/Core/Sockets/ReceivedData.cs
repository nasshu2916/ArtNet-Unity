using System;
using System.Linq;
using System.Net;
using ArtNet.Packets;

namespace ArtNet.Sockets
{
    public class ReceivedData
    {
        public byte[] Buffer { get; set; } = new byte[1500];

        public bool Validate => Buffer.Take(8).SequenceEqual(ArtPacket.IdentificationIds);
        public Enums.OpCode OpCode => (Enums.OpCode)(Buffer[8] + (Buffer[9] << 8));
        public IPAddress RemoteAddress { get; set; }

        public DateTime ReceivedTime { get; set; }
    }
}
