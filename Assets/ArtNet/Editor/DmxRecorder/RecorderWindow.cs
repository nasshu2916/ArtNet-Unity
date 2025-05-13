using System;
using System.Collections.Generic;
using System.Linq;
using ArtNet.Common;
using ArtNet.Editor.DmxRecorder.Util;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtNet.Editor.DmxRecorder
{
    public class RecorderWindow : EditorWindow
    {
        #region Contents

        private static class Contents
        {
            internal static readonly GUIContent DuplicateLabel = new("Duplicate");
            internal static readonly GUIContent DeleteLabel = new("Delete");

            internal static string PlayButtonTooltip => "Start recording";
            internal static string PauseButtonTooltip => "Pause recording";
            internal static string StopButtonTooltip => "Stop recording";
        }
        #endregion

        private class RecorderList : ElementItemList<RecorderItem> { }

        [SerializeField] private VisualTreeAsset _visualTree;
        [SerializeField] private StyleSheet _styleSheet;
        [SerializeField] private StyleSheet _darkStyleSheet, _lightStyleSheet;

        private static IEnumerable<Type> _cachedRecorderTypes;

        private VisualElement _addNewRecordPanel, _recorderSettingsPanel;
        private RecorderList _recorderList;
        private RecorderItem _selectedRecorderItem;

        private RecordController _controller;

        private Label _timeCode;
        private Button _playButton, _stopButton;

        private bool IsRecording => _controller?.Status == RecordingStatus.Recording;

        [MenuItem(Const.Editor.MenuItemNamePrefix + "DMX Recorder", false, Const.Editor.Priority)]
        public static void ShowWindow()
        {
            var window = GetWindow<RecorderWindow>();
            window.titleContent = new GUIContent("DMX Recorder");
        }

        private void OnEnable()
        {
            _cachedRecorderTypes ??= typeof(RecorderSettings).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(RecorderSettings)) && !t.IsAbstract);

            CreateView();
            RegisterCallbacks();
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
        }

        private void RegisterCallbacks()
        {
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update += OnUpdate;
        }

        private void UnregisterCallbacks()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.update -= OnUpdate;
        }

        private void OnUndoRedoPerformed()
        {
            ReloadRecorderSettings();
            SaveAndRepaint();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                SetRecordControllerSettings(RecordControllerSettings.GetOrNewGlobalSetting());
                ReloadRecorderSettings();
                Repaint();
            }
        }

        private void OnUpdate()
        {
            switch (_controller.Status)
            {
                case RecordingStatus.Recording:
                    _timeCode.text = TimeCodeText(_controller.GetRecordingTime());
                    break;
                case RecordingStatus.Paused:
                    break;
                case RecordingStatus.None:
                    OnUpdateRecordButton();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnUpdateRecordButton()
        {
            var recorderSettings = _controller.ControllerSettings.RecorderSettings;
            if (recorderSettings.All(x => !x.Enabled))
            {
                SetRecordButtonEnabled(false, "No recorders enabled");
                return;
            }

            if (recorderSettings.Any(x => x.Enabled && x.HasErrors()))
            {
                SetRecordButtonEnabled(false, "Some recorders have errors");
                return;
            }

            SetRecordButtonEnabled(true);
        }

        private void ReloadRecorderSettings()
        {
            if (_controller?.ControllerSettings == null)
                return;

            var recorderItems = _controller.ControllerSettings.RecorderSettings.Select(CreateRecorderItem).ToArray();
            foreach (var recorderItem in recorderItems)
                recorderItem.UpdateState();

            _recorderList.Reload(recorderItems);
        }

        private void SaveAndRepaint()
        {
            if (_controller.ControllerSettings != null)
                _controller.ControllerSettings.Save();

            Repaint();
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

            // TimeCode の作成
            _timeCode = visualElement.Q<Label>("timeCode");
            _timeCode.text = TimeCodeText(0);

            _playButton = visualElement.Q<Button>("playButton")!;
            _playButton.clicked += OnPlayButtonClicked;
            _playButton.style!.backgroundImage = (StyleBackground) IconHelper.PlayButton;
            _playButton.tooltip = Contents.PlayButtonTooltip;

            _stopButton = visualElement.Q<Button>("stopButton")!;
            _stopButton.clicked += OnStopButtonClicked;
            _stopButton.style!.backgroundImage = (StyleBackground) IconHelper.PreMatQuad;
            _stopButton.tooltip = Contents.StopButtonTooltip;
            _stopButton.SetEnabled(false);

            // RecordersPanel の作成
            var recordersPanel = visualElement.Q<VisualElement>("recordersPanel");

            _addNewRecordPanel = visualElement.Q<Label>("addRecorderLabel");
            _addNewRecordPanel.RegisterCallback<ClickEvent>(_ => ShowRecorderContextMenu());
            _recorderList = new RecorderList
            {
                name = "recorderList",
                focusable = true
            };

            _recorderList.OnItemContextMenu += OnRecorderContextMenu;
            _recorderList.OnSelectionChanged += OnRecorderSelectionChanged;
            _recorderList.OnItemRename += item => item.StartRenaming();
            _recorderList.OnContextMenu += ShowRecorderContextMenu;
            recordersPanel.Add(_recorderList);

            _recorderSettingsPanel = visualElement.Q<VisualElement>("recorderSettingsPanel");
            _recorderSettingsPanel.Add(new IMGUIContainer(RecorderSettingsGUI));

            var footerMessages = visualElement.Q<VisualElement>("footerMessages");
            footerMessages.Add(new IMGUIContainer(StatusMessagesGUI));

            SetRecordControllerSettings(RecordControllerSettings.GetOrNewGlobalSetting());
            SetSettingPanelEnabled(!DisableEditRecordSettings());
        }

        private void StatusMessagesGUI()
        {
            var activeRecorders = _controller.ControllerSettings.RecorderSettings.Where(x => x.Enabled).ToArray();

            if (activeRecorders.Length == 0)
            {
                ShowMessageInStatusBar("No active recorder", MessageType.Warning);
                return;
            }

            if (activeRecorders.Any(x => x.HasErrors()))
            {
                ShowMessageInStatusBar("Some recorders have errors", MessageType.Error);
                return;
            }


            switch (_controller.Status)
            {
                case RecordingStatus.Recording:
                    ShowMessageInStatusBar("Recording", MessageType.None);
                    break;
                case RecordingStatus.Paused:
                    ShowMessageInStatusBar("Paused", MessageType.None);
                    break;
                case RecordingStatus.None:
                    ShowMessageInStatusBar("Ready", MessageType.None);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool DisableEditRecordSettings()
        {
            return IsRecording;
        }

        private void SetRecordControllerSettings(RecordControllerSettings settings)
        {
            _controller = new RecordController(settings);
            _controller.OnStartRecording += OnStartRecording;
            _controller.OnPauseRecording += OnPauseRecording;
            _controller.OnStopRecording += OnFinishRecording;
            _controller.OnResumeRecording += OnStartRecording;

            ReloadRecorderSettings();
        }

        private void RecorderSettingsGUI()
        {
            if (_selectedRecorderItem != null)
            {
                if (_selectedRecorderItem.State == RecorderItem.RecorderState.Invalid)
                {
                    EditorGUILayout.LabelField("This Recorder has invalid settings", EditorStyles.boldLabel);
                }
                else
                {
                    var editor = _selectedRecorderItem.Editor;

                    if (editor == null)
                    {
                        EditorGUILayout.LabelField("No editor found for this Recorder", EditorStyles.boldLabel);
                    }
                    else
                    {
                        EditorGUILayout.Separator();

                        EditorGUILayout.BeginHorizontal();
                        var recorderName = editor.target.GetType().Name;
                        EditorGUILayout.LabelField("Recorder Type", ObjectNames.NicifyVariableName(recorderName));

                        var content = new GUIContent
                        {
                            tooltip = "Load or save a preset",
                            image = IconHelper.PresetIcon
                        };
                        if (GUILayout.Button(content, new GUIStyle("iconButton") { fixedWidth = 20f }))
                        {
                            var settings = editor.target as RecorderSettings;

                            if (settings != null)
                            {
                                var presetReceiver = CreateInstance<PresetRecorder>();
                                presetReceiver.Init(settings, Repaint);

                                PresetSelector.ShowSelector(settings, null, true, presetReceiver);
                            }
                        }

                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Separator();

                        EditorGUI.BeginChangeCheck();

                        editor.OnInspectorGUI();

                        if (EditorGUI.EndChangeCheck() || EditorUtility.IsDirty(_selectedRecorderItem.Settings))
                        {
                            // data changed
                            _controller.ControllerSettings.Save();
                            _selectedRecorderItem.UpdateState();
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("No recorder selected");
            }
        }

        private void ShowRecorderContextMenu()
        {
            var menu = new GenericMenu();
            var isDisabled = DisableEditRecordSettings();

            foreach (var type in _cachedRecorderTypes)
            {
                var context = new GUIContent(type.Name);
                if (isDisabled)
                {
                    menu.AddDisabledItem(context);
                }
                else
                {
                    menu.AddItem(context, false, _ => OnAddNewRecorder(type), type);
                }
            }

            menu.ShowAsContext();
        }

        private void OnRecorderContextMenu(RecorderItem recorder)
        {
            var menu = new GenericMenu();

            if (DisableEditRecordSettings())
            {
                menu.AddDisabledItem(Contents.DuplicateLabel);
                menu.AddDisabledItem(Contents.DeleteLabel);
            }
            else
            {
                menu.AddItem(Contents.DuplicateLabel, false,
                    data =>
                    {
                        DuplicateRecorder((RecorderItem) data);
                    }, recorder);

                menu.AddItem(Contents.DeleteLabel, false,
                    data =>
                    {
                        DeleteRecorder((RecorderItem) data);
                    }, recorder);
            }

            menu.ShowAsContext();
        }

        private RecorderItem CreateRecorderItem(RecorderSettings recorderSettings)
        {
            var recorderItem = new RecorderItem(_controller.ControllerSettings, recorderSettings);
            recorderItem.OnEnableStateChanged += enabled =>
            {
                if (enabled)
                {
                    _recorderList.Selection = recorderItem;
                }
            };

            return recorderItem;
        }

        private void OnRecorderSelectionChanged()
        {
            _selectedRecorderItem = _recorderList.Selection;
            foreach (var item in _recorderList.Items)
            {
                item.SetItemSelected(_selectedRecorderItem == item);
            }

            Repaint();
        }

        private void AddRecorder(RecorderSettings recorder, string recorderName, bool enabled)
        {
            recorder.name = UniqueRecorderName(recorderName);
            recorder.Enabled = enabled;
            _controller.ControllerSettings.AddRecorderSettings(recorder);

            var item = CreateRecorderItem(recorder);
            _recorderList.Add(item);
            _recorderList.Selection = item;
            _recorderList.Focus();
        }

        private void DuplicateRecorder(RecorderItem item)
        {
            var sourceSettings = item.Settings;
            var duplicatedSettings = Instantiate(sourceSettings);
            AddRecorder(duplicatedSettings, sourceSettings.name, sourceSettings.Enabled);
        }

        private void DeleteRecorder(RecorderItem item)
        {
            var settings = item.Settings;
            _controller.ControllerSettings.RemoveRecorderSettings(settings);
            _recorderList.Remove(item);
        }

        private void OnAddNewRecorder(Type type)
        {
            var recorder = (RecorderSettings) CreateInstance(type);
            AddRecorder(recorder, ObjectNames.NicifyVariableName(recorder.DefaultName), true);
        }

        private string UniqueRecorderName(string recorderName)
        {
            var existingNames = _controller.ControllerSettings.RecorderSettings.Select(settings => settings.name).ToArray();
            return ObjectNames.GetUniqueName(existingNames, recorderName);
        }

        private void OnPlayButtonClicked()
        {
            if (_controller == null)
                return;

            switch (_controller.Status)
            {
                case RecordingStatus.Recording:
                    _controller.PauseRecording();
                    break;
                case RecordingStatus.Paused:
                    _controller.ResumeRecording();
                    break;
                case RecordingStatus.None:
                    _controller.StartRecording();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnStopButtonClicked()
        {
            if (_controller == null)
                return;

            if (_controller.Status != RecordingStatus.None)
                _controller.StopRecording();
        }

        private void OnStartRecording()
        {
            _timeCode.ClearClassList();
            _timeCode.AddToClassList("recording");
            _playButton.style.backgroundImage = (StyleBackground) IconHelper.PauseButton;
            _playButton.tooltip = Contents.PauseButtonTooltip;
            _stopButton.SetEnabled(true);
            SetSettingPanelEnabled(false);
            _recorderList.Items.ForEach(x => x.SetReadOnly(true));
        }

        private void OnPauseRecording()
        {
            _timeCode.ClearClassList();
            _timeCode.AddToClassList("paused");
            _playButton.style.backgroundImage = (StyleBackground) IconHelper.PlayButton;
            _playButton.tooltip = Contents.PlayButtonTooltip;
            _stopButton.SetEnabled(true);
        }

        private void OnFinishRecording()
        {
            _timeCode.ClearClassList();
            _playButton.style.backgroundImage = (StyleBackground) IconHelper.PlayButton;
            _playButton.tooltip = Contents.PlayButtonTooltip;
            _stopButton.SetEnabled(false);
            _timeCode.text = TimeCodeText(_controller.GetRecordingTime());
            SetSettingPanelEnabled(true);
            _recorderList.Items.ForEach(x => x.SetReadOnly(false));
        }

        private void SetRecordButtonEnabled(bool enabled, string tooltip = null)
        {
            _playButton.SetEnabled(enabled);
            _playButton.tooltip = tooltip;
        }

        private void SetSettingPanelEnabled(bool enabled)
        {
            _addNewRecordPanel.SetEnabled(enabled);
            _recorderSettingsPanel.SetEnabled(enabled);
        }

        private static void ShowMessageInStatusBar(string msg, MessageType messageType)
        {
            var rect = EditorGUILayout.GetControlRect();

            if (messageType != MessageType.None)
            {
                var iconRect = rect;
                iconRect.width = iconRect.height;

                var icon = messageType switch
                {
                    MessageType.Error => IconHelper.ErrorIcon,
                    MessageType.Warning => IconHelper.WarningIcon,
                    MessageType.Info => IconHelper.InfoIcon,
                    _ => null
                };

                GUI.DrawTexture(iconRect, icon);
                rect.xMin = iconRect.xMax + 5.0f;
            }

            GUI.Label(rect, msg);
        }

        private static string TimeCodeText(long time)
        {
            var hours = time / 3600000;
            var minutes = time / 60000;
            var seconds = time / 1000 % 60;
            var milliseconds = time % 1000;
            return $"{MspaceText(hours)}:{MspaceText(minutes)}:{MspaceText(seconds)}:{MspaceText(milliseconds, 3)}";
        }

        private static string MspaceText(long value, int padding = 2, int mspace = 36)
        {
            var text = value.ToString().PadLeft(padding, '0');
            return $"<mspace={mspace}px>{text}</mspace>";
        }
    }
}
