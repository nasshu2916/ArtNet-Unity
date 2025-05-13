using System;
using System.Collections.Generic;
using ArtNet.Editor.DmxRecorder.Util;
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

        [NotNull] private readonly Slider _slider;
        [NotNull] private readonly Label _label;

        public float DefaultValue { get; set; } = 1.0f;

        public float Value
        {
            get => _slider.value;
            set
            {
                _slider.value = value;
                _label!.text = $"{value:F2}";
            }
        }

        public new class UxmlFactory : UxmlFactory<SnapSlider, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            [NotNull]
            private readonly UxmlFloatAttributeDescription _snapThreshold = new()
            { name = "snap-threshold", defaultValue = 0.05f };


            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var slider = (SnapSlider) ve!;

                slider.SnapThreshold = Mathf.Max(0.0f, _snapThreshold.GetValueFromBag(bag, cc));
            }
        }

        public SnapSlider()
        {
            _slider = new Slider(0f, 5f) { value = 0f }; // 初期値: 0, 範囲: 0～5
            _label = new Label("0.0");

            _slider.RegisterValueChangedCallback(evt =>
            {
                Value = GetSnappedValue(evt!.newValue);
            });
            _slider.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt?.button != 1) return;

                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Snap Enabled"), SnapEnabled, () => SnapEnabled = !SnapEnabled);

                menu.ShowAsContext();
            });
            _slider.RegisterCallback<WheelEvent>(evt =>
            {
                const float step = 0.05f;
                Value += (evt!.delta.y > 0 ? step : -step);
            });

            var resetButton = new Button(() => Value = DefaultValue)
            {
                name = "Reset value",
                tooltip = $"Reset value to default ({DefaultValue:F2})"
            };
            resetButton.Add(new Image { image = IconHelper.RefreshIcon });

            style!.flexDirection = FlexDirection.Row;
            _slider.style!.flexGrow = 1.0f;
            _label.style!.width = 40;
            _label.style!.minWidth = 40;
            _label.style!.alignSelf = Align.Center;

            Add(_slider);
            Add(_label);
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

        public void RegisterValueChangedCallback(Action<ChangeEvent<float>> action)
        {
            _slider.RegisterValueChangedCallback(evt => action!(evt));
        }
    }
}
