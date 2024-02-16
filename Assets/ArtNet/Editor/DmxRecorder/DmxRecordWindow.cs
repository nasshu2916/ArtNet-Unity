using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtNet.Editor.DmxRecorder
{
    public class DmxRecordWindow : EditorWindow
    {
        private const string EditorSettingPrefix = "ArtNet.DmxRecorder.";

        private static readonly Color RecordingColor = new(0.78f, 0f, 0f, 1f);
        private static readonly Color PausedColor = new(0.78f, 0.5f, 0f, 1f);
        [SerializeField] private VisualTreeAsset visualTree;
        [SerializeField] private StyleSheet styleSheet;

        private readonly DmxRecorder _recorder = new();
        private Label _errorMessageLabel;
        private TextField _outputFileNameField, _outputDirectoryField;

        private Label _outputFilePathLabel, _footerStatusLabel;
        private Image _outputWarningIcon;
        private Button _selectDirectoryButton;
        private VisualElement _timeCodeContainer, _errorMessageArea;

        private Label _timeCodeHourLabel, _timeCodeMinuteLabel, _timeCodeSecondLabel, _timeCodeMillisecondLabel;

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

            InitializeControlPanel(root);
            InitializeRecordingConfig(root);

            _footerStatusLabel = root.Q<Label>("footerStatusLabel");
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
            var playButton = root.Q<Button>("playButton");
            var pauseButton = root.Q<Button>("pauseButton");

            playButton.Add(startButtonImage);
            playButton.Add(stopButtonImage);
            playButton.clicked += () =>
            {
                if (!_recorder.Config.Validate()) return;
                if (_recorder.Status == RecordingStatus.None)
                {
                    SetEnabledTextField(false);
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
                    SetEnabledTextField(true);
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
            _outputFilePathLabel = root.Q<Label>("outputFileName");
            _outputWarningIcon = root.Q<Image>("outputWarningIcon");

            // 出力ファイル名の設定
            _outputFileNameField = root.Q<TextField>("outputFileNameField");
            _outputFileNameField.value = _recorder.Config.FileName;
            _outputFileNameField.RegisterValueChangedCallback(evt =>
            {
                var fileName = evt.newValue;
                _recorder.Config.FileName = fileName;
                UpdateOutputFilePath();
                EditorUserSettings.SetConfigValue(EditorSettingKey("OutputFileName"), fileName);
            });

            // 出力ディレクトリの設定
            _outputDirectoryField = root.Q<TextField>("outputDirectoryField");
            _outputDirectoryField.value = _recorder.Config.Directory;
            _outputDirectoryField.RegisterValueChangedCallback(evt =>
            {
                var directory = evt.newValue;
                _recorder.Config.Directory = directory;
                UpdateOutputFilePath();
                EditorUserSettings.SetConfigValue(EditorSettingKey("OutputDirectory"), directory);
            });
            _selectDirectoryButton = root.Q<Button>("selectFolderButton");
            _selectDirectoryButton.Add(new Image()
                {
                    image = EditorGUIUtility.IconContent("Folder Icon").image
                }
            );
            _selectDirectoryButton.clicked += () =>
            {
                var selectedDirectory =
                    EditorUtility.OpenFolderPanel(title: "Output Folder",
                        folder: _recorder.Config.Directory,
                        defaultName: "");

                if (string.IsNullOrEmpty(selectedDirectory)) return;

                _recorder.Config.Directory = selectedDirectory;
                _outputDirectoryField.value = selectedDirectory;
                UpdateOutputFilePath();
                EditorUserSettings.SetConfigValue(EditorSettingKey("OutputDirectory"), selectedDirectory);
            };


            var outputWarningIcon = root.Q<Image>("outputWarningIcon");
            outputWarningIcon.image = EditorGUIUtility.IconContent("Warning@2x").image;
            var openOutputFolderButton = root.Q<Button>("openOutputFolderButton");
            openOutputFolderButton.Add(new Image()
                {
                    image = EditorGUIUtility.IconContent("FolderOpened Icon").image
                }
            );
            openOutputFolderButton.clicked += () =>
            {
                Process.Start(_recorder.Config.Directory);
            };

            _errorMessageArea = root.Q<VisualElement>("errorMessageArea");
            _errorMessageArea.Add(new Image()
                {
                    image = EditorGUIUtility.IconContent("console.erroricon@2x").image
                }
            );
            _errorMessageLabel = new Label();
            _errorMessageArea.Add(_errorMessageLabel);

            UpdateOutputFilePath();
        }

        private void UpdateOutputFilePath()
        {
            var path = _recorder.Config.OutputPath;
            _outputFilePathLabel.text = path;
            _outputWarningIcon.style.display = System.IO.File.Exists(path) ? DisplayStyle.Flex : DisplayStyle.None;
            UpdateErrorMessage();
        }

        private void UpdateErrorMessage()
        {
            var errors = _recorder.Config.ValidateErrors();
            if (errors.Count > 0)
            {
                _errorMessageLabel.text = string.Join("\n", errors);
                _errorMessageArea.style.visibility = Visibility.Visible;
            }
            else
            {
                _errorMessageArea.style.visibility = Visibility.Hidden;
            }
        }

        private void SetEnabledTextField(bool enabled)
        {
            _outputFileNameField.SetEnabled(enabled);
            _outputDirectoryField.SetEnabled(enabled);
            _selectDirectoryButton.SetEnabled(enabled);
        }

        private static string EditorSettingKey(string key) => $"{EditorSettingPrefix}{key}";
    }
}
