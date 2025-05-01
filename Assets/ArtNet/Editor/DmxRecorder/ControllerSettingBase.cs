using System;
using System.IO;
using System.Linq;
using ArtNet.Common;
using JetBrains.Annotations;
using UnityEditorInternal;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    public abstract class ControllerSettingBase : ScriptableObject
    {
        private string SavePath { get; set; }

        protected abstract UnityEngine.Object[] SaveObjects();

        [NotNull]
        protected static T GetOrNewGlobalSetting<T>([NotNull] string name) where T : ControllerSettingBase, new()
        {
            var globalPath = Path.Combine(Application.dataPath, "..", "Library", "ArtNet", $"{name}.asset");
            return LoadOrNew<T>(globalPath, name);
        }

        [NotNull]
        private static T LoadOrNew<T>(string path, [NotNull] string defaultName) where T : ControllerSettingBase
        {
            T setting;
            try
            {
                var objs = InternalEditorUtility.LoadSerializedFileAndForget(path);
                setting = objs?.FirstOrDefault(o => o is T) as T;
            }
            catch (Exception e)
            {
                ArtNetLogger.LogError($"Failed to load RecorderSettings: {e.Message}");
                setting = null;
            }

            if (setting == null)
            {
                setting = CreateInstance<T>()!;
                // setting.hideFlags = HideFlags.HideAndDontSave;
                setting.name = defaultName;
            }

            setting.SavePath = path;
            return setting;
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(SavePath)) return;

            try
            {
                var directory = Path.GetDirectoryName(SavePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var objs = SaveObjects();
                InternalEditorUtility.SaveToSerializedFileAndForget(objs, SavePath, true);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save RecorderSettings: {e.Message}");
            }
        }
    }
}
