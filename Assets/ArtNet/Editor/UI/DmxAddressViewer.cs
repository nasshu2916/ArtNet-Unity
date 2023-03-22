using UnityEngine;
using UnityEngine.UIElements;

namespace ArtNet.Editor.UI
{
    public class DmxAddressViewer : VisualElement
    {
        private readonly VisualElement _bar;
        private readonly Label _addressLabel;
        private readonly Label _addressValue;
        private int _dmxValue;

        public int DmxValue
        {
            get => _dmxValue;
            set
            {
                _dmxValue = value;
                _addressValue.text = $"{_dmxValue}";
                var percent = _dmxValue * 100f / 255f;
                _bar.style.height = new StyleLength(new Length(percent, LengthUnit.Percent));
            }
        }

        public DmxAddressViewer(int address, int value) : this()
        {
            _addressLabel.text = $"{address}";
            DmxValue = value;
        }

        private DmxAddressViewer()
        {
            _bar = new VisualElement();
            _bar.AddToClassList("address-bar");
            Add(_bar);

            var addressTextPanel = new VisualElement();
            addressTextPanel.AddToClassList("address-text-panel");
            Add(addressTextPanel);

            _addressLabel = new Label();
            addressTextPanel.Add(_addressLabel);
            _addressValue = new Label
            {
                text = $"{DmxValue}"
            };
            _addressValue.AddToClassList("address-value");
            addressTextPanel.Add(_addressValue);

            var styleSheet = Resources.Load<StyleSheet>("DmxAddressViewer");
            styleSheets.Add(styleSheet);
        }
    }
}
