using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtNet.Editor.DmxRecorder
{
    [CustomEditor(typeof(SendDestination))]
    public class SendDestinationEditor : UnityEditor.Editor
    {
        private SerializedProperty _ip, _port;

        public Action OnValueChanged;

        public void OnEnable()
        {
            if (target == null) return;

            _ip = serializedObject.FindProperty("_ip");
            _port = serializedObject.FindProperty("_port");
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
            ipField.RegisterCallback<ChangeEvent<string>>(_ => OnValueChanged?.Invoke());
            root.Add(ipField);

            var portField = new PropertyField(_port)
            {
                label = "Port",
                tooltip = "The port number of the ArtNet DMX sender."
            };
            portField.Bind(serializedObject);
            portField.RegisterCallback<ChangeEvent<int>>(_ => OnValueChanged?.Invoke());
            root.Add(portField);

            return root;
        }
    }
}
