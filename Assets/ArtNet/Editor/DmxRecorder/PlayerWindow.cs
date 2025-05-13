using System.IO;
using System.Linq;
using ArtNet.Common;
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
        private Image _playButtonImage;

        private Toggle _loopToggle;
        private SnapSlider _speedSlider;

        private DestinationList _destinationList;

        private PlaybackState _sliderDragBeforeState = PlaybackState.Invalid;

        [MenuItem(Const.Editor.MenuItemNamePrefix + "DMX Player", false, Const.Editor.Priority)]
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
            EditorApplication.update += ProcessMainThreadUpdates;
        }

        private void UnregisterCallbacks()
        {
            EditorApplication.update -= ProcessMainThreadUpdates;
        }

        private void ProcessMainThreadUpdates()
        {
            _controller?.ProcessMainThreadUpdates();
        }

        private void CreateView()
        {
            minSize = new Vector2(350, 400);
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

            var skinStyleSheet = EditorGUIUtility.isProSkin ? _darkStyleSheet : _lightStyleSheet;
            if (skinStyleSheet == null)
            {
                Debug.LogError("SkinStyleSheet is null");
                return;
            }

            VisualElement visualElement = _visualTree.Instantiate();
            visualElement.AddToClassList("root");
            root.Add(visualElement);

            root.styleSheets.Add(skinStyleSheet);
            root.styleSheets.Add(_styleSheet);

            var senderFileNameField = root.Q<TextField>("senderFileNameField");
            senderFileNameField.value = "";

            var selectPlayFileButton = root.Q<Button>("selectPlayFileButton");
            selectPlayFileButton.Add(new Image { image = EditorGUIUtility.IconContent("Folder Icon").image });
            selectPlayFileButton.clicked += () =>
            {
                string openDirectory;
                if (string.IsNullOrEmpty(_senderFilePath) == false)
                {
                    openDirectory = Path.GetDirectoryName(_senderFilePath);
                }
                else
                {
                    var lastLoadedFilePath = _controller?.LastLoadedFilePath();
                    openDirectory = string.IsNullOrEmpty(lastLoadedFilePath)
                        ? "Assets"
                        : Path.GetDirectoryName(lastLoadedFilePath);
                }

                var selectedFile = EditorUtility.OpenFilePanel("Select Play File", openDirectory, "dmx");
                if (string.IsNullOrEmpty(selectedFile)) return;

                var result = LoadDmxFile(selectedFile);
                if (result == false) return;

                senderFileNameField.value = selectedFile;
                _senderFilePath = selectedFile;
            };

            var playButton = root.Q<Button>("PlayButton")!;
            _playButtonImage = new Image { image = IconHelper.PlayButton };
            playButton.Add(_playButtonImage);
            playButton.clicked += () =>
            {
                if (_controller == null || _controller.IsLoaded == false) return;
                if (_controller!.State == PlaybackState.Play)
                {
                    _controller.Pause();
                }
                else
                {
                    _controller.Play();
                }
            };

            _senderTimeLabel = root.Q<Label>("playTimeLabel")!;
            _senderTimeSlider = root.Q<Slider>("playSlider")!;
            _senderTimeSlider.RegisterValueChangedCallback(evt =>
            {
                if (_controller == null || _controller.IsLoaded == false) return;
                if (_sliderDragBeforeState == PlaybackState.Invalid)
                {
                    _sliderDragBeforeState = _controller.State;
                }

                _controller.Pause();
                var time = (long) evt!.newValue;
                _controller.ChangePlayTime(time);
                _senderTimeLabel.text = TimeText(time);
            });
            _senderTimeSlider.RegisterCallback<PointerCaptureOutEvent>(_ =>
                {
                    if (_controller == null || _controller.IsLoaded == false) return;
                    var time = (long) _senderTimeSlider.value;
                    _controller.ChangePlayTime(time);

                    if (_sliderDragBeforeState == PlaybackState.Play)
                        _controller!.Play();
                    _sliderDragBeforeState = PlaybackState.Invalid;
                }
            );

            _senderProgressBar = root.Q<ProgressBar>("playProgressBar");

            // ===== Send Settings =====
            _loopToggle = root.Q<Toggle>("sendLoopToggle");
            _loopToggle.RegisterValueChangedCallback(evt =>
            {
                if (_controller?.ControllerSetting == null) return;

                _controller.ControllerSetting.IsLoop = evt!.newValue;
                _controller.ControllerSetting.Save();
            });

            _speedSlider = root.Q<SnapSlider>("sendSpeedSlider");
            _speedSlider!.RegisterValueChangedCallback(evt =>
            {
                if (_controller?.ControllerSetting == null) return;

                _controller.ControllerSetting.Speed = evt!.newValue;
                _controller.ControllerSetting.Save();
            });

            // ===== Send Destination =====

            var sendDestinationsPanel = visualElement.Q<VisualElement>("sendDestinationsPanel")!;
            var addDestinationLabel = root.Q<Label>("addDestinationLabel")!;
            addDestinationLabel.RegisterCallback<ClickEvent>(_ => ShowDestinationContextMenu());
            _destinationList = new DestinationList
            {
                name = "destinationList",
                focusable = true,
                style =
                {
                    flexGrow = 1
                }
            };
            _destinationList.OnItemContextMenu += OnDestinationContextMenu;
            _destinationList.OnSelectionChanged += OnDestinationSelectionChanged;
            _destinationList.OnContextMenu += ShowDestinationContextMenu;
            sendDestinationsPanel.Add(_destinationList);

            SetPlayControllerSettings(PlayControllerSetting.GetOrNewGlobalSetting()!);
        }

        private void SetPlayControllerSettings([NotNull] PlayControllerSetting setting)
        {
            _controller = new PlayController(setting);

            _controller.TimeChanged += OnPlayTimeChanged;
            _controller.StateChanged += OnPlaybackStateChanged;

            ReloadSendSettings();
            ReloadSendDestinations();
        }

        private void AddNewSendDestination(bool isSend = false, string recorderName = null)
        {
            if (_controller == null || _destinationList == null) return;

            var sendDestination = (SendDestination) CreateInstance(typeof(SendDestination))!;
            sendDestination.IsSend = isSend;
            sendDestination.name = UniqueDestinationName(recorderName ?? SendDestination.DefaultName);
            var item = new SendDestinationItem(_controller.ControllerSetting, sendDestination);
            _destinationList.Add(item);
            _controller.ControllerSetting.AddSendDestination(sendDestination);
        }

        private void ReloadSendSettings()
        {
            if (_controller?.ControllerSetting == null)
                return;

            _loopToggle!.value = _controller.ControllerSetting.IsLoop;
            _speedSlider!.Value = _controller.ControllerSetting.Speed;
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

        private void ShowDestinationContextMenu()
        {
            var menu = new GenericMenu();
            var context = new GUIContent("Add New Send Destination");
            menu.AddItem(context, false, () => AddNewSendDestination());

            menu.ShowAsContext();
        }

        private void OnDestinationContextMenu(SendDestinationItem item)
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Delete"),
                false,
                _ =>
                {
                    var settings = item.SendDestination;
                    _controller.ControllerSetting.RemoveSendDestination(settings);
                    _destinationList.Remove(item);
                },
                item);

            menu.ShowAsContext();
        }

        private void OnDestinationSelectionChanged()
        {
            var selectedIndex = _destinationList!.SelectedIndex;
            var items = _destinationList!.Items;
            for (var i = 0; i < items.Count; i++)
            {
                items[i].SetItemSelected(i == selectedIndex);
            }

            Repaint();
        }

        [NotNull]
        private string UniqueDestinationName(string destinationName)
        {
            var existingNames = _controller.ControllerSetting.SendDestinations.Select(destination => destination.name)
                .ToArray();
            return ObjectNames.GetUniqueName(existingNames, destinationName) ?? string.Empty;
        }

        private bool LoadDmxFile([NotNull] string path)
        {
            if (_controller == null) return false;

            var result = _controller.LoadFile(path);
            if (result == false)
            {
                Debug.LogError("Failed to load file");
                return false;
            }

            var maxTimeLabel = rootVisualElement.Q<Label>("playbackMaxTimeLabel");

            var maxTime = _controller.MaxTime;
            maxTimeLabel!.text = TimeText(maxTime);

            _senderTimeSlider!.highValue = maxTime;
            _senderProgressBar!.highValue = maxTime;
            return true;
        }

        private void OnPlayTimeChanged(long time)
        {
            var newSliderValue = time;
            _senderTimeLabel!.text = TimeText(time);
            _senderTimeSlider!.SetValueWithoutNotify(newSliderValue);
            _senderProgressBar!.value = newSliderValue;
        }

        private void OnPlaybackStateChanged(PlaybackState state)
        {
            var image = state switch
            {
                PlaybackState.Play => IconHelper.PauseButton,
                PlaybackState.Pause => IconHelper.PlayButton,
                _ => IconHelper.PlayButton
            };
            _playButtonImage!.image = image;
        }

        private static string TimeText(long time)
        {
            var minutes = time / 60000;
            var seconds = time / 1000 % 60;
            var milliseconds = time % 1000;
            return $"{minutes}:{seconds:D2}.{milliseconds:D3}";
        }
    }
}
