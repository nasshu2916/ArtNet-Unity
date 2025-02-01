using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtNet.Editor.DmxRecorder
{
    public class SendDestinationItem : VisualElement
    {
        public SendDestination SendDestination { get; }
        public SendDestinationEditor Editor { get; }

        private bool IsEnabled => SendDestination is { IsSend: true };
        private Texture _icon;

        public SendDestinationItem(PlayControllerSetting playControllerSetting, SendDestination sendDestination)
        {
            SendDestination = sendDestination;

            if (SendDestination != null)
            {
                Editor = (SendDestinationEditor) UnityEditor.Editor.CreateEditor(SendDestination);
            }

            style!.flexDirection = FlexDirection.Row;

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
            Add(iconContainer);

            var label = new Label("SendDestination");
            label.AddToClassList("SendDestinationItemLabel");
            Add(label);
        }
    }
}
