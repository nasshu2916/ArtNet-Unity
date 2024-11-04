using System;
using UnityEditor;

namespace ArtNet.Editor.DmxRecorder
{
    [CustomEditor(typeof(RecorderSettings), true)]
    public class RecorderSettingsEditor : UnityEditor.Editor
    {

        internal event Action OnRecorderValidated;
        internal event Action OnDataChanged;

        protected void OnOnRecorderValidated()
        {
            OnRecorderValidated?.Invoke();
        }

        protected void OnOnDataChanged()
        {
            OnDataChanged?.Invoke();
        }
    }
}
