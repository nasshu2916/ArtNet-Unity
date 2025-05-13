using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder.Util
{
    public static class IconHelper
    {
        private static readonly Dictionary<string, Texture> IconCache = new();

        public static Texture ErrorIcon => Icon("console.erroricon");
        public static Texture WarningIcon => Icon("console.warnicon");
        public static Texture InfoIcon => Icon("console.infoicon");
        public static Texture PlayButton => Icon("PlayButton@2x", true);
        public static Texture PreMatQuad => Icon("PreMatQuad@2x", true);
        public static Texture PauseButton => Icon("PauseButton@2x", true);
        public static Texture FolderOpen => Icon("FolderOpened Icon");
        public static Texture PresetIcon => Icon("Preset.Context", true);
        public static Texture RefreshIcon => Icon("Refresh", true);

        internal static Texture Icon(string iconPath, bool provideDarkModel = false)
        {
            if (provideDarkModel && EditorGUIUtility.isProSkin)
            {
                iconPath = "d_" + iconPath;
            }

            if (IconCache.TryGetValue(iconPath, out var icon))
            {
                return icon;
            }

            icon = EditorGUIUtility.IconContent(iconPath).image;
            IconCache[iconPath] = icon;
            return icon;
        }
    }
}
