using System;
using UnityEditor;

namespace ArtNet.Editor.DmxRecorder
{
    [CustomEditor(typeof(RecorderSettings), true)]
    public class RecorderSettingsEditor : UnityEditor.Editor
    {
        internal event Action OnRecorderValidated;

        protected void OnOnRecorderValidated()
        {
            OnRecorderValidated?.Invoke();
        }
    }
}
