using System.Linq;
using ArtNet.Editor.DmxRecorder.Util;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtNet.Editor.DmxRecorder
{
    public class PlayerWindow : EditorWindow
    {
        private class DestinationList : ElementItemList<SendDestinationItem>
        {
        }

        [SerializeField] private VisualTreeAsset _visualTree;
        [SerializeField] private StyleSheet _styleSheet;
        [SerializeField] private StyleSheet _darkStyleSheet, _lightStyleSheet;

        private PlayController _controller;

        private string _senderFilePath;
        private ProgressBar _senderProgressBar;

        private Label _senderTimeLabel;
        private Slider _senderTimeSlider;

        private DestinationList _destinationList;

        [MenuItem("ArtNet/DMX Player")]
        public static void ShowWindow()
        {
            var window = GetWindow<PlayerWindow>()!;
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
            selectPlayFileButton.Add(new Image { image = EditorGUIUtility.IconContent("Folder Icon").image });
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
            playButton.clicked += () => { _controller?.Play(); };

            _senderTimeLabel = root.Q<Label>("playTimeLabel");
            _senderTimeSlider = root.Q<Slider>("playSlider");
            _senderTimeSlider.RegisterValueChangedCallback((evt) =>
            {
                var time = (int) evt.newValue;
                // _controller.ChangePlayTime(time);
                _senderTimeLabel.text = TimeText(time);
            });

            _senderProgressBar = root.Q<ProgressBar>("playProgressBar");

            // ===== Send Destination =====

            var sendDestinationsPanel = visualElement.Q<VisualElement>("sendDestinationsPanel")!;
            var addDestinationLabel = root.Q<Label>("addDestinationLabel")!;
            addDestinationLabel.RegisterCallback<ClickEvent>(_ =>
            {
                var menu = new GenericMenu();
                var context = new GUIContent("Add New Send Destination");
                menu.AddItem(context, false, () => AddNewSendDestination());

                menu.ShowAsContext();
            });
            _destinationList = new DestinationList
            {
                name = "destinationList",
                focusable = true
            };
            sendDestinationsPanel.Add(_destinationList);

            SetPlayControllerSettings(PlayControllerSetting.GetOrNewGlobalSetting()!);
        }

        private void SetPlayControllerSettings([NotNull] PlayControllerSetting setting)
        {
            _controller = new PlayController(setting);
            // _controller.OnStartRecording += OnStartRecording;
            // _controller.OnPauseRecording += OnPauseRecording;
            // _controller.OnStopRecording += OnFinishRecording;
            // _controller.OnResumeRecording += OnStartRecording;

            ReloadSendDestinations();
        }

        private void AddNewSendDestination(bool isSend = false)
        {
            if (_controller == null || _destinationList == null) return;

            var sendDestination = (SendDestination) CreateInstance(typeof(SendDestination))!;
            sendDestination.IsSend = isSend;
            var item = new SendDestinationItem(_controller.ControllerSetting, sendDestination);
            _destinationList.Add(item);
            _controller.ControllerSetting.AddSendDestination(sendDestination);
        }

        private void ReloadSendDestinations()
        {
            if (_controller?.ControllerSetting == null)
                return;

            var sendDirectionItem = _controller.ControllerSetting.SendDestinations.Select(CreateSendDestinationsItem)
                .ToArray();

            _destinationList?.Reload(sendDirectionItem);
        }

        private SendDestinationItem CreateSendDestinationsItem(SendDestination sendDestination)
        {
            var sendDestinationItem = new SendDestinationItem(_controller?.ControllerSetting, sendDestination);

            return sendDestinationItem;
        }

        private void LoadDmxFile([NotNull] string path)
        {
            _controller?.LoadFile(path);
            //
            // var maxTimeLabel = rootVisualElement.Q<Label>("playbackMaxTimeLabel");
            //
            // var maxSeconds = _sender.MaxTime / 1000;
            // var minutes = maxSeconds / 60;
            // var seconds = maxSeconds % 60;
            // maxTimeLabel.text = $"{minutes}:{seconds:D2}";
            // var maxValue = _sender.MaxTime;
            //
            // _senderTimeSlider.highValue = maxValue;
            // _senderProgressBar.highValue = maxValue;
        }

        private static string TimeText(int time)
        {
            var minutes = time / 60000;
            var seconds = time / 1000 % 60;
            var milliseconds = time % 1000;
            return $"{minutes}:{seconds:D2}.{milliseconds:D3}";
        }
    }
}
