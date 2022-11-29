using UnityEditor;
using UnityEngine;

namespace ArtNet.Editor
{
    public class DmxViewer : EditorWindow
    {
        private const int MaxUniverses = 32768;

        private ArtNetReceiver _artNetReceiver;
        private DmxDataManager _dmxDataManager;
        [Range(1, MaxUniverses)] private int _selectedUniverse;
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
            _artNetReceiver = FindObjectOfType<ArtNetReceiver>();
            _dmxDataManager = _artNetReceiver.dmxDataManager;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("ArtNet Receiver", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("ArtNet Receive Object", _artNetReceiver, typeof(ArtNetReceiver), true);
            EditorGUI.EndDisabledGroup();
            GUILayout.Box("", GUILayout.Width(this.position.width), GUILayout.Height(1));

            EditorGUILayout.LabelField("ArtNet Client", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("LastReceiveAt", _artNetReceiver.ArtClient?.LastReceiveAt.ToString("yyyy/MM/dd HH:mm:ss.fff"));
            EditorGUI.EndDisabledGroup();

            _selectedUniverse = EditorGUILayout.IntField("Universe", _selectedUniverse);
            _selectedUniverse = Mathf.Clamp(_selectedUniverse, 1, MaxUniverses);
            if (!_dmxDataManager.DmxMap.ContainsKey(_selectedUniverse - 1))
            {
                EditorGUILayout.HelpBox("Universe not found", MessageType.Error);
                return;
            }

            var dmx = _dmxDataManager.GetDmx(_selectedUniverse - 1);

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
        }
    }
}
