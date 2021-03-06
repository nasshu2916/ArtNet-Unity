using System.IO;
using System.Net;
using System.Text;

namespace ArtNet.IO
{
    public class ArtReader : BinaryReader
    {
        public ArtReader(Stream input) : base(input)
        {
        }

        public ushort ReadNetworkUInt16()
        {
            return (ushort)IPAddress.NetworkToHostOrder(ReadInt16());
        }

        public string ReadNetworkString(int count)
        {
            return Encoding.UTF8.GetString(ReadBytes(count));
        }
    }
}
