using System;

namespace ArtNet.Editor.DmxRecorder
{
    public class UniverseData
    {
        public double Time { get; }
        public ushort Universe { get; }
        public byte[] Values { get; }

        public UniverseData(double time, ushort universe, ReadOnlySpan<byte> values)
        {
            Time = time;
            Universe = universe;
            Values = values.ToArray();
        }

        public ushort Length => (ushort) Values.Length;
    }
}
