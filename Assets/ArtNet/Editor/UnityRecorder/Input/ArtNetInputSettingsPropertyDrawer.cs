using ArtNet.Editor.DmxRecorder;
using UnityEditor;
using UnityEngine;

namespace ArtNet.Editor.UnityRecorder.Input
{
    [CustomPropertyDrawer(typeof(ArtNetInputSettings))]
    public class ArtNetInputSettingsPropertyDrawer : TargetedPropertyDrawer<ArtNetInputSettings>
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            EditorGUI.BeginChangeCheck();

            var dmxManager = EditorGUILayout.ObjectField(new GUIContent("DmxManager", "The reference to the GameObject with the DmxManager component"), Target.DmxManager, typeof(DmxManager), true) as DmxManager;

            if (EditorGUI.EndChangeCheck())
            {
                Target.GameObject = dmxManager?.gameObject;
            }
        }
    }
}
