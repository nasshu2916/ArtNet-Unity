using ArtNet.Editor.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtNet.Editor
{
    public class DmxManagerViewer : EditorWindow
    {
        private DmxManager _dmxManager;
        private UniverseViewer _universeViewer;

        [MenuItem("ArtNet/DmxManagerViewer")]
        public static void ShowExample()
        {
            var wnd = GetWindow<DmxManagerViewer>();
            wnd.titleContent = new GUIContent("DmxManagerViewer");
        }

        public void CreateGUI()
        {
            minSize = new Vector2(1200, 500);
            if (_dmxManager == null) _dmxManager = FindObjectOfType<DmxManager>();
            if (_dmxManager == null) return;

            var root = rootVisualElement;
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/ArtNet/Editor/DmxManagerViewer.uxml");
            root.Add(visualTree.Instantiate());

            var dmxManagerObjectField = root.Q<ObjectField>("DmxManagerObjectField");
            dmxManagerObjectField.value = _dmxManager;
            _universeViewer = root.Q<UniverseViewer>();
            _universeViewer.DmxManager = _dmxManager;

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/ArtNet/Editor/DmxManagerViewer.uss");
            root.styleSheets.Add(styleSheet);
        }

        public void OnGUI()
        {
            _universeViewer?.UpdateDmxViewer();
        }
    }
}
