using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

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
                    _dmxAddressViewers[i].value = DmxValues[i];
                }
            }
        }

        public DmxViewer()
        {
            var chunks = DmxValues.Select((v, i) => new { v, i })
                .GroupBy(x => x.i / 20);

            foreach (var chunk in chunks)
            {
                var element = new VisualElement();
                foreach (var item in chunk)
                {
                    _dmxAddressViewers[item.i] = new DmxAddressViewer(item.i + 1, item.v);
                    element.Add(_dmxAddressViewers[item.i]);
                }

                element.AddToClassList("dmx-viewer-row");
                Add(element);
            }

            var styleSheet = Resources.Load<StyleSheet>("DmxViewer");
            styleSheets.Add(styleSheet);
        }
    }
}
