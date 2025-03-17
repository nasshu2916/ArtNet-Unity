using System;

namespace ArtNet.Editor.DmxRecorder
{
    public class UniverseData
    {
        public long Time { get; }
        public ushort Universe { get; }
        public byte[] Values { get; }

        public UniverseData(long time, ushort universe, ReadOnlySpan<byte> values)
        {
            Time = time;
            Universe = universe;
            Values = values.ToArray();
        }

        public ushort Length => (ushort) Values.Length;
    }
}
