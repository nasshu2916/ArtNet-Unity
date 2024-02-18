using System.Net;
using System.Net.Sockets;

namespace ArtNet
{
    public class UdpSender
    {
        private Socket _socket;

        public UdpSender(int port)
        {
            Port = port;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }
        public int Port { get; }

        public void Send(byte[] data, string ip)
        {
            _socket.SendTo(data, new IPEndPoint(IPAddress.Parse(ip), Port));
        }
    }
}
