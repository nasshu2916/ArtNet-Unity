using System;
using System.Net;

namespace ArtNet.Sockets
{
    public class ReceiveEventArgs<TPacketType> : EventArgs
    {
        public ReceiveEventArgs(TPacketType packet, IPEndPoint sourceEndPoint, DateTime receivedAt)
        {
            Packet = packet;
            SourceEndPoint = sourceEndPoint;
            ReceivedAt = receivedAt;
        }

        public TPacketType Packet { get; }
        public IPEndPoint SourceEndPoint { get; }
        public  DateTime ReceivedAt { get;  }
    }
}
