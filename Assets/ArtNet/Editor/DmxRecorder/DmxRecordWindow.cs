using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtNet.Editor.DmxRecorder
{
    public partial class DmxRecordWindow : EditorWindow
    {
        private const string EditorSettingPrefix = "ArtNet.DmxRecorder.";

        private static readonly Color RecordingColor = new(0.78f, 0f, 0f, 1f);
        private static readonly Color PausedColor = new(0.78f, 0.5f, 0f, 1f);
        [SerializeField] private VisualTreeAsset visualTree;
        [SerializeField] private StyleSheet styleSheet;

        private readonly Recorder _recorder = new();

        private Texture _playButtonTexture, _preMatQuadTexture;

        private void Update()
        {
            var timeCode = _recorder.GetRecordingTime();
            var timeCodeSpan = TimeSpan.FromSeconds(timeCode / 1000f);
            _timeCodeHourLabel.text = timeCodeSpan.Hours.ToString("00");
            _timeCodeMinuteLabel.text = timeCodeSpan.Minutes.ToString("00");
            _timeCodeSecondLabel.text = timeCodeSpan.Seconds.ToString("00");
            _timeCodeMillisecondLabel.text = Math.Floor(timeCodeSpan.Milliseconds / 10.0f).ToString("00");

            var recordCount = _recorder.GetRecordedCount();
            _footerStatusLabel.text = _recorder.Status switch
            {
                RecordingStatus.Recording => $"Recording. {recordCount} packet recorded",
                RecordingStatus.Paused => $"Paused. {recordCount} packet recorded",
                _ => ""
            };

            UpdateSender();
        }

        public void CreateGUI()
        {
            minSize = new Vector2(375, 400);
            var root = rootVisualElement;

            VisualElement recorderVisualElement = visualTree.Instantiate();
            recorderVisualElement.AddToClassList("root");
            root.Add(recorderVisualElement);
            root.styleSheets.Add(styleSheet);

            _timeCodeContainer = root.Q<VisualElement>("timeCodeContainer");
            _timeCodeHourLabel = root.Q<Label>("tcHour");
            _timeCodeMinuteLabel = root.Q<Label>("tcMinute");
            _timeCodeSecondLabel = root.Q<Label>("tcSecond");
            _timeCodeMillisecondLabel = root.Q<Label>("tcMillisecond");

            _playButtonTexture = EditorGUIUtility.IconContent("PlayButton@2x").image;
            _preMatQuadTexture = EditorGUIUtility.IconContent("PreMatQuad@2x").image;

            Initialize(root);
        }

        [MenuItem("ArtNet/DmxRecorder")]
        public static void ShowDmxRecorder()
        {
            var window = GetWindow<DmxRecordWindow>();
            window.titleContent = new GUIContent("DmxRecorder");
        }

        private void Initialize(VisualElement root)
        {
            var config = new RecordConfig
            {
                Directory = EditorUserSettings.GetConfigValue(EditorSettingKey("OutputDirectory")) ??
                            Application.dataPath,
                FileName = EditorUserSettings.GetConfigValue(EditorSettingKey("OutputFileName")) ??
                           "dmx-record"
            };
            _recorder.Config = config;

            InitializeHeaderTab(root);
            InitializeControlPanel(root);
            InitializeRecordingConfig(root);

            InitializeSender(root);

            _footerStatusLabel = root.Q<Label>("footerStatusLabel");
        }

        private void InitializeHeaderTab(VisualElement root)
        {
            var headerRecorderLabel = root.Q<Label>("headerRecorderLabel");
            var headerSenderLabel = root.Q<Label>("headerSenderLabel");

            var recorderPanel = root.Q<VisualElement>("recorderPanel");
            var senderPanel = root.Q<VisualElement>("senderPanel");

            headerSenderLabel.AddToClassList("selected");
            senderPanel.style.display = DisplayStyle.Flex;
            recorderPanel.style.display = DisplayStyle.None;

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
