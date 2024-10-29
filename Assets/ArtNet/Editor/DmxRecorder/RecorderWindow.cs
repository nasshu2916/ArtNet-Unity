using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtNet.Editor.DmxRecorder
{
    partial class DmxRecordWindow
    {
        private static readonly Color RecordingColor = new(0.78f, 0f, 0f, 1f);
        private static readonly Color PausedColor = new(0.78f, 0.5f, 0f, 1f);

        private readonly Recorder _recorder = new();

        private Label _errorMessageLabel;

        private VisualElement _outputBinaryConfig, _outputAnimationClipConfig;
        private TextField _outputAssetDirectoryField;
        private TextField _outputFileNameField, _outputDirectoryField;

        private Label _outputFilePathLabel, _footerStatusLabel;
        private Image _outputWarningIcon;

        private Button _selectDirectoryButton;
        private VisualElement _timeCodeContainer, _errorMessageArea;

        private Label _timeCodeHourLabel, _timeCodeMinuteLabel, _timeCodeSecondLabel, _timeCodeMillisecondLabel;

        private void UpdateRecorder()
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
        private void InitializeRecorder(VisualElement root)
        {
            InitializeControlPanel(root);
            InitializeRecordingConfig(root);
        }

        private void InitializeControlPanel(VisualElement root)
        {
            _timeCodeContainer = root.Q<VisualElement>("timeCodeContainer");
            _timeCodeHourLabel = root.Q<Label>("tcHour");
            _timeCodeMinuteLabel = root.Q<Label>("tcMinute");
            _timeCodeSecondLabel = root.Q<Label>("tcSecond");
            _timeCodeMillisecondLabel = root.Q<Label>("tcMillisecond");

            var startButtonImage = new Image { image = _playButtonTexture };
            var stopButtonImage = new Image
            {
                image = _preMatQuadTexture,
                style = { display = DisplayStyle.None }
            };
            var playButton = root.Q<Button>("playButton");
            var pauseButton = root.Q<Button>("pauseButton");

            playButton.Add(startButtonImage);
            playButton.Add(stopButtonImage);
            playButton.clicked += () =>
            {
                if (!_recorder.RecorderSettings.Validate()) return;
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
            _outputBinaryConfig = root.Q<VisualElement>("binaryOutputConfig");
            _outputAnimationClipConfig = root.Q<VisualElement>("animationClipOutputConfig");

            // 出力ファイルのフォーマット選択
            var outputFormatGroup = root.Q<RadioButtonGroup>("outputFormatGroup");
            outputFormatGroup.choices = new[] { "Binary", "AnimationClip" };
            outputFormatGroup.value = (int) _recorder.RecorderSettings.RecordFormat;
            ChangeOutputFormat(_recorder.RecorderSettings.RecordFormat);

            outputFormatGroup.RegisterValueChangedCallback(evt =>
            {
                var format = (RecodeFormat) evt.newValue;
                ChangeOutputFormat(format);
                UpdateErrorMessage();
            });

            _outputFilePathLabel = root.Q<Label>("outputFileName");
            _outputWarningIcon = root.Q<Image>("outputWarningIcon");

            // 出力ファイル名の設定
            _outputFileNameField = root.Q<TextField>("outputFileNameField");
            _outputFileNameField.value = _recorder.RecorderSettings.BinarySetting.FileName;
            _outputFileNameField.RegisterValueChangedCallback(evt =>
            {
                var fileName = evt.newValue;
                _recorder.RecorderSettings.BinarySetting.FileName = fileName;
                UpdateOutputFilePath();
            });

            // 出力ディレクトリの設定
            _outputDirectoryField = root.Q<TextField>("outputDirectoryField");
            _outputDirectoryField.value = _recorder.RecorderSettings.BinarySetting.Directory;
            _outputDirectoryField.RegisterValueChangedCallback(evt =>
            {
                var directory = evt.newValue;
                _recorder.RecorderSettings.BinarySetting.Directory = directory;
                UpdateOutputFilePath();
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
                        folder: _recorder.RecorderSettings.BinarySetting.Directory,
                        defaultName: "");

                if (string.IsNullOrEmpty(selectedDirectory)) return;

                _recorder.RecorderSettings.BinarySetting.Directory = selectedDirectory;
                _outputDirectoryField.value = selectedDirectory;
                UpdateOutputFilePath();
            };

            // Animation Config
            var outputAssetDirectoryField = root.Q<TextField>("outputAssetDirectoryField");
            outputAssetDirectoryField.value = _recorder.RecorderSettings.AnimationClipSetting.OutputAnimationClipAssetPath;
            outputAssetDirectoryField.RegisterValueChangedCallback(evt =>
            {
                var directory = evt.newValue;
                _recorder.RecorderSettings.AnimationClipSetting.OutputAnimationClipAssetPath = directory;
            });


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
                Process.Start(_recorder.RecorderSettings.BinarySetting.Directory);
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

        private void SaveConfig()
        {
            if (_recorder.RecorderSettings != null)
            {
                _recorder.RecorderSettings.Save();
            }

            if (_sender.SenderSettings != null)
            {
                _sender.SenderSettings.Save();
            }
        }

        private void ChangeOutputFormat(RecodeFormat format)
        {
            _outputBinaryConfig.style.display = DisplayStyle.None;
            _outputAnimationClipConfig.style.display = DisplayStyle.None;
            switch (format)
            {
                case RecodeFormat.Binary:
                    _outputBinaryConfig.style.display = DisplayStyle.Flex;
                    break;
                case RecodeFormat.AnimationClip:
                    _outputAnimationClipConfig.style.display = DisplayStyle.Flex;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }

            _recorder.RecorderSettings.ChangeRecordFormat(format);
        }

        private void UpdateOutputFilePath()
        {
            var path = _recorder.RecorderSettings.BinarySetting.OutputPath;
            _outputFilePathLabel.text = path;
            _outputWarningIcon.style.display = System.IO.File.Exists(path) ? DisplayStyle.Flex : DisplayStyle.None;
            UpdateErrorMessage();
        }

        private void UpdateErrorMessage()
        {
            var errors = _recorder.RecorderSettings.ValidateErrors();
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
    }
}
