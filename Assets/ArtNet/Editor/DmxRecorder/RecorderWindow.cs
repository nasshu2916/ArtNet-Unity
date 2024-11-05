using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
        }
        #endregion

        private class RecorderList : ElementItemList<RecorderItem> { }

        [SerializeField] private VisualTreeAsset _visualTree;
        [SerializeField] private StyleSheet _styleSheet;

        private static IEnumerable<Type> _cachedRecorderTypes;

        private RecorderList _recorderList;
        private RecorderItem _selectedRecorderItem;

        private RecordController _controller;
        private RecordControllerSettings _controllerSettings;

        private Label _timeCode;
        private Button _playButton, _stopButton;

        private bool IsRecording => _controller?.Status == RecordingStatus.Recording;


        [MenuItem("ArtNet/DMX Recorder")]
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

        private void RegisterCallbacks()
        {
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        private void OnUndoRedoPerformed()
        {
            ReloadRecorderSettings();
            SaveAndRepaint();
        }

        private void ReloadRecorderSettings()
        {
            if (_controllerSettings == null)
                return;

            var recorderItems = _controllerSettings.RecorderSettings.Select(CreateRecorderItem).ToArray();
            foreach (var recorderItem in recorderItems)
                recorderItem.UpdateState();

            _recorderList.Reload(recorderItems);
        }

        private void SaveAndRepaint()
        {
            if (_controllerSettings != null)
                _controllerSettings.Save();

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

            VisualElement visualElement = _visualTree.Instantiate();
            visualElement.AddToClassList("root");
            root.Add(visualElement);
            root.styleSheets.Add(_styleSheet);

            // TimeCode の作成
            _timeCode = visualElement.Q<Label>("timeCode");
            // TODO: TimeCode を更新する

            _playButton = visualElement.Q<Button>("playButton");
            _playButton.clicked += OnPlayButtonClicked;
            _playButton.Add(new Image { image = IconHelper.PlayButton });

            _stopButton = visualElement.Q<Button>("stopButton");
            _stopButton.clicked += OnStopButtonClicked;
            _stopButton.Add(new Image { image = IconHelper.PreMatQuad });
            _stopButton.SetEnabled(false);

            // RecordersPanel の作成
            var recordersPanel = visualElement.Q<VisualElement>("recordersPanel");

            var addRecorderLabel = visualElement.Q<Label>("addRecorderLabel");
            addRecorderLabel.RegisterCallback<ClickEvent>(_ => ShowRecorderContextMenu());
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

            var recorderSettingsPanel = visualElement.Q<VisualElement>("recorderSettingsPanel");
            recorderSettingsPanel.Add(new IMGUIContainer(RecorderSettingsGUI));

            SetRecordControllerSettings(RecordControllerSettings.GetOrNewGlobalSettings());
        }

        private bool DisableEditRecordSettings()
        {
            return IsRecording;
        }

        private void SetRecordControllerSettings(RecordControllerSettings settings)
        {
            _controllerSettings = settings;
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

                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Separator();

                        EditorGUI.BeginChangeCheck();

                        editor.OnInspectorGUI();

                        if (EditorGUI.EndChangeCheck() || EditorUtility.IsDirty(_selectedRecorderItem.Settings))
                        {
                            // data changed
                            _controllerSettings.Save();
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

            foreach (var type in _cachedRecorderTypes)
            {
                var context = new GUIContent(type.Name);
                if (DisableEditRecordSettings())
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
            var recorderItem = new RecorderItem(_controllerSettings, recorderSettings);
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
            _controllerSettings.AddRecorderSettings(recorder);

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
            _controllerSettings.RemoveRecorderSettings(settings);
            _recorderList.Remove(item);
        }

        private void OnAddNewRecorder(Type type)
        {
            var recorder = (RecorderSettings) CreateInstance(type);
            AddRecorder(recorder, ObjectNames.NicifyVariableName(recorder.DefaultName), true);
        }

        private string UniqueRecorderName(string recorderName)
        {
            var existingNames = _controllerSettings.RecorderSettings.Select(settings => settings.name).ToArray();
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
            _playButton.Clear();
            _playButton.Add(new Image { image = IconHelper.PauseButton });
            _stopButton.SetEnabled(true);
        }

        private void OnPauseRecording()
        {
            _timeCode.ClearClassList();
            _timeCode.AddToClassList("paused");
            _playButton.Clear();
            _playButton.Add(new Image { image = IconHelper.PlayButton });
            _stopButton.SetEnabled(true);
        }

        private void OnFinishRecording()
        {
            _timeCode.ClearClassList();
            _playButton.Clear();
            _playButton.Add(new Image { image = IconHelper.PlayButton });
            _stopButton.SetEnabled(false);
        }
    }
}
