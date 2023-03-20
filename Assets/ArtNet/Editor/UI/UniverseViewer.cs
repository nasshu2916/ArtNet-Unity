using UnityEditor;
using UnityEngine.UIElements;

namespace ArtNet.Editor.UI
{
    public class UniverseViewer : VisualElement
    {
        private const ushort SelectableUniverseCount = 16;

        private readonly DmxViewer _dmxViewer;
        private ushort _selectedUniverseNumber;
        private DmxManager _dmxManager;

        public DmxManager DmxManager
        {
            get => _dmxManager;
            set
            {
                _dmxManager = value;
                _dmxViewer.DmxValues = _dmxManager.DmxValues(_selectedUniverseNumber);
            }
        }

        public void UpdateDmxViewer()
        {
            _dmxViewer.DmxValues = DmxManager.DmxValues(_selectedUniverseNumber);
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
                var number = i;
                var universeInfo = new UniverseInfo(number);
                universeInfo.clickable.clickedWithEventInfo += evt => OnUniverseSelected(number, evt);
                if (number == _selectedUniverseNumber) universeInfo.AddToClassList("selected");

                universeSelector.Add(universeInfo);
            }

            var dmxScrollView = new ScrollView();
            Add(dmxScrollView);

            _dmxViewer = new DmxViewer();
            dmxScrollView.Add(_dmxViewer);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/ArtNet/Editor/UI/UniverseViewer.uss");
            styleSheets.Add(styleSheet);
        }

        private void OnUniverseSelected(ushort universeNumber, EventBase evt)
        {
            if (evt.target is not UniverseInfo universeInfo) return;
            this.Q<UniverseInfo>(null, "selected")?.RemoveFromClassList("selected");
            universeInfo.AddToClassList("selected");
            _selectedUniverseNumber = universeNumber;
            _dmxViewer.DmxValues = DmxManager.DmxValues(_selectedUniverseNumber);
        }

        public new class UxmlFactory : UxmlFactory<UniverseViewer, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }
    }
}
