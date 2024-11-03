using System;
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

        private enum State
        {
            Idle,
            Recording,
        }

        [SerializeField] private VisualTreeAsset _visualTree;
        [SerializeField] private StyleSheet _styleSheet;

        private RecorderList _recorderList;

        private RecordControllerSettings _controllerSettings;

        private State _state;

        private bool IsRecording => _state == State.Recording;

        [MenuItem("ArtNet/DMX Recorder")]
        public static void ShowWindow()
        {
            var window = GetWindow<RecorderWindow>();
            window.titleContent = new GUIContent("DMX Recorder");
        }

        private void OnEnable()
        {
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

            // RecordersPanel の作成
            var recordersPanel = visualElement.Q<VisualElement>("recordersPanel");

            var addRecorderLabel = visualElement.Q<Label>("addRecorderLabel");
            addRecorderLabel.RegisterCallback<ClickEvent>(e => ShowRecorderContextMenu());
            _recorderList = new RecorderList
            {
                name = "recorderList",
                focusable = true
            };

            _recorderList.OnItemContextMenu += OnRecorderContextMenu;
            _recorderList.OnSelectionChanged += OnRecorderSelectionChanged;
            _recorderList.OnContextMenu += ShowRecorderContextMenu;
            recordersPanel.Add(_recorderList);

            SetRecordControllerSettings(RecordControllerSettings.GetOrNewGlobalSettings());
        }

        private bool DisableEditRecordSettings()
        {
            return IsRecording;
        }

        private void SetRecordControllerSettings(RecordControllerSettings settings)
        {
            _controllerSettings = settings;

            // TODO: RecorderController Class に settings を渡す

            ReloadRecorderSettings();
        }

        private void ShowRecorderContextMenu()
        {
            var menu = new GenericMenu();

            // TODO: type をハードコートではなく動的に取得する
            var recordersTypes = new[] { typeof(RecordAnimationSettings) };
            foreach (var type in recordersTypes)
            {
                var context = new GUIContent(type.Name);
                if (DisableEditRecordSettings())
                {
                    menu.AddDisabledItem(context);
                }
                else
                {
                    menu.AddItem(context, false, data => OnAddNewRecorder(type), type);
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

        private RecorderItem CreateRecorderItem(RecordSettings recordSettings)
        {
            var recorderItem = new RecorderItem(_controllerSettings, recordSettings);
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
            var selectedItem = _recorderList.Selection;
            foreach (var item in _recorderList.Items)
            {
                item.SetItemSelected(selectedItem == item);
            }

            Repaint();
        }

        private void AddRecorder(RecordSettings record, string recorderName, bool enabled)
        {
            record.name = UniqueRecorderName(recorderName);
            record.Enabled = enabled;
            _controllerSettings.AddRecorderSettings(record);

            var item = CreateRecorderItem(record);
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
            var recorder = (RecordSettings) CreateInstance(type);
            AddRecorder(recorder, ObjectNames.NicifyVariableName(recorder.DefaultName), true);

            _state = State.Idle;
        }

        private string UniqueRecorderName(string recorderName)
        {
            var existingNames = _controllerSettings.RecorderSettings.Select(settings => settings.name).ToArray();
            return ObjectNames.GetUniqueName(existingNames, recorderName);
        }
    }
}
