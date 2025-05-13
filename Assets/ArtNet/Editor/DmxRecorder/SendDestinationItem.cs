using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtNet.Editor.DmxRecorder
{
    public class SendDestinationItem : VisualElement
    {
        [NotNull] public SendDestination SendDestination { get; }
        public SendDestinationEditor Editor { get; }

        private bool IsEnabled => SendDestination is { IsSend: true };

        private readonly PlayControllerSetting _playControllerSetting;
        [NotNull] private readonly Toggle _toggle = new();
        [NotNull] private readonly EditableLabel _editableLabel;

        private Texture _icon;

        public SendDestinationItem([NotNull] PlayControllerSetting playControllerSetting,
            [NotNull] SendDestination sendDestination)
        {
            SendDestination = sendDestination;

            if (SendDestination != null)
            {
                Editor = (SendDestinationEditor) UnityEditor.Editor.CreateEditor(SendDestination);
            }

            var header = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                }
            };

            _toggle.RegisterValueChangedCallback(evt =>
            {
                SetItemEnabled(evt!.newValue);
            });
            header.Add(_toggle);

            var iconContainer = new IMGUIContainer(() =>
            {
                var rect = EditorGUILayout.GetControlRect();
                rect.width = rect.height = Mathf.Min(rect.width, rect.height);

                var prevColor = GUI.color;
                var color = Color.white;

                if (!IsEnabled) color.a = 0.5f;

                GUI.color = color;
                if (_icon != null) GUI.DrawTexture(rect, _icon);

                GUI.color = prevColor;
            });

            iconContainer.AddToClassList("SendDestinationItemIcon");
            iconContainer.SetEnabled(false);
            header.Add(iconContainer);

            _editableLabel = new EditableLabel(sendDestination.name)
            {
                OnValueChanged = value =>
                {
                    SendDestination.name = value ?? string.Empty;
                    EditorUtility.SetDirty(SendDestination);
                    playControllerSetting.Save();
                }
            };
            _editableLabel.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt!.clickCount < 2) return;

                _editableLabel.StartEditing();
            });

            header.Add(_editableLabel);
            Add(header);

            var destinationElement = Editor.CreateInspectorGUI();
            Add(destinationElement);

            Editor!.OnValueChanged += () =>
            {
                EditorUtility.SetDirty(SendDestination);
                playControllerSetting.Save();
            };

            SetItemEnabled(IsEnabled);
            _playControllerSetting = playControllerSetting;
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

        private void SetItemEnabled(bool value)
        {
            SendDestination.IsSend = value;
            EditorUtility.SetDirty(SendDestination);
            _playControllerSetting?.Save();

            _toggle.value = value;

            if (value)
            {
                RemoveFromClassList("disabled");
            }
            else
            {
                AddToClassList("disabled");
            }
        }
    }
}
