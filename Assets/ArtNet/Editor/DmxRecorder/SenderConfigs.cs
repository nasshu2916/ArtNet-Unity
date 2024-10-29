using System;
using System.IO;
using System.Linq;
using System.Net;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ArtNet.Editor.DmxRecorder
{
    public class SenderConfigs : ScriptableObject
    {
        [SerializeField] private string _loadFilePath;
        [SerializeField] private string _ip = "127.0.0.1";
        [SerializeField] private bool _isLoop;
        [SerializeField] private bool _isRecordSequence;
        [SerializeField] private float _speed = 1;

        public string LoadFilePath { get => _loadFilePath; set => _loadFilePath = value; }
        public IPAddress Ip { get => IPAddress.Parse(_ip); set => _ip = value.ToString(); }
        public bool IsLoop { get => _isLoop; set => _isLoop = value; }
        public bool IsRecordSequence { get => _isRecordSequence; set => _isRecordSequence = value; }
        public float Speed { get => _speed; set => _speed = value; }

        private string _savePath;

        public static SenderConfigs GetOrNewGlobalConfigs()
        {
            var globalPath = Path.Combine(Application.dataPath, "..", "Library", "ArtNet", "DmxSenderConfigs.asset");
            return Load(globalPath);
        }

        private static SenderConfigs Load(string path)
        {
            SenderConfigs configs;
            try
            {
                var objs = InternalEditorUtility.LoadSerializedFileAndForget(path);
                configs = objs.FirstOrDefault(o => o is SenderConfigs) as SenderConfigs;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load SenderConfigs: {e.Message}");
                configs = null;
            }

            if (configs == null)
            {
                configs = CreateInstance<SenderConfigs>();
                // configs.hideFlags = HideFlags.HideAndDontSave;
                configs.name = "DmxSenderConfigs";
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
                Debug.LogError($"Failed to save SenderConfigs: {e.Message}");
            }
        }
    }
}
