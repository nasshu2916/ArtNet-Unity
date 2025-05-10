using ArtNet.Common;
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
        private bool _updateDmx = true;
        private Label _infoLabel;

        [MenuItem(Const.Editor.MenuItemNamePrefix + "DmxManagerViewer", false, Const.Editor.Priority)]
        public static void ShowWindow()
        {
            var wnd = GetWindow<DmxManagerViewer>();
            wnd.titleContent = new GUIContent("DmxManagerViewer");
        }

        private void CreateGUI()
        {
            minSize = new Vector2(1200, 500);
            if (_dmxManager == null) _dmxManager = FindObjectOfType<DmxManager>();

            var root = rootVisualElement;
            var visualTree = Resources.Load<VisualTreeAsset>("DmxManagerViewer");
            var visualElement = visualTree.Instantiate();

            var dmxManagerObjectField = visualElement.Q<ObjectField>("DmxManagerObjectField");
            dmxManagerObjectField.objectType = typeof(DmxManager);
            dmxManagerObjectField.value = _dmxManager;
            dmxManagerObjectField.RegisterValueChangedCallback(evt =>
            {
                _dmxManager = (DmxManager) evt.newValue;
                _universeViewer.DmxManager = _dmxManager;
            });
            _infoLabel = visualElement.Q<Label>("DmxManagerObjectFieldInfo");
            var updateDmxToggle = visualElement.Q<Toggle>("UpdateDmxToggle");
            updateDmxToggle.value = _updateDmx;
            updateDmxToggle.RegisterValueChangedCallback(evt =>
            {
                _updateDmx = evt.newValue;
                UpdateInfoText();
            });

            _universeViewer = visualElement.Q<UniverseViewer>();
            _universeViewer.DmxManager = _dmxManager;

            root.Add(visualElement);
            var styleSheet = Resources.Load<StyleSheet>("DmxManagerViewer");
            root.styleSheets.Add(styleSheet);
        }

        private void Update()
        {
            UpdateInfoText();
            if (_updateDmx) _universeViewer.UpdateDmxViewer();
        }

        private void UpdateInfoText()
        {
            _infoLabel.text = (_dmxManager, _updateDmx) switch
            {
                (null, _) => "DmxManager UnSelected",
                (_, false) => "Update Dmx Values is false",
                _ => ""
            };
        }
    }
}
