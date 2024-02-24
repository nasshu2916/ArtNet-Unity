using UnityEditor;
using UnityEngine.UIElements;

namespace ArtNet.Editor.DmxRecorder
{
    partial class DmxRecordWindow
    {
        private readonly Sender _sender = new();
        private bool _isAnyChange;
        private bool _isPlaying;

        private int _lastTime;

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
            var sendLoopToggle = root.Q<Toggle>("sendLoopToggle");
            var isLoop = bool.Parse(EditorUserSettings.GetConfigValue(EditorSettingKey("SenderLoop")) ?? "false");
            sendLoopToggle.value = isLoop;
            _sender.Config.IsLoop = isLoop;
            sendLoopToggle.RegisterValueChangedCallback((evt) =>
            {
                _sender.Config.IsLoop = evt.newValue;
                EditorUserSettings.SetConfigValue(EditorSettingKey("SenderLoop"), evt.newValue.ToString());
            });

            var senderDistIpField = root.Q<TextField>("sendDistIpField");
            var distIp = EditorUserSettings.GetConfigValue(EditorSettingKey("SenderDistIp")) ?? _sender.Config.Ip;
            senderDistIpField.value = distIp;
            _sender.Config.Ip = distIp;
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
    }
}
