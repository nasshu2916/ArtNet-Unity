using System;
using System.Net;

namespace ArtNet.Sockets
{
    public class ReceiveEventArgs<TPacketType> : EventArgs
    {
        public ReceiveEventArgs(TPacketType packet, IPEndPoint sourceEndPoint)
        {
            Packet = packet;
            SourceEndPoint = sourceEndPoint;
        }

        public TPacketType Packet { get; }
        public IPEndPoint SourceEndPoint { get; }
    }
}
