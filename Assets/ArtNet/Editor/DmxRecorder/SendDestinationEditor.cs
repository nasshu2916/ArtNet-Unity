using UnityEditor;

namespace ArtNet.Editor.DmxRecorder
{
    [CustomEditor(typeof(SendDestination))]
    public class SendDestinationEditor : UnityEditor.Editor
    {
        private SerializedProperty _ip, _port, _isSend;

        public void OnEnable()
        {
            if (target == null) return;

            _ip = serializedObject.FindProperty("_ip");
            _port = serializedObject.FindProperty("_port");
            _isSend = serializedObject.FindProperty("_isSend");
        }

        public override void OnInspectorGUI()
        {
            if (target == null)
                return;

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            EditorGUILayout.PropertyField(_ip);
            EditorGUILayout.PropertyField(_port);
            EditorGUILayout.PropertyField(_isSend);

            serializedObject.ApplyModifiedProperties();
            EditorGUI.EndChangeCheck();
        }
    }
}
