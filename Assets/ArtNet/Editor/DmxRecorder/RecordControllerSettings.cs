using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    public class RecordControllerSettings : ControllerSettingBase
    {
        [SerializeField] private List<RecorderSettings> _recorderSettings = new();

        public List<RecorderSettings> RecorderSettings => _recorderSettings;

        public static RecordControllerSettings GetOrNewGlobalSetting()
        {
            return GetOrNewGlobalSetting<RecordControllerSettings>("DmxRecorderSettings");
        }

        protected override Object[] SaveObjects()
        {
            var recordersCopy = RecorderSettings.ToArray();
            var objs = new Object[recordersCopy.Length + 1];
            objs[0] = this;

            for (var i = 0; i < recordersCopy.Length; ++i)
                objs[i + 1] = recordersCopy[i];
            return objs;
        }

        public void AddRecorderSettings(RecorderSettings settings)
        {
            EditorUtility.SetDirty(this);
            Undo.RegisterCompleteObjectUndo(this, "Add Recorder Settings");
            if (!RecorderSettings.Contains(settings))
            {
                RecorderSettings.Add(settings);
            }

            Save();
        }

        public void RemoveRecorderSettings(RecorderSettings settings)
        {
            if (!RecorderSettings.Contains(settings)) return;

            EditorUtility.SetDirty(this);
            Undo.RegisterCompleteObjectUndo(this, "Remove Recorder Settings");
            RecorderSettings.Remove(settings);
            Undo.DestroyObjectImmediate(settings);
            Save();
        }
    }
}
