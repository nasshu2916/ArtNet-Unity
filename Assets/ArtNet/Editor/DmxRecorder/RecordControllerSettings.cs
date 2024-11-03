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
    public enum RecodeFormat
    {
        Binary = 0,
        AnimationClip = 1,
    }

    public class RecordControllerSettings : ScriptableObject
    {
        [SerializeField] private RecodeFormat _recordFormat;

        [SerializeField] private List<RecordSettings> _recorderSettings = new();

        private string _savePath;

        public RecodeFormat RecordFormat => _recordFormat;

        public BinaryRecordSetting BinarySetting { get; } = new();
        public AnimationClipRecordSetting AnimationClipSetting { get; } = new();

        public List<RecordSettings> RecorderSettings => _recorderSettings;

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

        public void AddRecorderSettings(RecordSettings settings)
        {
            EditorUtility.SetDirty(this);
            Undo.RegisterCompleteObjectUndo(this, "Add Recorder Settings");
            if (!_recorderSettings.Contains(settings))
            {
                _recorderSettings.Add(settings);
            }

            Save();
        }

        public void RemoveRecorderSettings(RecordSettings settings)
        {
            if (!_recorderSettings.Contains(settings)) return;

            EditorUtility.SetDirty(this);
            Undo.RegisterCompleteObjectUndo(this, "Remove Recorder Settings");
            _recorderSettings.Remove(settings);
            Undo.DestroyObjectImmediate(settings);
            Save();
        }

        public void ChangeRecordFormat(RecodeFormat format)
        {
            EditorUtility.SetDirty(this);
            Undo.RecordObject(this, "Change Record Format");
            if (_recordFormat == format) return;

            _recordFormat = format;
            Save();
        }

        private IRecordSetting Setting => RecordFormat switch
        {
            RecodeFormat.Binary => BinarySetting,
            RecodeFormat.AnimationClip => AnimationClipSetting,
            _ => throw new System.NotImplementedException()
        };

        public bool Validate() => ValidateErrors().Count == 0;
        public List<string> ValidateErrors() => Setting.ValidateErrors();
    }

    public class BinaryRecordSetting : IRecordSetting
    {
        private const string Extension = ".dmx";

        public string Directory { get; set; }
        public string FileName { get; set; }

        public string OutputPath => $"{Directory}/{FileName}{Extension}";

        public List<string> ValidateErrors()
        {
            var errors = new List<string>();
            if (!ValidateDirectory()) errors.Add("Directory is not set");
            if (!ValidateFileName()) errors.Add("FileName is not set");

            return errors;
        }

        private bool ValidateDirectory() => !string.IsNullOrEmpty(Directory);
        private bool ValidateFileName() => !string.IsNullOrEmpty(FileName);
    }

    public class AnimationClipRecordSetting : IRecordSetting
    {
        public string OutputAnimationClipAssetPath { get; set; } = "Assets/Recording";

        public List<string> ValidateErrors()
        {
            return new List<string>();
        }
    }

    public interface IRecordSetting
    {
        public List<string> ValidateErrors();
    }
}
