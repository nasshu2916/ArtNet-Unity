using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArtNet.Editor
{
    public class DmxViewer : EditorWindow
    {
        private DmxDataManager _dmxDataManager;
        private int _selectedUniverseIndex;
        private Vector2 _scrollPosition;
        private const int RowDisplayNumber = 20;
        private const int DisplayMaxHeight = 300;
        private readonly GUILayoutOption _dmxLayoutOption = GUILayout.MaxWidth(25);

        [MenuItem("Tools/DmxViewer")]
        private static void Open()
        {
            var window = GetWindow<DmxViewer>("DmxViewer");
            window.Show();
        }

        private void OnEnable()
        {
            _dmxDataManager = FindObjectOfType<DmxDataManager>();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("ArtNet Receiver", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Dmx Data Manager", _dmxDataManager, typeof(DmxDataManager), true);
            EditorGUI.EndDisabledGroup();
            GUILayout.Box("", GUILayout.Width(position.width), GUILayout.Height(1));

            EditorGUILayout.LabelField("ArtNet Client", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.EndDisabledGroup();

            var universes = _dmxDataManager.Universes();
            var options = universes.Select(universe => $"Universe: {universe + 1}").ToArray();
            _selectedUniverseIndex = EditorGUILayout.Popup("Universe", _selectedUniverseIndex, options);

            if (universes.Length == 0) return;
            var selectedUniverse = universes[_selectedUniverseIndex];
            if (!universes.Contains(selectedUniverse))
            {
                EditorGUILayout.HelpBox("Universe not found", MessageType.Error);
                return;
            }

            var dmxValues = _dmxDataManager.DmxValues(selectedUniverse);

            _scrollPosition =
                EditorGUILayout.BeginScrollView(_scrollPosition, GUI.skin.box, GUILayout.MaxHeight(DisplayMaxHeight));
            {
                EditorGUILayout.BeginHorizontal();
                for (var i = 0; i < dmxValues.Length; i++)
                {
                    var dmxValue = dmxValues[i];
                    GUI.backgroundColor = new Color(0, (float) dmxValue / 256 * 5, 0, 1);
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
        }
    }
}