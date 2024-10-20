using System.IO;
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
                if (GUILayout.Button("ConvertAnim"))
                {
                    ConvertAnim(convertAnim);
                }

                GUILayout.Space(5);

                if (GUILayout.Button("ConvertPacket"))
                {
                    ConvertPacket(convertAnim);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private static void ConvertAnim(ConvertAnim convertAnim)
        {
            var binary = convertAnim.binary;
            if (!binary)
            {
                Debug.LogError("Binary is null");
                return;
            }
            if (string.IsNullOrEmpty(convertAnim.OutputDirectory))
            {
                Debug.LogError("Output directory is null or empty");
                return;
            }

            var bytes = binary.bytes;
            TimelineConverter timelineConverter = new(RecordData.Deserialize(bytes));
            timelineConverter.SaveDmxTimelineClips(convertAnim.OutputDirectory);

            Debug.Log("Conversion complete");
        }

        private static void ConvertPacket(ConvertAnim convertAnim)
        {
            var binary = convertAnim.binary;
            if (!binary)
            {
                Debug.LogError("Binary is null");
                return;
            }
            if (string.IsNullOrEmpty(convertAnim.OutputDirectory))
            {
                Debug.LogError("Output directory is null or empty");
                return;
            }

            var timelineSettingPath = convertAnim.OutputDirectory + "/DmxTimeline.asset";
            var dmxTimelineSetting = AssetDatabase.LoadAssetAtPath(timelineSettingPath, typeof(DmxTimelineSetting)) as DmxTimelineSetting;
            if (dmxTimelineSetting is null)
            {
                Debug.LogError("DmxTimelineSetting is null");
                return;
            }

            Debug.Log($"ArtNet Recorder: {dmxTimelineSetting.DmxTimelines.Count} timelines found");
            var timelineConverter = new TimelineConverter(dmxTimelineSetting);
            var dmxPackets = timelineConverter.ToDmxPackets();
            var storeData = RecordData.Serialize(dmxPackets);


            var path = convertAnim.OutputDirectory + "/DmxPackets.bytes";
            var exists = File.Exists(path);
            File.WriteAllBytes(path, storeData);
            var message = exists ? "Data updated" : "Data stored";
            Debug.Log($"ArtNet Recorder: {message} at {path}");
            Debug.Log("Conversion complete");
        }
    }
}
