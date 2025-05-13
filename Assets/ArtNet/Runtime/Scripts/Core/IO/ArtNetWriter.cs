using System.IO;
using System.Net;
using System.Text;

namespace ArtNet.IO
{
    public class ArtNetWriter : BinaryWriter
    {
        public ArtNetWriter(Stream output) : base(output)
        {
        }

        public void WriteNetwork(ushort value)
        {
            base.Write(IPAddress.HostToNetworkOrder((short) value));
        }

        public void WriteNetwork(string value, int length)
        {
            base.Write(Encoding.UTF8.GetBytes(value.PadRight(length, '\0')));
        }
    }
}
