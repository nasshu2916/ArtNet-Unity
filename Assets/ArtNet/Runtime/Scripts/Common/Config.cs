using UnityEditor;
using UnityEngine;

namespace ArtNet.Common
{
    public class Config : ScriptableObject
    {
        private const string AssetName = "ArtNetConfig";
        private const string AssetNameExt = ".asset";

        private const int Priority = 20000;

        [Header("Log Settings")]
        [SerializeField] private bool _enableLogging = true;
        [SerializeField] private LogLevel _logLevel = Const.Config.DefaultLogLevel;

        private static Config _instance;
        private static bool _isConfigAssetLoaded;
        private static Config _fallbackInstance;

        [MenuItem("Edit/" + "\u2699 Open ArtNet Config", false, Priority)]
        private static void MenuEditOpenConfig() { EditorSelectInstance(); }

        private static Config Instance => GetOrLoadInstance(isFallback: true);

        public static bool EnableLog => Instance._enableLogging;
        public static LogLevel LogLevel => Instance._logLevel;

        private static void EditorSelectInstance()
        {
            Selection.activeObject = GetOrLoadInstance(isForceLoad: true, isFallback: false);

            if (Selection.activeObject == null)
            {
                var instance = CreateInstanceAsset();
                Debug.LogError("Cannot find any Config resource. Created a new one.");
                Selection.activeObject = instance;
            }
        }

        private static Config GetOrLoadInstance(bool isForceLoad = false, bool isFallback = false)
        {
            if (isForceLoad) _isConfigAssetLoaded = false;

            if (_instance != null && _isConfigAssetLoaded) return _instance;

            _instance = LoadAsset(AssetName);
            _isConfigAssetLoaded = true;
            if (_instance != null) return _instance;

            if (isFallback)
            {
                _fallbackInstance = CreateInstance<Config>();
                return _fallbackInstance;
            }

            return _instance;
        }

        private static Config CreateInstanceAsset()
        {
            var asset = CreateInstance<Config>();
            CreateDirectoryAndAsset(asset, "Resources", AssetName + AssetNameExt);

            _isConfigAssetLoaded = true;
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

        private static Config LoadAsset(string assetName)
        {
            return Resources.Load<Config>(assetName);
        }
    }
}
