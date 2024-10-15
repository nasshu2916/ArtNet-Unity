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
    }
}
