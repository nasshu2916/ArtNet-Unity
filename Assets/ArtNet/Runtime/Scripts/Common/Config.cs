using UnityEditor;
using UnityEngine;

namespace ArtNet.Common
{
    public class Config : ScriptableObject
    {
        private const string AssetName = "ArtNetConfig";
        private const string AssetNameExt = ".asset";

        private const int Priority = 20000;

        [SerializeField] private bool _enableLogging = true;
        [SerializeField] private LogLevel _logLevel = Const.Config.DefaultLogLevel;

        private static Config _instance;

        [MenuItem("Edit/" + "\u2699 Open ArtNet Config", false, Priority)]
        private static void MenuEditOpenConfig() { EditorSelectInstance(); }

        private static Config Instance => GetInstance();
        public static bool EnableLog => Instance._enableLogging;
        public static LogLevel LogLevel => Instance._logLevel;

        private static void EditorSelectInstance()
        {
            Selection.activeObject = Instance;
            if (Selection.activeObject == null)
                Debug.LogError("Cannot find any Config resource");
        }

        private static Config GetInstance()
        {
            if (_instance != null) return _instance;

            _instance = LoadAsset(AssetName);
            if (_instance != null) return _instance;

#if UNITY_EDITOR
            return CreateInstanceAsset();
#else
            return CreateInstance<Config>();
#endif
        }

        private static Config CreateInstanceAsset()
        {
            var asset = CreateInstance<Config>();
            CreateDirectoryAndAsset(asset, "Resources", AssetName + AssetNameExt);
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
