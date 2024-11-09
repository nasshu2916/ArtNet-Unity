namespace ArtNet.Editor.DmxRecorder
{
    public class UniverseData
    {
        public double Time { get; }
        public uint Universe { get; }
        public byte[] Values { get; }

        public UniverseData(double time, uint universe, byte[] values)
        {
            Time = time;
            Universe = universe;
            Values = values;
        }
    }
}
