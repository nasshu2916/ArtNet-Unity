using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtNet.Editor.DmxRecorder
{
    public partial class DmxRecordWindow : EditorWindow
    {
        private const string EditorSettingPrefix = "ArtNet.DmxRecorder.";

        [SerializeField] private VisualTreeAsset visualTree, recorderVisualTree, senderVisualTree;
        [SerializeField] private StyleSheet styleSheet;

        private Texture _playButtonTexture, _preMatQuadTexture;

        private void Update()
        {
            UpdateRecorder();
            UpdateSender();
        }

        public void CreateGUI()
        {
            minSize = new Vector2(375, 400);
            var root = rootVisualElement;

            VisualElement visualElement = visualTree.Instantiate();
            visualElement.AddToClassList("root");
            root.Add(visualElement);
            root.styleSheets.Add(styleSheet);

            _playButtonTexture = EditorGUIUtility.IconContent("PlayButton@2x").image;
            _preMatQuadTexture = EditorGUIUtility.IconContent("PreMatQuad@2x").image;

            Initialize(visualElement);
        }

        [MenuItem("ArtNet/DmxRecorder")]
        public static void ShowDmxRecorder()
        {
            var window = GetWindow<DmxRecordWindow>();
            window.titleContent = new GUIContent("DmxRecorder");
        }

        private void Initialize(VisualElement root)
        {
            var tabContent = new VisualElement { name = "tabContent" };
            root.Add(tabContent);

            VisualElement recorderVisualElement = recorderVisualTree.CloneTree();
            recorderVisualElement.name = "recorderPanel";
            tabContent.Add(recorderVisualElement);
            VisualElement senderVisualElement = senderVisualTree.Instantiate();
            senderVisualElement.name = "senderPanel";
            tabContent.Add(senderVisualElement);

            var config = new RecordConfig
            {
                Directory = EditorUserSettings.GetConfigValue(EditorSettingKey("OutputDirectory")) ??
                            Application.dataPath,
                FileName = EditorUserSettings.GetConfigValue(EditorSettingKey("OutputFileName")) ??
                           "dmx-record"
            };
            _recorder.Config = config;

            InitializeHeaderTab(root);
            InitializeRecorder(recorderVisualElement);
            InitializeSender(senderVisualElement);

            _footerStatusLabel = root.Q<Label>("footerStatusLabel");
        }

        private void InitializeHeaderTab(VisualElement root)
        {
            var headerRecorderLabel = root.Q<Label>("headerRecorderLabel");
            var headerSenderLabel = root.Q<Label>("headerSenderLabel");

            var recorderPanel = root.Q<VisualElement>("recorderPanel");
            var senderPanel = root.Q<VisualElement>("senderPanel");

            headerRecorderLabel.AddToClassList("selected");
            senderPanel.style.display = DisplayStyle.None;
            recorderPanel.style.display = DisplayStyle.Flex;

            headerRecorderLabel.RegisterCallback<MouseUpEvent>(_ =>
            {
                headerRecorderLabel.AddToClassList("selected");
                headerSenderLabel.RemoveFromClassList("selected");
                recorderPanel.style.display = DisplayStyle.Flex;
                senderPanel.style.display = DisplayStyle.None;
            });

            headerSenderLabel.RegisterCallback<MouseUpEvent>(_ =>
            {
                headerSenderLabel.AddToClassList("selected");
                headerRecorderLabel.RemoveFromClassList("selected");
                senderPanel.style.display = DisplayStyle.Flex;
                recorderPanel.style.display = DisplayStyle.None;
            });
        }

        private static string EditorSettingKey(string key) => $"{EditorSettingPrefix}{key}";
    }
}
