using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ArtNet.UnityRecorder.Input
{
    [CustomPropertyDrawer(typeof(ArtNetInputSettings))]
    public class ArtNetInputSettingsPropertyDrawer : PropertyDrawer
    {
        private ArtNetInputSettings _target;

        private void Initialize(SerializedProperty prop)
        {
            var path = prop.propertyPath.Split('.');
            object obj = prop.serializedObject.targetObject;

            foreach (var pathNode in path)
                obj = GetSerializedField(obj, pathNode).GetValue(obj);

            _target = obj as ArtNetInputSettings;
        }

        private static FieldInfo GetSerializedField(object target, string pathNode)
        {
            return target.GetType().GetField(pathNode, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0.0f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            EditorGUI.BeginChangeCheck();

            var dmxManager = EditorGUILayout.ObjectField(new GUIContent("DmxManager", "The reference to the GameObject with the DmxManager component"), _target.DmxManager, typeof(DmxManager), true) as DmxManager;

            if (EditorGUI.EndChangeCheck())
            {
                _target.GameObject = dmxManager?.gameObject;
            }
        }
    }
}
