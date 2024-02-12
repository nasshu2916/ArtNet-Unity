namespace ArtNet.Editor.DmxRecorder
{
    public class RecordConfig
    {
        private const string Extension = ".dmx";

        public string Directory;
        public string FileName;

        public string OutputPath => $"{Directory}/{FileName}{Extension}";
    }
}
