using UnityEngine;
using UnityEngine.UIElements;

namespace ArtNet.Editor.UI
{
    public class DmxAddressViewer : VisualElement, INotifyValueChanged<byte>
    {
        public new class UxmlFactory : UxmlFactory<DmxAddressViewer, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlIntAttributeDescription _addressNumAttr = new()
            { name = "address-num" };

            private UxmlIntAttributeDescription _addressValueAttr = new()
            { name = "address-value" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var testElement = (DmxAddressViewer) ve;
                testElement._addressNumLabel.text = _addressNumAttr.GetValueFromBag(bag, cc).ToString();
                testElement.value = (byte) _addressValueAttr.GetValueFromBag(bag, cc);
            }
        }

        private readonly VisualElement _bar;
        private readonly Label _addressNumLabel;
        private readonly Label _addressValueLabel;
        private byte _addressValue;

        private int addressNum => int.Parse(_addressNumLabel.text);
        private int addressValue => int.Parse(_addressValueLabel.text);

        public byte value
        {
            get => _addressValue;
            set
            {
                if (_addressValue == value) return;

                using (var pooled = ChangeEvent<byte>.GetPooled(_addressValue, value))
                {
                    pooled.target = this;
                    SetValueWithoutNotify(value);
                    SendEvent(pooled);
                }
            }
        }

        public void SetValueWithoutNotify(byte newValue)
        {
            _addressValue = newValue;
            _addressValueLabel.text = newValue.ToString();
            var percent = newValue * 100f / 255f;
            _bar.style.height = new StyleLength(new Length(percent, LengthUnit.Percent));
        }

        public DmxAddressViewer(int address, byte value) : this()
        {
            _addressNumLabel.text = $"{address}";
            this.value = value;
        }

        public DmxAddressViewer()
        {
            _bar = new VisualElement();
            _bar.AddToClassList("dmx-address-bar");
            Add(_bar);

            var addressTextPanel = new VisualElement();
            addressTextPanel.AddToClassList("dmx-address-panel");
            Add(addressTextPanel);

            _addressNumLabel = new Label();
            addressTextPanel.Add(_addressNumLabel);
            _addressValueLabel = new Label
            {
                text = value.ToString()
            };
            _addressValueLabel.AddToClassList("dmx-address-value");
            addressTextPanel.Add(_addressValueLabel);

            var styleSheet = Resources.Load<StyleSheet>("DmxAddressViewer");
            styleSheets.Add(styleSheet);
        }
    }
}
