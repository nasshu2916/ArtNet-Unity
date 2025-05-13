using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    public class TargetedPropertyDrawer<T> : PropertyDrawer where T : class
    {
        protected T Target;

        protected virtual void Initialize(SerializedProperty prop)
        {
            var path = prop.propertyPath.Split('.');
            object obj = prop.serializedObject.targetObject;

            foreach (var pathNode in path)
                obj = GetSerializedField(obj, pathNode).GetValue(obj);

            Target = obj as T;
        }

        private static FieldInfo GetSerializedField(object target, string pathNode)
        {
            return target.GetType().GetField(pathNode, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0.0f;
        }
    }
}
