using UnityEditor;
using UnityEditor.Recorder;
using UnityEngine;

namespace ArtNet.Editor.UnityRecorder
{
    [CustomEditor(typeof(ArtNetRecorderSettings))]
    public class ArtNetRecorderEditor : RecorderEditor
    {
        private SerializedProperty _outputFormat;

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
        }

        protected override void FileTypeAndFormatGUI()
        {
            EditorGUILayout.PropertyField(_outputFormat, Styles.FormatLabel);
        }
    }
}
