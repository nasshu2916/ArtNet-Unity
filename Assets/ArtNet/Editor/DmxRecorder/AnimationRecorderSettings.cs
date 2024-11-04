namespace ArtNet.Editor.DmxRecorder
{
    public class AnimationRecorderSettings : RecorderSettings
    {
        protected internal override string Extension { get; } = "anim";
        internal override string DefaultName { get; } = "Animation";
    }
}
