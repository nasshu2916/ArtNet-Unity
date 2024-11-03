using UnityEditor;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    public static class IconHelper
    {
        private static Texture2D _errorIcon, _warningIcon, _infoIcon;

        public static Texture2D ErrorIcon => Icon(_errorIcon, "icons/console.erroricon.png");

        public static Texture2D WarningIcon => Icon(_warningIcon, "icons/console.warnicon.png");

        public static Texture2D InfoIcon => Icon(_infoIcon, "icons/console.infoicon.png");

        private static Texture2D Icon(Texture2D icon, string iconPath)
        {
            if (icon != null) return icon;

            icon = EditorGUIUtility.Load(iconPath) as Texture2D;
            return icon;
        }
    }
}
