using System;
using ArtNet.Editor.DmxRecorder.Util;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtNet.Editor.DmxRecorder
{
    public class RecorderItem : VisualElement
    {
        public RecorderSettings Settings { get; }
        public RecorderSettingsEditor Editor { get; }

        private readonly Toggle _toggle = new();
        private readonly EditableLabel _editableLabel;

        private bool _isDisabled;

        private Texture _icon;
        private RecorderState _state;

        public event Action<bool> OnEnableStateChanged;

        public enum RecorderState
        {
            None,
            Normal,
            HasWarnings,
            HasErrors,
            Invalid
        }

        public RecorderItem(RecordControllerSettings recordControllerSettings, RecorderSettings recorderSettings)
        {
            Settings = recorderSettings;

            if (Settings != null)
            {
                Editor = (RecorderSettingsEditor) UnityEditor.Editor.CreateEditor(Settings);
                Editor.OnRecorderValidated += OnRecorderValidated;
            }

            style.flexDirection = FlexDirection.Row;

            _toggle.RegisterValueChangedCallback(evt =>
            {
                SetItemEnabled(recordControllerSettings, evt.newValue);
            });
            Add(_toggle);

            UpdateState(false);

            var iconContainer = new IMGUIContainer(() =>
            {
                var rect = EditorGUILayout.GetControlRect();
                rect.width = rect.height = Mathf.Min(rect.width, rect.height);

                var prevColor = GUI.color;
                var color = Color.white;

                if (_isDisabled) color.a = 0.5f;

                GUI.color = color;
                if (_icon != null) GUI.DrawTexture(rect, _icon);

                GUI.color = prevColor;
            });

            iconContainer.AddToClassList("RecorderItemIcon");
            iconContainer.SetEnabled(false);
            Add(iconContainer);

            _editableLabel = new EditableLabel(recorderSettings.name)
            {
                OnValueChanged = newValue =>
                {
                    Settings.name = newValue;
                    recordControllerSettings.Save();
                }
            };
            Add(_editableLabel);

            var recorderEnabled = Settings.Enabled;
            _toggle.value = recorderEnabled;

            SetItemEnabled(recordControllerSettings, recorderEnabled);
        }

        public void UpdateState(bool checkErrorAndWarning = true)
        {
            if (Settings == null)
            {
                State = RecorderState.Invalid;
                return;
            }

            switch (checkErrorAndWarning)
            {
                case true when Settings.HasErrors():
                    State = RecorderState.HasErrors;
                    return;
                case true when Settings.HasWarnings():
                    State = RecorderState.HasWarnings;
                    return;
                default:
                    State = RecorderState.Normal;
                    break;
            }
        }

        public RecorderState State
        {
            get => _state;
            set
            {
                if (value == RecorderState.None)
                    return;

                if (_state == value)
                    return;

                switch (_state)
                {
                    case RecorderState.HasWarnings:
                        RemoveFromClassList("hasWarnings");
                        break;

                    case RecorderState.HasErrors:
                        RemoveFromClassList("hasErrors");
                        break;

                    case RecorderState.Invalid:
                        RemoveFromClassList("isInvalid");
                        break;
                }

                switch (value)
                {
                    case RecorderState.HasWarnings:
                        AddToClassList("hasWarnings");
                        _icon = IconHelper.WarningIcon;
                        break;

                    case RecorderState.HasErrors:
                        AddToClassList("hasErrors");
                        _icon = IconHelper.ErrorIcon;
                        break;

                    case RecorderState.Invalid:
                        AddToClassList("isInvalid");
                        _icon = IconHelper.ErrorIcon;
                        break;

                    case RecorderState.Normal:
                        _icon = Settings.Icon;
                        break;
                }

                _state = value;
            }
        }

        public void SetItemSelected(bool value)
        {
            if (value)
            {
                AddToClassList("selected");
            }
            else
            {
                RemoveFromClassList("selected");
            }
        }

        private void SetItemEnabled(RecordControllerSettings recordControllerSettings, bool value)
        {
            _isDisabled = !value;
            Settings.Enabled = value;
            recordControllerSettings.Save();

            _toggle.value = value;
            _editableLabel.SetLabelEnabled(value);

            if (value)
            {
                RemoveFromClassList("disabled");
            }
            else
            {
                AddToClassList("disabled");
            }

            OnEnableStateChanged?.Invoke(value);
        }

        public void SetReadOnly(bool value)
        {
            _editableLabel.SetEditable(!value);
            _toggle.SetEnabled(!value);
        }

        public void StartRenaming()
        {
            _editableLabel.StartEditing();
        }


        private void OnRecorderValidated()
        {
            UpdateState();
        }
    }
}
