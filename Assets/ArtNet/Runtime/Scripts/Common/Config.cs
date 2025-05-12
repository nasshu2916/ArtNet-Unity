using UnityEngine;

namespace ArtNet.Common
{
    public partial class Config : ScriptableObject
    {
        private const string AssetName = "ArtNetConfig";
        private const string AssetNameExt = ".asset";

        private const int Priority = 20000;

        [Header("Log Settings")]
        [SerializeField]
        private bool _enableLogging = true;

        [SerializeField] private LogLevel _logLevel = Const.Config.DefaultLogLevel;

        private static Config _instance;
        private static bool _isConfigAssetLoaded;
        private static Config _defaultInstance;


        private static Config Instance => GetOrDefaultInstance();

        public static bool EnableLog => Instance._enableLogging;
        public static LogLevel LogLevel => Instance._logLevel;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void OnSetup()
        {
            LoadAsset(AssetName);
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

        private static Config LoadAsset(string assetName)
        {
            var result = Resources.Load<Config>(assetName);
            _isConfigAssetLoaded = true;
            _defaultInstance = CreateInstance<Config>();
            return result;
        }
    }
}
