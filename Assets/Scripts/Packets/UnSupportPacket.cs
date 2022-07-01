using ArtNet.Sockets;

namespace ArtNet.Packets
{
    public class UnSupportPacket : ArtPacket
    {
        public UnSupportPacket(ReceivedData data) : base(data)
        {
        }
    }
}
