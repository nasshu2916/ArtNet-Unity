using ArtNet.Editor.DmxRecorder.Util;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtNet.Editor.DmxRecorder
{
    public class PlayerWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset _visualTree;
        [SerializeField] private StyleSheet _styleSheet;
        [SerializeField] private StyleSheet _darkStyleSheet, _lightStyleSheet;

        private readonly Sender _sender = new();

        private bool _isAnyChange;
        private bool _isPlaying;

        private int _lastTime;

        private string _senderFilePath;
        private ProgressBar _senderProgressBar;

        private Label _senderTimeLabel;
        private Slider _senderTimeSlider;

        [MenuItem("ArtNet/DMX Player")]
        public static void ShowWindow()
        {
            var window = GetWindow<PlayerWindow>();
            window.titleContent = new GUIContent("DMX Player");
        }

        private void OnEnable()
        {
            CreateView();
            RegisterCallbacks();
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
        }

        private void RegisterCallbacks()
        {
        }

        private void UnregisterCallbacks()
        {
        }

        private void CreateView()
        {
            minSize = new Vector2(400, 200);
            var root = rootVisualElement;

            if (_visualTree == null)
            {
                Debug.LogError("VisualTree is null");
                return;
            }

            if (_styleSheet == null)
            {
                Debug.LogError("StyleSheet is null");
                return;
            }

            VisualElement visualElement = _visualTree.Instantiate();
            visualElement.AddToClassList("root");
            root.Add(visualElement);

            root.styleSheets.Add(_styleSheet);

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
                    EditorUtility.OpenFilePanel("Select Play File", "Assets", "dmx");
                if (string.IsNullOrEmpty(selectedFile)) return;

                senderFileNameField.value = selectedFile;
                _senderFilePath = selectedFile;
                LoadDmxFile(_senderFilePath);
            };

            var playButton = root.Q<Button>("PlayButton");
            var playButtonImage = new Image { image = IconHelper.PlayButton };
            playButton.Add(playButtonImage);
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
