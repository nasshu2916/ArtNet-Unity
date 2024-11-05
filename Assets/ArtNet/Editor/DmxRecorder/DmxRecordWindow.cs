using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtNet.Editor.DmxRecorder
{
    public partial class DmxRecordWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset senderVisualTree;
        [SerializeField] private StyleSheet styleSheet;

        private Texture _playButtonTexture, _preMatQuadTexture;

        private void Update()
        {
            UpdateSender();
        }

        [MenuItem("ArtNet/DmxRecorder")]
        public static void ShowDmxRecorder()
        {
            GetWindow(typeof(DmxRecordWindow), false, "DmxRecorder");
        }

        private void OnEnable()
        {
            CreateView();
        }

        private void CreateView()
        {
            minSize = new Vector2(375, 400);
            var root = rootVisualElement;

            VisualElement visualElement = senderVisualTree.Instantiate();
            visualElement.AddToClassList("root");
            root.Add(visualElement);
            root.styleSheets.Add(styleSheet);

            _playButtonTexture = EditorGUIUtility.IconContent("PlayButton@2x").image;
            _preMatQuadTexture = EditorGUIUtility.IconContent("PreMatQuad@2x").image;

            Initialize(visualElement);
        }

        private void Initialize(VisualElement root)
        {
            InitializeSender(root);
        }
    }
}
