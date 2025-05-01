using System.Diagnostics;
using System.Runtime.CompilerServices;
using Debug = UnityEngine.Debug;

namespace ArtNet.Common
{
    public static class ArtNetDebug
    {
        private const string DefaultTag = "ArtNet";

        [DebuggerStepThrough, Conditional("ART_NET_DEBUG_LOG")]
        private static void InternalLog(LogLevel level, string tag, string message)
        {
            if (EnableLog(level) == false || string.IsNullOrEmpty(message)) return;

            var text = FormatMessage(tag, message);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool EnableLog(LogLevel level)
        {
            return LogSetting.EnableLog && LogSetting.LogLevel <= level;
        }

        private static string FormatMessage(string tag, string message)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return message;
            }

            return $"[{tag}] {message}";
        }

        [DebuggerStepThrough, Conditional("ART_NET_DEBUG_LOG")]
        public static void LogDebug(string message)
        {
            InternalLog(LogLevel.Debug, DefaultTag, message);
        }

        [DebuggerStepThrough, Conditional("ART_NET_DEBUG_LOG")]
        public static void LogDebug(string tag, string message)
        {
            InternalLog(LogLevel.Debug, tag, message);
        }

        [DebuggerStepThrough, Conditional("ART_NET_DEBUG_LOG")]
        public static void LogInfo(string message)
        {
            InternalLog(LogLevel.Info, DefaultTag, message);
        }


        [DebuggerStepThrough, Conditional("ART_NET_DEBUG_LOG")]
        public static void LogInfo(string tag, string message)
        {
            InternalLog(LogLevel.Info, tag, message);
        }

        [DebuggerStepThrough, Conditional("ART_NET_DEBUG_LOG")]
        public static void LogWarn(string message)
        {
            InternalLog(LogLevel.Warn, DefaultTag, message);
        }

        [DebuggerStepThrough, Conditional("ART_NET_DEBUG_LOG")]
        public static void LogWarn(string tag, string message)
        {
            InternalLog(LogLevel.Warn, tag, message);
        }

        [DebuggerStepThrough, Conditional("ART_NET_DEBUG_LOG")]
        public static void LogError(string message)
        {
            InternalLog(LogLevel.Error, DefaultTag, message);
        }

        [DebuggerStepThrough, Conditional("ART_NET_DEBUG_LOG")]
        public static void LogError(string tag, string message)
        {
            InternalLog(LogLevel.Error, tag, message);
        }
    }
}
