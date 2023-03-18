using UnityEditor;
using UnityEngine.UIElements;

namespace ArtNet.Editor.UI
{
    public class DmxAddressViewer : VisualElement
    {
        private readonly VisualElement _bar;
        private readonly Label _addressLabel;
        private readonly Label _addressValue;

        private int _dmxAddress;
        private int _dmxValue;

        public int DmxAddress
        {
            get => _dmxAddress;
            set
            {
                _dmxAddress = value;
                _addressLabel.text = $"{_dmxAddress}";
            }
        }

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

        public DmxAddressViewer()
        {
            _bar = new VisualElement
            {
                name = "AddressBar"
            };
            Add(_bar);

            var addressTextPanel = new VisualElement
            {
                name = "AddressTextPanel"
            };
            Add(addressTextPanel);

            _addressLabel = new Label
            {
                name = "AddressLabel",
                text = $"{_dmxAddress}"
            };
            addressTextPanel.Add(_addressLabel);
            _addressValue = new Label
            {
                name = "AddressValue",
                text = $"{_dmxValue}"
            };
            addressTextPanel.Add(_addressValue);


            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/ArtNet/Editor/UI/DmxAddressViewer.uss");
            styleSheets.Add(styleSheet);
            style.minHeight = 50;
            style.minWidth = 50;
        }
    }
}
