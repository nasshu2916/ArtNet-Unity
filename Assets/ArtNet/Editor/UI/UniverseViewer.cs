using UnityEditor;
using UnityEngine.UIElements;

namespace ArtNet.Editor.UI
{
    public class UniverseViewer : VisualElement
    {
        public UniverseViewer()
        {
            var universeSelector = new ScrollView
            {
                name = "UniverseSelector"
            };
            Add(universeSelector);
            for (var i = 0; i < 16; i++)
            {
                var universeInfo = new UniverseInfo
                {
                    UniverseNumber = i
                };
                universeInfo.AddToClassList("unity-button");
                universeSelector.Add(universeInfo);
            }

            var dmxScrollView = new ScrollView();
            Add(dmxScrollView);

            var dmxViewer = new DmxViewer();
            dmxScrollView.Add(dmxViewer);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/ArtNet/Editor/UI/UniverseViewer.uss");
            styleSheets.Add(styleSheet);
        }

        public new class UxmlFactory : UxmlFactory<UniverseViewer, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }
    }
}
