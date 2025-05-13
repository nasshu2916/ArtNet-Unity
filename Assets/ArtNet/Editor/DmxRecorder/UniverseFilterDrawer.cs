using UnityEditor;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    [CustomPropertyDrawer(typeof(UniverseFilter))]
    public class UniverseFilterDrawer : TargetedPropertyDrawer<UniverseFilter>
    {
        private static class Styles
        {
            internal static readonly GUIContent UniverseFilterLabel = new("Universe Filter", "Filter the universes to record");
            internal static readonly GUIContent FilterTextInfo = new("Enter the universes to record. Example: 0-2, 4, 7-9");
        }

        private UniverseFilter _target;

        private SerializedProperty _enabled;
        private SerializedProperty _filterText;

        protected override void Initialize(SerializedProperty property)
        {
            base.Initialize(property);

            _enabled = property.FindPropertyRelative("_enabled");
            _filterText = property.FindPropertyRelative("_filterText");
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            EditorGUI.BeginProperty(position, label, property);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Styles.UniverseFilterLabel, GUILayout.Width(EditorGUIUtility.labelWidth));

                EditorGUILayout.PropertyField(_enabled, GUIContent.none, GUILayout.Width(20));

                EditorGUILayout.BeginVertical();
                using (new EditorGUI.DisabledScope(_enabled.boolValue == false))
                {
                    var filterText = EditorGUILayout.TextField(_filterText.stringValue);
                    if (filterText != _filterText.stringValue)
                    {
                        _filterText.stringValue = filterText;
                        Target.FilterText = filterText;
                    }

                    EditorGUILayout.LabelField(Styles.FilterTextInfo, EditorStyles.miniLabel);
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUI.EndProperty();
        }
    }
}
