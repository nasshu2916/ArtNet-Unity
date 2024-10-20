using UnityEngine;
using UnityEngine.UIElements;

namespace ArtNet.Editor.UI
{
    public class UniverseViewer : VisualElement, INotifyValueChanged<ushort>
    {
        private const ushort SelectableUniverseCount = 16;

        private readonly DmxViewer _dmxViewer;
        private ushort _selectedUniverse;
        private DmxManager _dmxManager;

        public ushort value
        {
            get => _selectedUniverse;
            set
            {
                if (_selectedUniverse == value) return;

                using (var pooled = ChangeEvent<ushort>.GetPooled(_selectedUniverse, value))
                {
                    pooled.target = this;
                    SetValueWithoutNotify(value);
                    SendEvent(pooled);
                }
            }
        }

        public void SetValueWithoutNotify(ushort newValue)
        {
            _selectedUniverse = newValue;
            _dmxViewer.value = _dmxManager.DmxValues(_selectedUniverse);
        }

        public DmxManager DmxManager
        {
            set
            {
                _dmxManager = value;
                UpdateDmxViewer();
            }
        }

        public void UpdateDmxViewer()
        {
            _dmxViewer.value = _dmxManager == null ? new byte[512] : _dmxManager.DmxValues(_selectedUniverse);
        }

        public UniverseViewer()
        {
            var universeSelector = new ScrollView
            {
                name = "UniverseSelector"
            };
            Add(universeSelector);

            for (ushort i = 0; i < SelectableUniverseCount; i++)
            {
                var universeInfo = new UniverseInfo(i);
                universeInfo.clickable.clickedWithEventInfo += evt => OnUniverseSelected(i, evt);
                if (i == _selectedUniverse) universeInfo.AddToClassList("selected");

                universeSelector.Add(universeInfo);
            }

            var dmxScrollView = new ScrollView();
            Add(dmxScrollView);

            _dmxViewer = new DmxViewer();
            dmxScrollView.Add(_dmxViewer);

            var styleSheet = Resources.Load<StyleSheet>("UniverseViewer");
            styleSheets.Add(styleSheet);
        }

        private void OnUniverseSelected(ushort universe, EventBase evt)
        {
            if (evt.target is not UniverseInfo universeInfo) return;
            this.Q<UniverseInfo>(null, "selected")?.RemoveFromClassList("selected");
            universeInfo.AddToClassList("selected");
            value = universe;
        }

        public new class UxmlFactory : UxmlFactory<UniverseViewer, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }
    }
}
