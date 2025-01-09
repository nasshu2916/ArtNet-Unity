using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    [CustomEditor(typeof(RecorderSettings), true)]
    public class RecorderSettingsEditor : UnityEditor.Editor
    {
        private static class Styles
        {
            internal static readonly GUIContent TakeNumberLabel = new("Take Number", "Value that the Recorder uses to number the recordings. It increases by one after each recording.");
        }

        internal event Action OnRecorderValidated;
        private SerializedProperty _universeFilter, _fileGenerator, _take;

        public void OnEnable()
        {
            if (target == null) return;

            _universeFilter = serializedObject.FindProperty("_universeFilter");
            _fileGenerator = serializedObject.FindProperty("_fileGenerator");
            _take = serializedObject.FindProperty("_take");
        }

        public override void OnInspectorGUI()
        {
            if (target == null)
                return;

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            DrawHeader("Output File");
            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(_universeFilter, GUIContent.none);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_fileGenerator, GUIContent.none);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_take, Styles.TakeNumberLabel);

            EditorGUILayout.Separator();

            EditorGUILayout.Separator();
            serializedObject.ApplyModifiedProperties();

            EditorGUI.EndChangeCheck();

            OnValidateSettingsGUI();
        }

        private void OnValidateSettingsGUI()
        {
            var targetSettings = (RecorderSettings) target;

            var warnings = new List<string>();
            var errors = new List<string>();

            targetSettings.GetWarnings(warnings);
            foreach (var w in warnings)
                EditorGUILayout.HelpBox(w, MessageType.Warning);

            targetSettings.GetErrors(errors);
            foreach (var e in errors)
                EditorGUILayout.HelpBox(e, MessageType.Error);

            if (warnings.Count > 0 || errors.Count > 0)
                InvokeRecorderValidated();
        }

        private void InvokeRecorderValidated()
        {
            OnRecorderValidated?.Invoke();
        }

        private static void DrawHeader(string title)
        {
            const float height = 17f;
            var backgroundRect = GUILayoutUtility.GetRect(1f, height);

            var labelRect = backgroundRect;
            labelRect.xMin += 8f;
            labelRect.xMax -= 20f;

            backgroundRect.xMin = 0f;
            backgroundRect.width += 4f;

            var backgroundTint = EditorGUIUtility.isProSkin ? 0.1f : 1f;
            EditorGUI.DrawRect(backgroundRect, new Color(backgroundTint, backgroundTint, backgroundTint, 0.2f));

            EditorGUI.LabelField(labelRect, new GUIContent(title), EditorStyles.boldLabel);
        }
    }
}
