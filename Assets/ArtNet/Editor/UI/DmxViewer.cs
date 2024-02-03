using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtNet.Editor.UI
{
    public class DmxViewer : VisualElement, INotifyValueChanged<byte[]>
    {
        private const int DmxLength = 512;
        private byte[] _dmxValues = new byte[DmxLength];
        private readonly DmxAddressViewer[] _dmxAddressViewers = new DmxAddressViewer[DmxLength];

        public byte[] value
        {
            get => _dmxValues;
            set
            {
                if (_dmxValues.SequenceEqual(value)) return;

                using (var pooled = ChangeEvent<byte[]>.GetPooled(_dmxValues, value))
                {
                    pooled.target = this;
                    SetValueWithoutNotify(value);
                    SendEvent(pooled);
                }
            }
        }

        public void SetValueWithoutNotify(byte[] newValues)
        {
            var dmxLength = newValues.Length;
            Buffer.BlockCopy(newValues, 0, _dmxValues, 0, dmxLength);
            for (var i = 0; i < dmxLength; i++)
            {
                _dmxAddressViewers[i].value = newValues[i];
            }
        }

        public DmxViewer()
        {
            var chunks = value.Select((v, i) => new { v, i })
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

        public new class UxmlFactory : UxmlFactory<DmxViewer, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }
    }
}
