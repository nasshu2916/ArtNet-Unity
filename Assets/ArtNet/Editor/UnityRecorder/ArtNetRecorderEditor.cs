using UnityEditor;
using UnityEditor.Recorder;
using UnityEngine;

namespace ArtNet.Editor.UnityRecorder
{
    [CustomEditor(typeof(ArtNetRecorderSettings))]
    public class ArtNetRecorderEditor : RecorderEditor
    {
        private SerializedProperty _outputFormat, _universeFilter, _isCompressBinary;

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
            _isCompressBinary = serializedObject.FindProperty("_isCompressBinary");
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
            if (_outputFormat?.enumValueIndex == (int) ArtNetRecorderSettings.ArtNetRecorderOutputFormat.Binary)
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_isCompressBinary);
            }
        }
    }
}
