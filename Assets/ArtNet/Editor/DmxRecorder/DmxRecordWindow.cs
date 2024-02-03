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
        private bool _isRecording, _isPaused;
        private float _recordStartTime;
        private float _alreadyRecordedTime;

        private static readonly Color RecordingColor = new Color(0.78f, 0f, 0f, 1f);
        private static readonly Color PausedColor = new Color(0.78f, 0.5f, 0f, 1f);

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
                _isRecording = !_isRecording;
                if (_isRecording)
                {
                    Debug.Log("DMX Recording started");
                    startButtonImage.style.display = DisplayStyle.None;
                    stopButtonImage.style.display = DisplayStyle.Flex;

                    OnRecordStart();
                }
                else
                {
                    Debug.Log("DMX Recording stopped");
                    startButtonImage.style.display = DisplayStyle.Flex;
                    stopButtonImage.style.display = DisplayStyle.None;

                    OnRecordStop();
                }
            };

            _pauseButton.SetEnabled(false);
            _pauseButton.clicked += () =>
            {
                if (!_isRecording) return;

                _isPaused = !_isPaused;
                if (_isPaused)
                {
                    Debug.Log("DMX Recording paused");
                    _pauseButton.AddToClassList("selected");
                    OnRecordPause();
                }
                else
                {
                    Debug.Log("DMX Recording resumed");
                    _pauseButton.RemoveFromClassList("selected");
                    OnRecordResume();
                }
            };
        }

        private void OnRecordStart()
        {
            _recordStartTime = Time.realtimeSinceStartup;
            _timeCodeContainer.style.backgroundColor = RecordingColor;
            _pauseButton.SetEnabled(true);
        }

        private void OnRecordStop()
        {
            _recordStartTime = 0;
            _alreadyRecordedTime = 0;
            _isPaused = false;

            _timeCodeContainer.style.backgroundColor = default;
            _pauseButton.RemoveFromClassList("selected");
            _pauseButton.SetEnabled(false);
        }

        private void OnRecordPause()
        {
            var recordedTime = Time.realtimeSinceStartup - _recordStartTime;
            _alreadyRecordedTime += recordedTime;
            _recordStartTime = 0;
            _timeCodeContainer.style.backgroundColor = PausedColor;
        }

        private void OnRecordResume()
        {
            _recordStartTime = Time.realtimeSinceStartup;
            _timeCodeContainer.style.backgroundColor = RecordingColor;
        }

        private void Update()
        {
            float timeCode;
            if (_isRecording && !_isPaused)
            {
                timeCode = Time.realtimeSinceStartup - _recordStartTime + _alreadyRecordedTime;
            }
            else
            {
                timeCode = _alreadyRecordedTime;
            }
            var timeCodeSpan = TimeSpan.FromSeconds(timeCode);
            _timeCodeHourLabel.text = timeCodeSpan.Hours.ToString("00");
            _timeCodeMinuteLabel.text = timeCodeSpan.Minutes.ToString("00");
            _timeCodeSecondLabel.text = timeCodeSpan.Seconds.ToString("00");
            _timeCodeMillisecondLabel.text = Math.Floor(timeCodeSpan.Milliseconds / 10.0f).ToString("00");
        }
    }
}
