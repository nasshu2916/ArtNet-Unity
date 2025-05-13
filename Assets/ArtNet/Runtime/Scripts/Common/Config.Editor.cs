#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ArtNet.Common
{
    public partial class Config
    {
        [MenuItem("Edit/" + "\u2699 Open ArtNet Config", false, Priority)]
        private static void MenuEditOpenConfig() { EditorSelectInstance(); }

        private static void EditorSelectInstance()
        {
            Selection.activeObject = GetOrLoadInstance();

            if (Selection.activeObject is null)
            {
                var instance = CreateInstanceAsset();
                Debug.LogError("Cannot find any Config resource. Created a new one.");
                Selection.activeObject = instance;
            }
        }

        private static Config CreateInstanceAsset()
        {
            var asset = CreateInstance<Config>();
            CreateDirectoryAndAsset(asset, "Resources", AssetName + AssetNameExt);

            _isConfigAssetLoaded = true;
            _defaultInstance = asset;
            _instance = asset;
            return asset;
        }

        private static void CreateDirectoryAndAsset(Config obj, string directory, string assetName)
        {
            if (!AssetDatabase.IsValidFolder($"Assets/{directory}"))
                AssetDatabase.CreateFolder("Assets", directory);

            AssetDatabase.CreateAsset(obj, $"Assets/{directory}/{assetName}");
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
