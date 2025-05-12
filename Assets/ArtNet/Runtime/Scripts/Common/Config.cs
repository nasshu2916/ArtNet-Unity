using UnityEditor;
using UnityEngine;

namespace ArtNet.Common
{
    public class Config : ScriptableObject
    {
        private const string AssetName = "ArtNetConfig";
        private const string AssetNameExt = ".asset";

        private const int Priority = 20000;

        [Header("Log Settings")] [SerializeField]
        private bool _enableLogging = true;

        [SerializeField] private LogLevel _logLevel = Const.Config.DefaultLogLevel;

        private static Config _instance;
        private static bool _isConfigAssetLoaded;
        private static Config _defaultInstance;

        [MenuItem("Edit/" + "\u2699 Open ArtNet Config", false, Priority)]
        private static void MenuEditOpenConfig() { EditorSelectInstance(); }

        private static Config Instance => GetOrDefaultInstance();

        public static bool EnableLog => Instance._enableLogging;
        public static LogLevel LogLevel => Instance._logLevel;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void OnSetup()
        {
            LoadAsset(AssetName);
        }

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

        private static Config GetOrDefaultInstance()
        {
            if (_instance) return _instance;
            if (_isConfigAssetLoaded) return _defaultInstance;

            LoadAsset(AssetName);
            return _instance ? _instance : _defaultInstance;
        }

        private static Config GetOrLoadInstance()
        {
            if (_instance) return _instance;

            return LoadAsset(AssetName);
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

        private static Config LoadAsset(string assetName)
        {
            var result = Resources.Load<Config>(assetName);
            _isConfigAssetLoaded = true;
            _defaultInstance = CreateInstance<Config>();
            return result;
        }
    }
}
