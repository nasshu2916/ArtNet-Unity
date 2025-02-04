using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtNet.Editor.DmxRecorder
{
    public class SnapSlider : VisualElement
    {
        [NotNull] public float[] SnapPoints { get; set; } = { 0, 0.5f, 1f, 2f, 3f };
        public float SnapThreshold { get; set; } = 0.1f;
        public bool SnapEnabled { get; set; } = true;

        public new class UxmlFactory : UxmlFactory<SnapSlider, UxmlTraits>
        {
        }

        public SnapSlider()
        {
            var slider = new Slider(0f, 5f) { value = 0f }; // 初期値: 0, 範囲: 0～5
            var label = new Label("0.0");

            slider.RegisterValueChangedCallback(evt =>
            {
                slider.value = GetSnappedValue(evt!.newValue);
                label!.text = $"{slider!.value:F2}";
            });
            slider.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt?.button != 1) return;

                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Snap Enabled"), SnapEnabled, () => SnapEnabled = !SnapEnabled);

                menu.ShowAsContext();
            });
            slider.RegisterCallback<WheelEvent>(evt =>
            {
                const float step = 0.05f;
                slider.value += (evt!.delta.y > 0 ? step : -step);
            });

            var resetButton = new Button(() => slider.value = 1) { text = "Reset" };
            resetButton.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt!.button != 1) return;

                slider.value = 1;
            });

            style!.flexDirection = FlexDirection.Row;
            slider.style!.flexGrow = 1.0f;
            label.style!.width = 40;
            label.style!.minWidth = 40;
            label.style!.alignSelf = Align.Center;

            Add(slider);
            Add(label);
            Add(resetButton);
        }

        private float GetSnappedValue(float value)
        {
            if (!SnapEnabled) return value;

            var closestSnap = value;
            var minDistance = Mathf.Infinity;

            foreach (var snap in SnapPoints)
            {
                var distance = Mathf.Abs(value - snap);
                if (minDistance <= distance || SnapThreshold < distance) continue;

                closestSnap = snap;
                minDistance = distance;
            }

            return closestSnap;
        }
    }
}
