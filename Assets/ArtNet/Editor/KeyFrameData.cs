namespace ArtNet.Editor
{
    public struct KeyFrameData
    {
        public float Time { get; }
        public byte Value { get; }

        public KeyFrameData(float time, byte value)
        {
            Time = time;
            Value = value;
        }
    }
}
