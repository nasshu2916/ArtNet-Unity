using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using ArtNet.Packets;

namespace ArtNet.Sockets
{
    public class ReceivedData
    {
        public byte[] Buffer { get; }
        public Enums.OpCode OpCode => (Enums.OpCode)(Buffer[8] + (Buffer[9] << 8));
        public IPEndPoint RemoteEndPoint { get; }
        public DateTime ReceivedAt { get; }

        private ReceivedData()
        {
            ReceivedAt = DateTime.Now;
        }

        public ReceivedData(UdpReceiveResult result) : this()
        {
            Buffer = result.Buffer;
            RemoteEndPoint = result.RemoteEndPoint;
            if (!Validate()) throw new ArgumentException("Invalid ArtNet packet");
        }

        private bool Validate()
        {
            return Buffer.Take(8).SequenceEqual(ArtPacket.IdentificationIds);
        }
    }
}
