using System.Diagnostics;
using System.Runtime.CompilerServices;
using Debug = UnityEngine.Debug;

namespace ArtNet.Common
{
    public static partial class ArtNetLogger
    {
        private const string DefaultTag = "ArtNet";

        [DebuggerStepThrough]
        private static void InternalLog(LogLevel level, string tag, string message)
        {
            if (DisableLog(level)) return;

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
        private static bool DisableLog(LogLevel level)
        {
            return !Config.EnableLog || Config.LogLevel > level;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string FormatMessage(string tag, string message)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return message;
            }

            return $"[<color=cyan>{tag}</color>] {message}";
        }
    }
}
