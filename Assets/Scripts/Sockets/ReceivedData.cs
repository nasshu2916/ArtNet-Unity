using System.Linq;
using System.Net;
using System.Text;
using ArtNet.Packets;

namespace ArtNet.Sockets
{
    public class ReceivedData
    {
        public byte[] Buffer;
        private static readonly byte[] IdentificationId = Encoding.ASCII.GetBytes("Art-Net\0");

        public bool Validate => Buffer.Take(8).SequenceEqual(IdentificationId);
        public ushort OpCode => (ushort)(Buffer[8] + (Buffer[9] << 8));
        public IPAddress RemoteAddress { get; set; }

        public static ArtPacket CreatePacket(ReceivedData data)
        {
            return (Enums.OpCode)data.OpCode switch
            {
                Enums.OpCode.Dmx => new ArtDmxPacket(data),
                _ => new UnSupportPacket(data)
            };
        }
    }
}
