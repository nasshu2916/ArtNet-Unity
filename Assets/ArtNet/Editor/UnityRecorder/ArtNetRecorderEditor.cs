using UnityEditor;
using UnityEditor.Recorder;
using UnityEngine;

namespace ArtNet.Editor.UnityRecorder
{
    [CustomEditor(typeof(ArtNetRecorderSettings))]
    public class ArtNetRecorderEditor : RecorderEditor
    {
        private SerializedProperty _outputFormat, _universeFilter;

        private static class Styles
        {
            internal static readonly GUIContent FormatLabel = new("Art-Net File Format", "The file encoding format of the Art-Net recording output.");
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (target == null)
                return;

            _outputFormat = serializedObject.FindProperty("_outputFormat");
            _universeFilter = serializedObject.FindProperty("_universeFilter");
        }

        protected override void ExtraOptionsGUI()
        {
            base.ExtraOptionsGUI();

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_universeFilter, Styles.FormatLabel);
        }

        protected override void FileTypeAndFormatGUI()
        {
            EditorGUILayout.PropertyField(_outputFormat, Styles.FormatLabel);
        }
    }
}
