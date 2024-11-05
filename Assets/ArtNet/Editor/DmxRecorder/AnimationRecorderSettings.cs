using UnityEditor;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    public class AnimationRecorderSettings : RecorderSettings
    {
        protected internal override string Extension => "anim";
        internal override string DefaultName => "Animation";
        protected internal override Texture Icon => IconHelper.Icon("Animation Icon", true);
    }
}
