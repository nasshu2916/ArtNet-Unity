using System.Net;
using System.Net.Sockets;
using JetBrains.Annotations;

namespace ArtNet
{
    public class UdpSender
    {
        [NotNull] private readonly Socket _socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        public void Send([NotNull] byte[] data, [NotNull] IPAddress ip, int port)
        {
            _socket.SendTo(data, new IPEndPoint(ip, port));
        }

        public void Send([NotNull] byte[] data, [NotNull] EndPoint endPoint)
        {
            _socket.SendTo(data, endPoint);
        }
    }
}
