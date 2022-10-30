using System;
using System.Net;
using System.Net.Sockets;
using ArtNet.Packets;

namespace ArtNet.Sockets
{
    public class ArtClient
    {
        private const int ArtNetPort = 6454;
        private readonly UdpClient _udpClient;
        public DateTime LastReceiveAt { get; private set; }
        public event EventHandler<ReceiveEventArgs<ArtPacket>> ReceiveEvent;

        public ArtClient()
        {
            _udpClient = new UdpClient(ArtNetPort);
        }

        public ArtClient(IPAddress address)
        {
            var bindEndPoint = new IPEndPoint(address, ArtNetPort);
            _udpClient = new UdpClient(bindEndPoint);
        }

        public void Open()
        {
            StartReceive();
        }

        public void Close()
        {
            _udpClient.Close();
        }

        public void Send(ArtPacket packet, IPAddress sendAddress)
        {
            var data = packet.ToByteArray();
            var sendEndPoint = new IPEndPoint(sendAddress, ArtNetPort);
            _udpClient.Send(data, data.Length, sendEndPoint);
        }

        private void StartReceive()
        {
            _udpClient.BeginReceive(ReceiveCallback, new ReceivedData());
        }

        private void ReceiveCallback(IAsyncResult state)
        {
            var receivedData = (ReceivedData)(state.AsyncState);

            if (receivedData == null) return;

            var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            receivedData.Buffer = _udpClient.EndReceive(state, ref remoteEndPoint);

            if (!receivedData.Validate) return;

            receivedData.RemoteAddress = remoteEndPoint.Address;
            receivedData.ReceivedTime = DateTime.Now;

            ReceiveArtNet(receivedData, remoteEndPoint);
            StartReceive();
        }

        private void ReceiveArtNet(ReceivedData receivedData, IPEndPoint sourceEndPoint)
        {
            LastReceiveAt = DateTime.Now;
            if (ReceiveEvent == null) return;
            var packet = ArtPacket.Create(receivedData);
            if (packet != null)
            {
                ReceiveEvent(this, new ReceiveEventArgs<ArtPacket>(packet, sourceEndPoint));
            }
        }
    }
}
