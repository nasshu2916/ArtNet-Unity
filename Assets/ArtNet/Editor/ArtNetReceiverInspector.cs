using UnityEditor;
using UnityEngine;

namespace ArtNet.Editor
{
    [CustomEditor(typeof(ArtNetReceiver))]
    public class ArtNetReceiverInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var artNetReceiver = target as ArtNetReceiver;

            DrawDefaultInspector();

            if (artNetReceiver == null) return;

            GUILayout.Space(5);

            var defaultColor = GUI.backgroundColor;
            var style = new GUIStyle(GUI.skin.label);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                GUI.color = Color.cyan;
                GUILayout.Label($"ArtNet Information", style);
                GUI.color = defaultColor;

                GUILayout.Space(10);
                EditorGUILayout.LabelField($"Connected : {artNetReceiver.IsConnected}");
                EditorGUILayout.LabelField($"Last Received At : {artNetReceiver.LastReceivedAt:MM/dd HH:mm:ss fff}",
                    style);
            }
            EditorGUILayout.EndVertical();
        }
    }
}
