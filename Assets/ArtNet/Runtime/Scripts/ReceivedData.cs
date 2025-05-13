using System;
using System.Net;
using ArtNet.Packets;

namespace ArtNet
{
    public class ReceivedData<TPacket> where TPacket : ArtNetPacket
    {
        private ReceivedData()
        {
            ReceivedAt = DateTime.Now;
        }

        public ReceivedData(TPacket packet, EndPoint remoteEndPoint) : this() =>
            (Packet, RemoteEp) = (packet, remoteEndPoint);
        public TPacket Packet { get; }
        public EndPoint RemoteEp { get; }
        public DateTime ReceivedAt { get; }
    }
}
