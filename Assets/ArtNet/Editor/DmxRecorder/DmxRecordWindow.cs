using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtNet.Editor.DmxRecorder
{
    public class DmxRecordWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset visualTree;
        [SerializeField] private StyleSheet styleSheet;

        private Button _playButton, _pauseButton;
        private Label _timeCodeHourLabel, _timeCodeMinuteLabel, _timeCodeSecondLabel, _timeCodeMillisecondLabel;
        private VisualElement _timeCodeContainer;

        private static readonly Color RecordingColor = new(0.78f, 0f, 0f, 1f);
        private static readonly Color PausedColor = new(0.78f, 0.5f, 0f, 1f);

        private readonly DmxRecorder _recorder = new();

        [MenuItem("ArtNet/DmxRecorder")]
        public static void ShowDmxRecorder()
        {
            var window = GetWindow<DmxRecordWindow>();
            window.titleContent = new GUIContent("DmxRecorder");
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;

            VisualElement recorderVisualElement = visualTree.Instantiate();
            root.Add(recorderVisualElement);
            root.styleSheets.Add(styleSheet);

            _timeCodeContainer = root.Q<VisualElement>("time-code-container");
            _timeCodeHourLabel = root.Q<Label>("tc-hour");
            _timeCodeMinuteLabel = root.Q<Label>("tc-minute");
            _timeCodeSecondLabel = root.Q<Label>("tc-second");
            _timeCodeMillisecondLabel = root.Q<Label>("tc-millisecond");

            _playButton = root.Q<Button>("play-button");
            _pauseButton = root.Q<Button>("pause-button");
            var startButtonImage = root.Q<Image>("start-button-image");
            var stopButtonImage = root.Q<Image>("stop-button-image");

            _playButton.clicked += () =>
            {
                if (_recorder.Status == RecordingStatus.None)
                {
                    _recorder.StartRecording();

                    startButtonImage.style.display = DisplayStyle.None;
                    stopButtonImage.style.display = DisplayStyle.Flex;

                    _timeCodeContainer.style.backgroundColor = RecordingColor;
                    _pauseButton.SetEnabled(true);
                }
                else
                {
                    _recorder.StopRecording();

                    startButtonImage.style.display = DisplayStyle.Flex;
                    stopButtonImage.style.display = DisplayStyle.None;

                    _timeCodeContainer.style.backgroundColor = default;
                    _pauseButton.RemoveFromClassList("selected");
                    _pauseButton.SetEnabled(false);
                }
            };

            _pauseButton.SetEnabled(false);
            _pauseButton.clicked += () =>
            {
                switch (_recorder.Status)
                {
                    case RecordingStatus.Recording:
                        _recorder.PauseRecording();

                        _pauseButton.AddToClassList("selected");
                        _timeCodeContainer.style.backgroundColor = PausedColor;
                        break;
                    case RecordingStatus.Paused:
                        _recorder.ResumeRecording();

                        _pauseButton.RemoveFromClassList("selected");
                        _timeCodeContainer.style.backgroundColor = RecordingColor;
                        break;
                }
            };
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
    }
}
