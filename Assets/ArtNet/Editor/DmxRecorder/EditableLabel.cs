using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtNet.Editor.DmxRecorder
{
    public class EditableLabel : VisualElement
    {
        private readonly Label _label;
        private readonly TextField _textField;

        private bool _isEditing;
        private bool _isEditable = true;

        public Action<string> OnValueChanged;
        private Focusable _previouslyFocused;

        internal EditableLabel(string labelText)
        {
            _isEditing = false;
            _label = new Label(labelText);
            _textField = new TextField();

            style.flexGrow = 1.0f;
            _textField.style.flexGrow = 1.0f;

            Add(_label);

            RegisterCallback<KeyUpEvent>(KeyPressedCallback);
            RegisterCallback<KeyDownEvent>(KeyPressedCallback);

            _textField.RegisterCallback<FocusOutEvent>(OnTextFieldLostFocus);
        }

        internal void SetLabelEnabled(bool value)
        {
            _label.SetEnabled(value);
        }

        internal void SetEditable(bool value)
        {
            _isEditable = value;
        }

        private void SetValueAndNotify(string newValue)
        {
            if (EqualityComparer<string>.Default.Equals(_label.text, newValue))
                return;

            if (string.IsNullOrEmpty(newValue))
                return;

            _label.text = newValue;
            OnValueChanged?.Invoke(newValue);
        }

        internal void StartEditing()
        {
            if (_isEditing || !_isEditable) return;

            _isEditing = true;
            _textField.value = _label.text;
            Remove(_label);
            Add(_textField);
            _previouslyFocused = focusController.focusedElement;
            _textField.Focus();
        }

        private void ApplyEditing()
        {
            if (!_isEditing) return;

            SetValueAndNotify(_textField.text);

            _isEditing = false;
            Remove(_textField);
            Add(_label);
        }

        private void CancelEditing()
        {
            if (!_isEditing) return;

            _isEditing = false;
            Remove(_textField);
            Add(_label);
        }

        private void OnTextFieldLostFocus(FocusOutEvent evt)
        {
            ApplyEditing();
        }

        private void KeyPressedCallback<T>(KeyboardEventBase<T> evt) where T : KeyboardEventBase<T>, new()
        {
            if (!_isEditing) return;

            switch (evt.keyCode)
            {
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    ApplyEditing();
                    RestorePreviousFocus();

                    evt.StopImmediatePropagation();
                    break;
                case KeyCode.Escape:
                    CancelEditing();
                    RestorePreviousFocus();

                    evt.StopImmediatePropagation();
                    break;
            }
        }

        private void RestorePreviousFocus()
        {
            _previouslyFocused?.Focus();
        }
    }
}
