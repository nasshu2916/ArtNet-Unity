using System.Net;
using System.Net.Sockets;

namespace ArtNet
{
    public class UdpSender
    {
        private readonly Socket _socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        public void Send(byte[] data, IPAddress ip, int port)
        {
            _socket.SendTo(data, new IPEndPoint(ip, port));
        }
    }
}
