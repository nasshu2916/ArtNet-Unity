using UnityEditor;
using UnityEditor.Recorder;

namespace ArtNet.UnityRecorder
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
