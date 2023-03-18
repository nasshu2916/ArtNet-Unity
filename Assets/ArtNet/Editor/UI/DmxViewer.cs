using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace ArtNet.Editor.UI
{
    public class DmxViewer : VisualElement
    {
        public byte[] DmxValues { get; set; } = new byte[512];

        public DmxViewer()
        {
            for (var i = 0; i < DmxValues.Length; i++)
            {
                var addressViewer = new DmxAddressViewer
                {
                    DmxAddress = i,
                    // DmxValue = DmxValues[i]
                    // FIXME: Random value for testing
                    DmxValue = new Random().Next(0, 255)
                };
                Add(addressViewer);
            }

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/ArtNet/Editor/UI/DmxViewer.uss");
            styleSheets.Add(styleSheet);
        }
    }
}
