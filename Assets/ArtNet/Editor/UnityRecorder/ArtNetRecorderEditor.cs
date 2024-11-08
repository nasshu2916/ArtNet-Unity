using UnityEditor;
using UnityEditor.Recorder;

namespace ArtNet.Editor.UnityRecorder
{
    [CustomEditor(typeof(ArtNetRecorderSettings))]
    public class ArtNetRecorderEditor : RecorderEditor
    {
        protected override void FileTypeAndFormatGUI()
        {
            EditorGUILayout.LabelField("Format", "ArtNet Binary");
        }
    }
}
