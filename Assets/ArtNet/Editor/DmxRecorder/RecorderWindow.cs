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

        private Image _startButtonImage, _stopButtonImage;
        private Button _pauseButton;

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
            _recorder.RecordControllerSettings = RecordControllerSettings.GetOrNewGlobalSettings();
            InitializeControlPanel(root);
            InitializeRecordSetting(root);
        }

        private void InitializeControlPanel(VisualElement root)
        {
            _timeCodeContainer = root.Q<VisualElement>("timeCodeContainer");
            _timeCodeHourLabel = root.Q<Label>("tcHour");
            _timeCodeMinuteLabel = root.Q<Label>("tcMinute");
            _timeCodeSecondLabel = root.Q<Label>("tcSecond");
            _timeCodeMillisecondLabel = root.Q<Label>("tcMillisecond");

            _startButtonImage = new Image { image = _playButtonTexture };
            _stopButtonImage = new Image
            {
                image = _preMatQuadTexture,
                style = { display = DisplayStyle.None }
            };
            var playButton = root.Q<Button>("playButton");
            _pauseButton = root.Q<Button>("pauseButton");

            playButton.Add(_startButtonImage);
            playButton.Add(_stopButtonImage);
            playButton.clicked += OnRecordStartButton;

            _pauseButton.SetEnabled(false);
            _pauseButton.Add(new Image
            {
                image = EditorGUIUtility.IconContent("PauseButton@2x").image
            });
            _pauseButton.clicked += OnRecordPauseButton;
        }

        private void InitializeRecordSetting(VisualElement root)
        {
            _outputBinaryConfig = root.Q<VisualElement>("binaryOutputConfig");
            _outputAnimationClipConfig = root.Q<VisualElement>("animationClipOutputConfig");

            // 出力ファイルのフォーマット選択
            var outputFormatGroup = root.Q<RadioButtonGroup>("outputFormatGroup");
            outputFormatGroup.choices = new[] { "Binary", "AnimationClip" };
            outputFormatGroup.value = (int) _recorder.RecordControllerSettings.RecordFormat;
            ChangeOutputFormat(_recorder.RecordControllerSettings.RecordFormat);

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
            _outputFileNameField.value = _recorder.RecordControllerSettings.BinarySetting.FileName;
            _outputFileNameField.RegisterValueChangedCallback(evt =>
            {
                var fileName = evt.newValue;
                _recorder.RecordControllerSettings.BinarySetting.FileName = fileName;
                UpdateOutputFilePath();
            });

            // 出力ディレクトリの設定
            _outputDirectoryField = root.Q<TextField>("outputDirectoryField");
            _outputDirectoryField.value = _recorder.RecordControllerSettings.BinarySetting.Directory;
            _outputDirectoryField.RegisterValueChangedCallback(evt =>
            {
                var directory = evt.newValue;
                _recorder.RecordControllerSettings.BinarySetting.Directory = directory;
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
                        folder: _recorder.RecordControllerSettings.BinarySetting.Directory,
                        defaultName: "");

                if (string.IsNullOrEmpty(selectedDirectory)) return;

                _recorder.RecordControllerSettings.BinarySetting.Directory = selectedDirectory;
                _outputDirectoryField.value = selectedDirectory;
                UpdateOutputFilePath();
            };

            // Animation Config
            var outputAssetDirectoryField = root.Q<TextField>("outputAssetDirectoryField");
            outputAssetDirectoryField.value = _recorder.RecordControllerSettings.AnimationClipSetting.OutputAnimationClipAssetPath;
            outputAssetDirectoryField.RegisterValueChangedCallback(evt =>
            {
                var directory = evt.newValue;
                _recorder.RecordControllerSettings.AnimationClipSetting.OutputAnimationClipAssetPath = directory;
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
                Process.Start(_recorder.RecordControllerSettings.BinarySetting.Directory);
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
            if (_recorder.RecordControllerSettings != null)
            {
                _recorder.RecordControllerSettings.Save();
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

            _recorder.RecordControllerSettings.ChangeRecordFormat(format);
        }

        private void UpdateOutputFilePath()
        {
            var path = _recorder.RecordControllerSettings.BinarySetting.OutputPath;
            _outputFilePathLabel.text = path;
            _outputWarningIcon.style.display = System.IO.File.Exists(path) ? DisplayStyle.Flex : DisplayStyle.None;
            UpdateErrorMessage();
        }

        private void UpdateErrorMessage()
        {
            var errors = _recorder.RecordControllerSettings.ValidateErrors();
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

        private void OnRecordStartButton()
        {
            if (!_recorder.RecordControllerSettings.Validate()) return;
            if (_recorder.Status == RecordingStatus.None)
            {
                SetEnabledTextField(false);
                _recorder.StartRecording();

                _startButtonImage.style.display = DisplayStyle.None;
                _stopButtonImage.style.display = DisplayStyle.Flex;

                _timeCodeContainer.style.backgroundColor = RecordingColor;
                _pauseButton.SetEnabled(true);
            }
            else
            {
                _recorder.StopRecording();

                _startButtonImage.style.display = DisplayStyle.Flex;
                _stopButtonImage.style.display = DisplayStyle.None;

                _timeCodeContainer.style.backgroundColor = default;
                _pauseButton.RemoveFromClassList("selected");
                _pauseButton.SetEnabled(false);
                SetEnabledTextField(true);
            }
        }

        private void OnRecordPauseButton()
        {
            switch (_recorder.Status)
            {
                case RecordingStatus.Recording:
                    _recorder.PauseRecording();
                    break;
                case RecordingStatus.Paused:
                    _recorder.ResumeRecording();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            UpdateRecordStatus();
        }

        private void UpdateRecordStatus()
        {
            switch (_recorder.Status)
            {
                case RecordingStatus.Recording:
                    _pauseButton.AddToClassList("selected");
                    _timeCodeContainer.style.backgroundColor = PausedColor;
                    break;
                case RecordingStatus.Paused:
                case RecordingStatus.None:
                    _pauseButton.RemoveFromClassList("selected");
                    _timeCodeContainer.style.backgroundColor = RecordingColor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
