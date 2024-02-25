using System.Net;

namespace ArtNet.Editor.DmxRecorder
{
    public class SenderConfig
    {
        public IPAddress Ip { get; set; } = IPAddress.Parse("127.0.0.1");
        public bool IsLoop { get; set; }
        public bool IsRecordSequence { get; set; }
        public float Speed { get; set; } = 1;
    }
}
