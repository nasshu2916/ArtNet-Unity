using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtNet.Editor.DmxRecorder
{
    public class DmxRecordWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset visualTree;
        [SerializeField] private StyleSheet styleSheet;

        private Label _timeCodeHourLabel, _timeCodeMinuteLabel, _timeCodeSecondLabel, _timeCodeMillisecondLabel;
        private VisualElement _timeCodeContainer;

        private static readonly Color RecordingColor = new(0.78f, 0f, 0f, 1f);
        private static readonly Color PausedColor = new(0.78f, 0.5f, 0f, 1f);

        private readonly DmxRecorder _recorder = new();
        private string _outputDirectory, _outputFileName;

        private const string EditorSettingPrefix = "ArtNet.DmxRecorder.";

        [MenuItem("ArtNet/DmxRecorder")]
        public static void ShowDmxRecorder()
        {
            var window = GetWindow<DmxRecordWindow>();
            window.titleContent = new GUIContent("DmxRecorder");
        }

        public void CreateGUI()
        {
            _outputDirectory = EditorUserSettings.GetConfigValue(EditorSettingKey("OutputDirectory")) ??
                               Application.dataPath;
            _outputFileName = EditorUserSettings.GetConfigValue(EditorSettingKey("OutputFileName")) ?? "dmx-record";

            var root = rootVisualElement;

            VisualElement recorderVisualElement = visualTree.Instantiate();
            recorderVisualElement.AddToClassList("root");
            root.Add(recorderVisualElement);
            root.styleSheets.Add(styleSheet);

            _timeCodeContainer = root.Q<VisualElement>("time-code-container");
            _timeCodeHourLabel = root.Q<Label>("tc-hour");
            _timeCodeMinuteLabel = root.Q<Label>("tc-minute");
            _timeCodeSecondLabel = root.Q<Label>("tc-second");
            _timeCodeMillisecondLabel = root.Q<Label>("tc-millisecond");

            Initialize(root);
        }

        private void Initialize(VisualElement root)
        {
            InitializeControlPanel(root);
            InitializeRecordingConfig(root);
        }

        private void InitializeControlPanel(VisualElement root)
        {
            var startButtonImage = new Image
                { image = EditorGUIUtility.IconContent("PlayButton@2x").image };
            var stopButtonImage = new Image
            {
                image = EditorGUIUtility.IconContent("PreMatQuad@2x").image,
                style =
                {
                    display = DisplayStyle.None
                }
            };
            var playButton = root.Q<Button>("play-button");
            var pauseButton = root.Q<Button>("pause-button");

            playButton.Add(startButtonImage);
            playButton.Add(stopButtonImage);
            playButton.clicked += () =>
            {
                if (_recorder.Status == RecordingStatus.None)
                {
                    _recorder.StartRecording();

                    startButtonImage.style.display = DisplayStyle.None;
                    stopButtonImage.style.display = DisplayStyle.Flex;

                    _timeCodeContainer.style.backgroundColor = RecordingColor;
                    pauseButton.SetEnabled(true);
                }
                else
                {
                    _recorder.StopRecording();

                    startButtonImage.style.display = DisplayStyle.Flex;
                    stopButtonImage.style.display = DisplayStyle.None;

                    _timeCodeContainer.style.backgroundColor = default;
                    pauseButton.RemoveFromClassList("selected");
                    pauseButton.SetEnabled(false);
                }
            };

            pauseButton.SetEnabled(false);
            pauseButton.Add(new Image()
            {
                image = EditorGUIUtility.IconContent("PauseButton@2x").image
            });
            pauseButton.clicked += () =>
            {
                switch (_recorder.Status)
                {
                    case RecordingStatus.Recording:
                        _recorder.PauseRecording();

                        pauseButton.AddToClassList("selected");
                        _timeCodeContainer.style.backgroundColor = PausedColor;
                        break;
                    case RecordingStatus.Paused:
                        _recorder.ResumeRecording();

                        pauseButton.RemoveFromClassList("selected");
                        _timeCodeContainer.style.backgroundColor = RecordingColor;
                        break;
                }
            };
        }

        private void InitializeRecordingConfig(VisualElement root)
        {
            var outputFileNameLabel = root.Q<Label>("output-file-name");

            // 出力ファイル名の設定
            var outputFileNameField = root.Q<TextField>("output-file-name-field");
            outputFileNameField.value = _outputFileName;
            outputFileNameField.RegisterValueChangedCallback(evt =>
            {
                _outputFileName = evt.newValue;
                outputFileNameLabel.text = GetOutputFilePath();
                EditorUserSettings.SetConfigValue(EditorSettingKey("OutputFileName"), _outputFileName);
            });

            // 出力ディレクトリの設定
            var outputDirectoryField = root.Q<TextField>("output-directory-field");
            outputDirectoryField.value = _outputDirectory;
            outputDirectoryField.RegisterValueChangedCallback(evt =>
            {
                _outputDirectory = evt.newValue;
                outputFileNameLabel.text = GetOutputFilePath();
                EditorUserSettings.SetConfigValue(EditorSettingKey("OutputDirectory"), _outputDirectory);
            });
            var selectDirectoryButton = root.Q<Button>("select-folder-button");
            selectDirectoryButton.Add(new Image()
                {
                    image = EditorGUIUtility.IconContent("Folder Icon").image
                }
            );
            selectDirectoryButton.clicked += () =>
            {
                var selectedDirectory =
                    EditorUtility.OpenFolderPanel(title: "Output Folder",
                        folder: _outputDirectory,
                        defaultName: "");

                if (string.IsNullOrEmpty(selectedDirectory)) return;

                _outputDirectory = selectedDirectory;
                outputDirectoryField.value = _outputDirectory;
                outputFileNameLabel.text = GetOutputFilePath();
                EditorUserSettings.SetConfigValue(EditorSettingKey("OutputDirectory"), _outputDirectory);
            };


            var openOutputFolderButton = root.Q<Button>("open-output-folder-button");
            openOutputFolderButton.Add(new Image()
                {
                    image = EditorGUIUtility.IconContent("FolderOpened Icon").image
                }
            );
            openOutputFolderButton.clicked += () =>
            {
                Process.Start(_outputDirectory);
            };

            outputFileNameLabel.text = GetOutputFilePath();
        }

        private void Update()
        {
            var timeCode = _recorder.GetRecordingTime();
            var timeCodeSpan = TimeSpan.FromSeconds(timeCode / 1000f);
            _timeCodeHourLabel.text = timeCodeSpan.Hours.ToString("00");
            _timeCodeMinuteLabel.text = timeCodeSpan.Minutes.ToString("00");
            _timeCodeSecondLabel.text = timeCodeSpan.Seconds.ToString("00");
            _timeCodeMillisecondLabel.text = Math.Floor(timeCodeSpan.Milliseconds / 10.0f).ToString("00");
        }

        private static string EditorSettingKey(string key) => $"{EditorSettingPrefix}.{key}";
        private string GetOutputFilePath() => $"{_outputDirectory}/{_outputFileName}.dmx";
    }
}
