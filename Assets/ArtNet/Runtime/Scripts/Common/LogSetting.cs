using System;
using JetBrains.Annotations;
using UnityEngine;

namespace ArtNet.Common
{
    [Serializable]
    public class LogSetting
    {
        [NotNull] private static readonly LogSetting Instance = new();
        [SerializeField] private ArtNetDebug.LogLevel _logLevel = ArtNetDebug.LogLevel.Debug;

        public static ArtNetDebug.LogLevel LogLevel
        {
            get => Instance._logLevel;
            set
            {
                if (Instance._logLevel == value) return;

                Instance._logLevel = value;
                ArtNetDebug.LogDebug($"Log level changed to {Instance._logLevel}");
            }
        }
    }
}
