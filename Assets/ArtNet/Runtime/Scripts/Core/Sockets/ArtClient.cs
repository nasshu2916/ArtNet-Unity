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
        public IPAddress LocalAddress { get; }
        public DateTime LastReceiveAt { get; private set; }
        public bool IsOpen { get; private set; }
        public event EventHandler<ReceiveEventArgs<ArtPacket>> ReceiveEvent;

        public ArtClient()
        {
            _udpClient = new UdpClient(ArtNetPort);
        }

        public ArtClient(IPAddress address)
        {
            LocalAddress = address;
            var bindEndPoint = new IPEndPoint(LocalAddress, ArtNetPort);
            _udpClient = new UdpClient(bindEndPoint);
        }

        public void Open()
        {
            IsOpen = true;
            StartReceive();
        }

        public void Close()
        {
            _udpClient.Close();
            IsOpen = false;
        }

        public void Send(ArtPacket packet, IPAddress sendAddress)
        {
            var data = packet.ToByteArray();
            var sendEndPoint = new IPEndPoint(sendAddress, ArtNetPort);
            _udpClient.Send(data, data.Length, sendEndPoint);
        }

        public void Dispose()
        {
            IsOpen = false;
            _udpClient.Dispose();
        }

        private void StartReceive()
        {
            _udpClient.BeginReceive(OnReceive, new ReceivedData());
        }

        private void OnReceive(IAsyncResult state)
        {
            var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                var message = _udpClient.EndReceive(state, ref remoteEndPoint);
                var receivedData = new ReceivedData(message, remoteEndPoint.Address);
                ReceiveArtNet(receivedData, remoteEndPoint);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                StartReceive();
            }
        }

        private void ReceiveArtNet(ReceivedData receivedData, IPEndPoint sourceEndPoint)
        {
            var packet = ArtPacket.Create(receivedData);

            if (packet == null) return;
            LastReceiveAt = DateTime.Now;

            ReceiveEvent?.Invoke(this, new ReceiveEventArgs<ArtPacket>(packet, sourceEndPoint));
        }
    }
}
