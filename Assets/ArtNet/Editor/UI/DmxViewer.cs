using UnityEngine;
using UnityEngine.UIElements;

namespace ArtNet.Editor.UI
{
    public class DmxViewer : VisualElement
    {
        private const int DmxLength = 512;
        private byte[] _dmxValues = new byte[DmxLength];
        private readonly DmxAddressViewer[] _dmxAddressViewers = new DmxAddressViewer[DmxLength];

        public byte[] DmxValues
        {
            private get => _dmxValues;
            set
            {
                _dmxValues = value;
                for (var i = 0; i < DmxLength; i++)
                {
                    _dmxAddressViewers[i].DmxValue = DmxValues[i];
                }
            }
        }

        public DmxViewer()
        {
            for (var i = 0; i < DmxLength; i++)
            {
                _dmxAddressViewers[i] = new DmxAddressViewer(i + 1, DmxValues[i]);
                Add(_dmxAddressViewers[i]);
            }

            var styleSheet = Resources.Load<StyleSheet>("DmxViewer");
            styleSheets.Add(styleSheet);
        }
    }
}
