using UnityEditor;
using UnityEngine;

namespace ArtNet.Editor
{
    [CustomEditor(typeof(ArtNetReceiver))]
    public class ArtNetReceiverInspector : UnityEditor.Editor
    {
        private const float FoldoutLeftMargin = 14f;
        private const float ContentLeftMargin = 18f;

        private readonly (string PropertyName, string Label, string Description)[] _handlers =
        {
            ("_onReceivedPollEvent", "OpPoll", "Node discovery request"),
            ("_onReceivedPollReplyEvent", "OpPollReply", "Node discovery reply"),
            ("_onReceivedDmxEvent", "OpDmx", "DMX512 payload"),
            ("_onReceivedSyncEvent", "OpSync", "Frame synchronization"),
            ("_onReceivedTimeCodeEvent", "OpTimeCode", "Time code signal"),
            ("_onReceivedAddressEvent", "OpAddress", "Node addressing"),
            ("_onReceivedTodRequestEvent", "OpTodRequest", "RDM TOD request"),
            ("_onReceivedTodDataEvent", "OpTodData", "RDM TOD data"),
            ("_onReceivedTodControlEvent", "OpTodControl", "RDM TOD control"),
            ("_onReceivedRdmEvent", "OpRdm", "RDM message"),
        };

        private bool[] _handlerFoldouts;

        public override void OnInspectorGUI()
        {
            var artNetReceiver = target as ArtNetReceiver;
            serializedObject.Update();

            EnsureFoldoutStateBuffer();

            DrawHeader();
            DrawReceiveSettings();
            DrawHandlers();

            serializedObject.ApplyModifiedProperties();

            if (artNetReceiver == null) return;

            DrawRuntimeStatus(artNetReceiver);
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(4);
            using var __ = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
            GUILayout.Label("ArtNet Receiver", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Configure receive callbacks per OpCode.", EditorStyles.miniLabel);
            EditorGUILayout.Space(2);
        }

        private void DrawReceiveSettings()
        {
            using var __ = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
            GUILayout.Label("Receive Settings", EditorStyles.boldLabel);
            var autoStart = serializedObject.FindProperty("_autoStart");
            if (autoStart != null) EditorGUILayout.PropertyField(autoStart, new GUIContent("Auto Start"));
        }

        private void DrawHandlers()
        {
            using var __ = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
            GUILayout.Label("OpCode Handlers", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Expand All")) SetAllFoldoutState(true);
                if (GUILayout.Button("Collapse All")) SetAllFoldoutState(false);
            }

            EditorGUILayout.Space(2);
            for (var i = 0; i < _handlers.Length; i++)
            {
                DrawHandler(i);
            }
        }

        private void DrawRuntimeStatus(ArtNetReceiver artNetReceiver)
        {
            EditorGUILayout.Space(4);
            using var __ = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
            GUILayout.Label("Runtime Status", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Connected", artNetReceiver.IsConnected ? "Yes" : "No");
            EditorGUILayout.LabelField("Last Received", artNetReceiver.LastReceivedAt.ToString("MM/dd HH:mm:ss fff"));
        }

        private void DrawHandler(int index)
        {
            var handler = _handlers[index];
            var propertyName = handler.PropertyName;
            var property = serializedObject.FindProperty(propertyName);
            if (property == null) return;

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(FoldoutLeftMargin);
                _handlerFoldouts[index] = EditorGUILayout.Foldout(
                    _handlerFoldouts[index],
                    new GUIContent(handler.Label, handler.Description),
                    true
                );
            }
            if (_handlerFoldouts[index] == false) return;

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(ContentLeftMargin);
                EditorGUILayout.PropertyField(property, GUIContent.none, true);
            }
            EditorGUILayout.Space(2);
        }

        private void SetAllFoldoutState(bool isExpanded)
        {
            EnsureFoldoutStateBuffer();
            for (var i = 0; i < _handlerFoldouts.Length; i++)
            {
                _handlerFoldouts[i] = isExpanded;
            }
        }

        private void EnsureFoldoutStateBuffer()
        {
            if (_handlerFoldouts != null && _handlerFoldouts.Length == _handlers.Length) return;
            _handlerFoldouts = new bool[_handlers.Length];
        }
    }
}
