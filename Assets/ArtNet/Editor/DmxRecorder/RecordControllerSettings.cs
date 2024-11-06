using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ArtNet.Editor.DmxRecorder
{
    public class RecordControllerSettings : ScriptableObject
    {
        [SerializeField] private List<RecorderSettings> _recorderSettings = new();

        private string _savePath;

        public List<RecorderSettings> RecorderSettings => _recorderSettings;

        public static RecordControllerSettings GetOrNewGlobalSettings()
        {
            var globalPath = Path.Combine(Application.dataPath, "..", "Library", "ArtNet", "DmxRecorderSettings.asset");
            return Load(globalPath);
        }

        private static RecordControllerSettings Load(string path)
        {
            RecordControllerSettings settings;
            try
            {
                var objs = InternalEditorUtility.LoadSerializedFileAndForget(path);
                settings = objs.FirstOrDefault(o => o is RecordControllerSettings) as RecordControllerSettings;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load RecorderSettings: {e.Message}");
                settings = null;
            }

            if (settings == null)
            {
                settings = CreateInstance<RecordControllerSettings>();
                // Settings.hideFlags = HideFlags.HideAndDontSave;
                settings.name = "DmxRecorderSettings";
            }

            settings._savePath = path;
            return settings;
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(_savePath)) return;

            try
            {
                var directory = Path.GetDirectoryName(_savePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var recordersCopy = RecorderSettings.ToArray();

                var objs = new Object[recordersCopy.Length + 1];
                objs[0] = this;

                for (var i = 0; i < recordersCopy.Length; ++i)
                    objs[i + 1] = recordersCopy[i];

                InternalEditorUtility.SaveToSerializedFileAndForget(objs, _savePath, true);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save RecorderSettings: {e.Message}");
            }
        }

        public void AddRecorderSettings(RecorderSettings settings)
        {
            EditorUtility.SetDirty(this);
            Undo.RegisterCompleteObjectUndo(this, "Add Recorder Settings");
            if (!_recorderSettings.Contains(settings))
            {
                _recorderSettings.Add(settings);
            }

            Save();
        }

        public void RemoveRecorderSettings(RecorderSettings settings)
        {
            if (!_recorderSettings.Contains(settings)) return;

            EditorUtility.SetDirty(this);
            Undo.RegisterCompleteObjectUndo(this, "Remove Recorder Settings");
            _recorderSettings.Remove(settings);
            Undo.DestroyObjectImmediate(settings);
            Save();
        }
    }
}
