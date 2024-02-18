using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtNet.Editor.DmxRecorder
{
    partial class DmxRecordWindow
    {
        private readonly Sender _sender = new();

        private bool _isForceSenderSlider;

        private Image _playButtonImage;

        private string _senderFilePath;
        private ProgressBar _senderProgressBar;

        private Label _senderTimeLabel;
        private Slider _senderTimeSlider;

        private void InitializeSender(VisualElement root)
        {
            _senderFilePath = EditorUserSettings.GetConfigValue(EditorSettingKey("SenderPath")) ??
                              "select dmx file";

            InitializeSenderPanel(root);
            InitializeSenderSettings(root);

            LoadDmxFile(_senderFilePath);
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
                    EditorUtility.OpenFilePanel("Select Play File", _recorder.Config.Directory, "dmx");
                if (string.IsNullOrEmpty(selectedFile)) return;

                senderFileNameField.value = selectedFile;
                _senderFilePath = selectedFile;
                EditorUserSettings.SetConfigValue(EditorSettingKey("SenderPath"), selectedFile);
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

            _senderTimeLabel = root.Q<Label>("playTimeLabel");
            _senderTimeSlider = root.Q<Slider>("playSlider");
            _senderTimeSlider.RegisterValueChangedCallback((evt) =>
            {
                _sender.ChangePlayTime(evt.newValue);
            });

            _senderProgressBar = root.Q<ProgressBar>("playProgressBar");
        }

        private void InitializeSenderSettings(VisualElement root)
        {
            var sendLoopToggle = root.Q<Toggle>("sendLoopToggle");
            sendLoopToggle.value =
                bool.Parse(EditorUserSettings.GetConfigValue(EditorSettingKey("SenderLoop")) ?? "false");
            sendLoopToggle.RegisterValueChangedCallback((evt) =>
            {
                _sender.Config.IsLoop = evt.newValue;
                EditorUserSettings.SetConfigValue(EditorSettingKey("SenderLoop"), evt.newValue.ToString());
            });

            var senderDistIpField = root.Q<TextField>("sendDistIpField");
            var distIp = EditorUserSettings.GetConfigValue(EditorSettingKey("SenderDistIp")) ?? _sender.Config.Ip;
            senderDistIpField.value = distIp;
            senderDistIpField.RegisterValueChangedCallback((evt) =>
            {
                _sender.Config.Ip = evt.newValue;
                EditorUserSettings.SetConfigValue(EditorSettingKey("SenderDistIp"), evt.newValue);
            });

            var sendRecordSequenceToggle = root.Q<Toggle>("sendRecordSequenceToggle");
            sendRecordSequenceToggle.RegisterValueChangedCallback((evt) =>
            {
                _sender.Config.IsRecordSequence = evt.newValue;
            });
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
            if (_sender.IsPlaying) _sender.Update(Time.deltaTime);

            var time = _sender.CurrentTime;
            var minutes = (int) time / 60000;
            var seconds = (int) time / 1000 % 60;
            var milliseconds = (int) time % 1000;

            _senderTimeLabel.text = $"{minutes}:{seconds:D2}.{milliseconds:D3}";
            _senderTimeSlider.value = time;
            _senderProgressBar.value = time;

            _playButtonImage.image = _sender.IsPlaying ? _preMatQuadTexture : _playButtonTexture;
        }
    }
}
