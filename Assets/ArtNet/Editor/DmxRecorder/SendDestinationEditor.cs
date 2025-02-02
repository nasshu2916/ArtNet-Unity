using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

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

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var ipField = new PropertyField(_ip)
            {
                label = "IP Address",
                tooltip = "The IP address of the ArtNet DMX sender."
            };
            ipField.Bind(serializedObject);
            root.Add(ipField);

            var portField = new PropertyField(_port)
            {
                label = "Port",
                tooltip = "The port number of the ArtNet DMX sender."
            };
            portField.Bind(serializedObject);
            root.Add(portField);

            return root;
        }
    }
}
