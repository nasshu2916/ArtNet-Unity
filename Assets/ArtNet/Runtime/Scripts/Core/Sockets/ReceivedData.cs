using System;
using System.Net;

namespace ArtNet.Sockets
{
    public class ReceivedData
    {
        public byte[] Buffer { get; }
        public int Length { get; }
        public IPEndPoint RemoteEndPoint { get; }
        public DateTime ReceivedAt { get; }

        private ReceivedData()
        {
            ReceivedAt = DateTime.Now;
        }

        public ReceivedData(byte[] buffer, int length, IPEndPoint remoteEndPoint) : this()
        {
            Buffer = buffer;
            Length = length;
            RemoteEndPoint = remoteEndPoint;
        }

        public Enums.OpCode OpCode => (Enums.OpCode) (Buffer[8] + (Buffer[9] << 8));
    }
}
