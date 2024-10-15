using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    [CustomEditor(typeof(ConvertAnim))]
    public class ConvertAnimInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var convertAnim = target as ConvertAnim;
            if (!convertAnim) return;

            GUILayout.Space(5);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                if (GUILayout.Button("Convert"))
                {
                    Convert(convertAnim);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private static void Convert(ConvertAnim convertAnim)
        {
            var binary = convertAnim.binary;
            if (!binary)
            {
                Debug.LogError("Binary is null");
                return;
            }

            var bytes = binary.bytes;
            TimelineConverter timelineConverter = new(RecordData.Deserialize(bytes));
            var dmxTimelines = new List<DmxTimeline>();

            foreach (var timelineUniverse in timelineConverter.Timelines)
            {
                var universe = timelineUniverse.Universe;
                timelineUniverse.ThinOutUnchangedFrames();
                var clip = timelineUniverse.ToAnimationClip();

                SaveAnimationClip(clip, $"Assets/Test/Universe{universe}.anim");

                var timelineElement = new DmxTimeline
                {
                    DmxTimelineClip = clip,
                    Universe = universe
                };
                dmxTimelines.Add(timelineElement);
            }

            SaveDmxTimelineAsset(dmxTimelines, "Assets/Test/DmxTimeline.asset");

            AssetDatabase.Refresh();
        }

        private static void SaveAnimationClip(AnimationClip clip, string path)
        {
            AssetDatabase.CreateAsset(clip, path);
            AssetDatabase.SaveAssets();
            Debug.Log("AnimationClip saved to " + path);
        }

        private static void SaveDmxTimelineAsset(List<DmxTimeline> dmxTimeline, string path)
        {
            var dmxTimelineAsset = CreateInstance<DmxTimelineSetting>();
            dmxTimelineAsset.DmxTimelines = dmxTimeline;
            AssetDatabase.CreateAsset(dmxTimelineAsset, path);
            AssetDatabase.SaveAssets();
            Debug.Log("DmxTimelineAsset saved to " + path);
        }
    }
}
