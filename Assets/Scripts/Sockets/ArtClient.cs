using System;
using System.Net;
using System.Net.Sockets;
using ArtNet.Packets;
using UnityEngine;

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

        private void StartReceive()
        {
            var receivedData = new ReceivedData();
            _udpClient.BeginReceive(ReceiveCallback, receivedData);
        }

        private void ReceiveCallback(IAsyncResult state)
        {
            var receivedData = (ReceivedData)(state.AsyncState);

            if (receivedData != null)
            {
                var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                receivedData.Buffer = _udpClient.EndReceive(state, ref remoteEndPoint);
                receivedData.RemoteAddress = remoteEndPoint.Address;
                if (receivedData.Validate)
                {
                    ReceiveNat(receivedData, remoteEndPoint);
                }
            }

            StartReceive();
        }

        private void ReceiveNat(ReceivedData receivedData, IPEndPoint sourceEndPoint)
        {
            LastReceiveAt = DateTime.Now;
            if (ReceiveEvent == null) return;
            var packet = ReceivedData.CreatePacket(receivedData);
            ReceiveEvent(this, new ReceiveEventArgs<ArtPacket>(packet, sourceEndPoint));
        }
    }
}
