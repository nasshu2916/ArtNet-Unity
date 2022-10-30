using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArtNet.Editor
{
    [CustomEditor(typeof(ArtNetReceiver))]
    public class DmxEditor : UnityEditor.Editor
    {
        private const int RowDisplayNumber = 20;
        private const int DisplayMaxHeight = 300;
        private ArtNetReceiver _receiver;
        private byte _selectedUniverse;
        private readonly GUILayoutOption _dmxLayoutOption = GUILayout.MaxWidth(25);
        private Vector2 _scrollPosition = Vector2.zero;
        private bool _autoRepaint;

        public override void OnInspectorGUI()
        {
            _receiver = (ArtNetReceiver)target;
            base.OnInspectorGUI();
            EditorGUILayout.LabelField("ArtNet Receiver", EditorStyles.boldLabel);
            _autoRepaint = EditorGUILayout.Toggle("AutoRepaint", _autoRepaint);

            var options = Enumerable.Range(1, ArtNetReceiver.MaxUniverse)
                .Select(number => $"Universe {number}")
                .ToArray();
            _selectedUniverse = (byte)EditorGUILayout.Popup("Universe", _selectedUniverse, options);

            var dmx = _receiver.GetDmx(_selectedUniverse) ?? new byte[512];
            _scrollPosition =
                EditorGUILayout.BeginScrollView(_scrollPosition, GUI.skin.box, GUILayout.MaxHeight(DisplayMaxHeight));
            {
                EditorGUILayout.BeginHorizontal();
                for (var i = 0; i < dmx.Length; i++)
                {
                    var dmxValue = dmx[i];
                    GUI.backgroundColor = new Color(0, (float)dmxValue / 256 * 5, 0, 1);
                    EditorGUILayout.BeginVertical(GUI.skin.box, _dmxLayoutOption);
                    EditorGUILayout.LabelField((i + 1).ToString(), _dmxLayoutOption);
                    EditorGUILayout.LabelField(dmxValue == 0 ? "" : dmxValue.ToString(), EditorStyles.boldLabel,
                        _dmxLayoutOption);
                    EditorGUILayout.EndVertical();

                    if (i % RowDisplayNumber != RowDisplayNumber - 1) continue;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            if (_autoRepaint)
            {
                Repaint();
            }
        }
    }
}
