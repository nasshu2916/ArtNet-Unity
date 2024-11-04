using UnityEditor;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    public class AnimationRecorderSettings : RecorderSettings
    {
        protected internal override string Extension => "anim";
        internal override string DefaultName => "Animation";
        protected internal override Texture2D Icon => _icon ??= (Texture2D) EditorGUIUtility.Load("Animation Icon");

        private static Texture2D _icon;
    }
}
