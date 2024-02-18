namespace ArtNet.Editor.DmxRecorder
{
    public class SenderConfig
    {
        public string Ip { get; set; } = "127.0.0.1";
        public bool IsLoop { get; set; }
        public bool IsRecordSequence { get; set; }
    }
}
