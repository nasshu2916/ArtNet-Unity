using System.Collections.Generic;
using UnityEditor;

namespace ArtNet.Editor.DmxRecorder.IO
{
    public static class AnimationClipDmx
    {
        public static void Export(IEnumerable<UniverseData> universeData, string path)
        {
            var clip = new UnityEngine.AnimationClip();

            AssetDatabase.CreateAsset(clip, path);
            var timelineConverter = new TimelineConverter(universeData);
            timelineConverter.SaveToClip(clip);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
