using UnityEditor;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    public static class IconHelper
    {
        private static Texture _errorIcon, _warningIcon, _infoIcon;
        private static Texture _playButton, _preMatQuad, _pauseButton;

        public static Texture ErrorIcon => Icon(_errorIcon, "console.erroricon");
        public static Texture WarningIcon => Icon(_warningIcon, "console.warnicon");
        public static Texture InfoIcon => Icon(_infoIcon, "console.infoicon");
        public static Texture PlayButton => Icon(_playButton, "PlayButton");
        public static Texture PreMatQuad => Icon(_preMatQuad, "PreMatQuad");
        public static Texture PauseButton => Icon(_pauseButton, "PauseButton");

        private static Texture Icon(Texture icon, string iconPath)
        {
            if (icon != null) return icon;

            icon = EditorGUIUtility.IconContent(iconPath).image;
            return icon;
        }
    }
}
