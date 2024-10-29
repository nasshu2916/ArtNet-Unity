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

    public class RecorderConfigs : ScriptableObject
    {
        [SerializeField] private RecodeFormat _recordFormat;

        private string _savePath;

        public RecodeFormat RecordFormat => _recordFormat;

        public BinaryRecordConfig BinaryConfig { get; } = new();
        public AnimationClipRecordConfig AnimationClipConfig { get; } = new();

        public static RecorderConfigs GetOrNewGlobalConfigs()
        {
            var globalPath = Path.Combine(Application.dataPath, "..", "Library", "ArtNet", "DmxRecorderConfigs.asset");
            return Load(globalPath);
        }

        private static RecorderConfigs Load(string path)
        {
            RecorderConfigs configs;
            try
            {
                var objs = InternalEditorUtility.LoadSerializedFileAndForget(path);
                configs = objs.FirstOrDefault(o => o is RecorderConfigs) as RecorderConfigs;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load RecorderConfigs: {e.Message}");
                configs = null;
            }

            if (configs == null)
            {
                configs = CreateInstance<RecorderConfigs>();
                // configs.hideFlags = HideFlags.HideAndDontSave;
                configs.name = "DmxRecorderConfigs";
            }

            configs._savePath = path;
            return configs;
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(_savePath)) return;

            try
            {
                var directory = Path.GetDirectoryName(_savePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var objs = new Object[] { this };
                InternalEditorUtility.SaveToSerializedFileAndForget(objs, _savePath, true);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save RecorderConfigs: {e.Message}");
            }
        }

        public void ChangeRecordFormat(RecodeFormat format)
        {
            EditorUtility.SetDirty(this);
            Undo.RecordObject(this, "Change Record Format");
            if (_recordFormat == format) return;

            _recordFormat = format;
            Save();
        }

        private IRecordConfig Config => RecordFormat switch
        {
            RecodeFormat.Binary => BinaryConfig,
            RecodeFormat.AnimationClip => AnimationClipConfig,
            _ => throw new System.NotImplementedException()
        };

        public bool Validate() => ValidateErrors().Count == 0;
        public List<string> ValidateErrors() => Config.ValidateErrors();
    }

    public class BinaryRecordConfig : IRecordConfig
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

    public class AnimationClipRecordConfig : IRecordConfig
    {
        public string OutputAnimationClipAssetPath { get; set; } = "Assets/Recording";

        public List<string> ValidateErrors()
        {
            return new List<string>();
        }
    }

    public interface IRecordConfig
    {
        public List<string> ValidateErrors();
    }
}
