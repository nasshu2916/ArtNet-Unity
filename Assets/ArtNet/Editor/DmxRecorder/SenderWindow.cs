using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using UnityEditor;
using UnityEngine.UIElements;

namespace ArtNet.Editor.DmxRecorder
{
    partial class DmxRecordWindow
    {
        private readonly Sender _sender = new();
        private readonly List<string> _senderErrorMessages = new();

        private readonly List<(float, string)> _senderSpeedDropdown = new()
        {
            (0.1f, "0.1x"),
            (0.25f, "0.25x"),
            (0.5f, "0.5x"),
            (1f, "1x"),
            (2f, "2x"),
            (5f, "5x"),
            (10f, "10x"),
        };

        private bool _isAnyChange;
        private bool _isPlaying;

        private int _lastTime;

        private Image _playButtonImage;
        private VisualElement _senderErrorMessageArea;
        private Label _senderErrorMessageLabel;

        private string _senderFilePath;
        private ProgressBar _senderProgressBar;

        private Label _senderTimeLabel;
        private Slider _senderTimeSlider;

        private void InitializeSender(VisualElement root)
        {
            InitializeSenderPanel(root);
            InitializeSenderSettings(root);

            if (!string.IsNullOrEmpty(_senderFilePath)) LoadDmxFile(_senderFilePath);
        }

        private void InitializeSenderPanel(VisualElement root)
        {
            var senderFileNameField = root.Q<TextField>("senderFileNameField");
            senderFileNameField.value = _senderFilePath;

            var selectPlayFileButton = root.Q<Button>("selectPlayFileButton");
            selectPlayFileButton.Add(new Image()
            {
                image = EditorGUIUtility.IconContent("Folder Icon").image
            }
            );
            selectPlayFileButton.clicked += () =>
            {
                var selectedFile =
                    EditorUtility.OpenFilePanel("Select Play File", _recorder.RecorderConfigs.BinaryConfig.Directory, "dmx");
                if (string.IsNullOrEmpty(selectedFile)) return;

                senderFileNameField.value = selectedFile;
                _senderFilePath = selectedFile;
                LoadDmxFile(_senderFilePath);
            };

            var playButton = root.Q<Button>("PlayButton");
            _playButtonImage = new Image { image = _playButtonTexture };
            playButton.Add(_playButtonImage);
            playButton.clicked += () =>
            {
                if (_sender.IsPlaying)
                {
                    _sender.Stop();
                }
                else
                {
                    _sender.Play();
                }
            };
            _sender.ChangedPlaying += isPlaying =>
            {
                _isPlaying = isPlaying;
                _isAnyChange = true;
            };

            _senderTimeLabel = root.Q<Label>("playTimeLabel");
            _senderTimeSlider = root.Q<Slider>("playSlider");
            _senderTimeSlider.RegisterValueChangedCallback((evt) =>
            {
                var time = (int) evt.newValue;
                _sender.ChangePlayTime(time);
                _senderTimeLabel.text = TimeText(time);
            });

            _senderProgressBar = root.Q<ProgressBar>("playProgressBar");

            _sender.TimeChanged += OnTimeChanged;
        }

        private void InitializeSenderSettings(VisualElement root)
        {
            _sender.SenderConfigs = SenderConfigs.GetOrNewGlobalConfigs();

            var sendLoopToggle = root.Q<Toggle>("sendLoopToggle");
            sendLoopToggle.value = _sender.SenderConfigs.IsLoop;
            sendLoopToggle.RegisterValueChangedCallback((evt) =>
            {
                _sender.SenderConfigs.IsLoop = evt.newValue;
            });

            var senderDistIpField = root.Q<TextField>("sendDistIpField");
            senderDistIpField.value = _sender.SenderConfigs.Ip.ToString(); ;
            senderDistIpField.RegisterValueChangedCallback((evt) =>
            {
                var ipText = evt.newValue;
                if (!IPAddress.TryParse(ipText, out var ip))
                {
                    if (_senderErrorMessages.Contains("Invalid IP address")) return;
                    _senderErrorMessages.Add("Invalid IP address");
                    UpdateSenderErrorMessage();
                }
                else
                {
                    _sender.SenderConfigs.Ip = ip;
                    if (!_senderErrorMessages.Contains("Invalid IP address")) return;
                    _senderErrorMessages.Remove("Invalid IP address");
                    UpdateSenderErrorMessage();
                }
            });

            var sendRecordSequenceToggle = root.Q<Toggle>("sendRecordSequenceToggle");
            sendRecordSequenceToggle.RegisterValueChangedCallback(evt =>
            {
                _sender.SenderConfigs.IsRecordSequence = evt.newValue;
            });

            var sendSpeedSlider = root.Q<Slider>("sendSpeed");
            var sendSpeedDropdown = root.Q<DropdownField>("sendSpeedDropdown");
            sendSpeedDropdown.choices.Clear();
            sendSpeedDropdown.choices.AddRange(_senderSpeedDropdown.Select(x => x.Item2));
            sendSpeedDropdown.index = -1;

            sendSpeedSlider.value = _sender.SenderConfigs.Speed;
            sendSpeedSlider.label = $"Speed (x{_sender.SenderConfigs.Speed})";
            sendSpeedSlider.RegisterValueChangedCallback(evt =>
            {
                var speedValue = evt.newValue;
                _sender.SenderConfigs.Speed = speedValue;
                sendSpeedSlider.label = $"Speed (x{speedValue})";
                sendSpeedDropdown.index = -1;
            });

            sendSpeedDropdown.RegisterValueChangedCallback(evt =>
            {
                var speedValue = _senderSpeedDropdown.Find(x => x.Item2 == evt.newValue).Item1;
                if (speedValue == 0) return;
                sendSpeedSlider.value = speedValue;
            });

            _senderErrorMessageArea = root.Q<VisualElement>("senderErrorMessageArea");
            _senderErrorMessageArea.Add(new Image()
            {
                image = EditorGUIUtility.IconContent("console.erroricon@2x").image
            }
            );
            _senderErrorMessageLabel = new Label();
            _senderErrorMessageArea.Add(_senderErrorMessageLabel);
            UpdateSenderErrorMessage();
        }

        private void LoadDmxFile(string path)
        {
            _sender.Load(path);

            var maxTimeLabel = rootVisualElement.Q<Label>("playbackMaxTimeLabel");

            var maxSeconds = _sender.MaxTime / 1000;
            var minutes = maxSeconds / 60;
            var seconds = maxSeconds % 60;
            maxTimeLabel.text = $"{minutes}:{seconds:D2}";
            var maxValue = _sender.MaxTime;

            _senderTimeSlider.highValue = maxValue;
            _senderProgressBar.highValue = maxValue;
        }

        private void UpdateSender()
        {
            if (!_isAnyChange) return;

            _senderTimeLabel.text = TimeText(_lastTime);
            _senderTimeSlider.value = _lastTime;
            _senderProgressBar.value = _lastTime;

            _playButtonImage.image = _isPlaying ? _preMatQuadTexture : _playButtonTexture;

            _isAnyChange = false;
        }

        private static string TimeText(int time)
        {
            var minutes = time / 60000;
            var seconds = time / 1000 % 60;
            var milliseconds = time % 1000;
            return $"{minutes}:{seconds:D2}.{milliseconds:D3}";
        }

        private void OnTimeChanged(int time)
        {
            _lastTime = time;
            _isAnyChange = true;
        }

        private void UpdateSenderErrorMessage()
        {
            if (_senderErrorMessages.Count > 0)
            {
                _senderErrorMessageLabel.text = string.Join("\n", _senderErrorMessages);
                _senderErrorMessageArea.style.visibility = Visibility.Visible;
            }
            else
            {
                _senderErrorMessageArea.style.visibility = Visibility.Hidden;
            }
        }
    }
}
