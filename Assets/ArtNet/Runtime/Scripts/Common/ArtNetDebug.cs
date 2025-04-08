using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace ArtNet.Common
{
    public static class ArtNetDebug
    {
        public enum LogLevel
        {
            Debug,
            Info,
            Warn,
            Error
        }

        [DebuggerStepThrough]
        private static void InternalLog(LogLevel level, string message)
        {
            if (LogSetting.LogLevel > level) return;
            if (string.IsNullOrEmpty(message)) return;

            var text = $"{message}";

            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    Debug.Log(text);
                    break;
                case LogLevel.Warn:
                    Debug.LogWarning(text);
                    break;
                case LogLevel.Error:
                    Debug.LogError(text);
                    break;
            }
        }

        [DebuggerStepThrough, Conditional("ART_NET_DEBUG_LOG")]
        public static void LogDebug(string message)
        {
            InternalLog(LogLevel.Debug, message);
        }

        [DebuggerStepThrough, Conditional("ART_NET_DEBUG_LOG")]
        public static void LogInfo(string message)
        {
            InternalLog(LogLevel.Info, message);
        }

        [DebuggerStepThrough, Conditional("ART_NET_DEBUG_LOG")]
        public static void LogWarn(string message)
        {
            InternalLog(LogLevel.Warn, message);
        }


        [DebuggerStepThrough, Conditional("ART_NET_DEBUG_LOG")]
        public static void LogError(string message)
        {
            InternalLog(LogLevel.Error, message);
        }
    }
}
