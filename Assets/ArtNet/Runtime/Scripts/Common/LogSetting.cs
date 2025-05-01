using System;
using JetBrains.Annotations;
using UnityEngine;

namespace ArtNet.Common
{
    [Serializable]
    public class LogSetting
    {
        [NotNull] private static readonly LogSetting Instance = new();

        [SerializeField] private bool _enableLogging = true;
        [SerializeField] private LogLevel _logLevel = LogLevel.Info;

        public static bool EnableLog
        {
            get => Instance._enableLogging;
            set
            {
                if (Instance._enableLogging == value) return;

                Instance._enableLogging = value;
                ArtNetLogger.DevLogDebug($"Logging enabled: {Instance._enableLogging}");
            }
        }

        public static LogLevel LogLevel
        {
            get => Instance._logLevel;
            set
            {
                if (Instance._logLevel == value) return;

                Instance._logLevel = value;
                ArtNetLogger.DevLogDebug($"Log level changed to {Instance._logLevel}");
            }
        }
    }
}
