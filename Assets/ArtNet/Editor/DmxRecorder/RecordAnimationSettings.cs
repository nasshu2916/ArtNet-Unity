namespace ArtNet.Editor.DmxRecorder
{
    public class RecordAnimationSettings : RecordSettings
    {
        protected internal override string Extension { get; } = "anim";
        internal override string DefaultName { get; } = "Animation";
    }
}
