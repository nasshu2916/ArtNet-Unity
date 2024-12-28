using System;
using System.IO;
using System.Linq;
using System.Net;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ArtNet.Editor.DmxRecorder
{
    public class SenderSettings : ScriptableObject
    {
        [SerializeField] private string _loadFilePath;
        [SerializeField] private string _ip = "127.0.0.1";
        [SerializeField] private bool _isLoop;
        [SerializeField] private bool _isRecordSequence;
        [SerializeField] private float _speed = 1;

        public string LoadFilePath { get => _loadFilePath; set => _loadFilePath = value; }
        public IPAddress Ip { get => IPAddress.Parse(_ip); set => _ip = value.ToString(); }
        public bool IsLoop { get => _isLoop; set => _isLoop = value; }
        public float Speed { get => _speed; set => _speed = value; }

        private string _savePath;

        public static SenderSettings GetOrNewGlobalSettings()
        {
            var globalPath = Path.Combine(Application.dataPath, "..", "Library", "ArtNet", "DmxSenderSettings.asset");
            return Load(globalPath);
        }

        private static SenderSettings Load(string path)
        {
            SenderSettings settings;
            try
            {
                var objs = InternalEditorUtility.LoadSerializedFileAndForget(path);
                settings = objs.FirstOrDefault(o => o is SenderSettings) as SenderSettings;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load SenderSettings: {e.Message}");
                settings = null;
            }

            if (settings == null)
            {
                settings = CreateInstance<SenderSettings>();
                // Settings.hideFlags = HideFlags.HideAndDontSave;
                settings.name = "DmxSenderSettings";
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

                var objs = new Object[] { this };
                InternalEditorUtility.SaveToSerializedFileAndForget(objs, _savePath, true);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save SenderSettings: {e.Message}");
            }
        }
    }
}
