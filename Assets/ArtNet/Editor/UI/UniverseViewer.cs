using UnityEditor;
using UnityEngine.UIElements;

namespace ArtNet.Editor.UI
{
    public class UniverseViewer : VisualElement
    {
        private const ushort SelectableUniverseCount = 16;

        private readonly DmxViewer _dmxViewer;
        private ushort _selectedUniverseNumber;

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
            if (evt.target is not UniverseInfo button) return;
            this.Q<UniverseInfo>(null, "selected")?.RemoveFromClassList("selected");
            button.AddToClassList("selected");
            _selectedUniverseNumber = universeNumber;

            // FIXME: This is just for testing
            var dmx = new byte[512];
            for (var i = 0; i < 512; i++)
            {
                dmx[i] = (byte)(universeNumber * 10);
            }
            _dmxViewer.DmxValues = dmx;
        }


        public new class UxmlFactory : UxmlFactory<UniverseViewer, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }
    }
}
