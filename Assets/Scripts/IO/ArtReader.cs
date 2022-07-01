using System.IO;
using System.Net;

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
    }
}
