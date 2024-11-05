using System.Collections.Generic;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    public abstract class RecorderSettings : ScriptableObject, ISerializationCallbackReceiver
    {
        private const int MaxPathLength = 259;

        [SerializeField] private string _outputPath;
        [SerializeField] private bool _enabled = true;

        protected internal abstract string Extension { get; }
        protected internal abstract Texture Icon { get; }

        internal abstract string DefaultName { get; }

        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        protected internal virtual void GetErrors(List<string> errors)
        {
            if (string.IsNullOrEmpty(_outputPath))
            {
                errors.Add("Save path is empty");
            }
            else if (_outputPath.Length > MaxPathLength)
            {
                errors.Add($"Save path is too long. Max length is {MaxPathLength}");
            }
        }

        protected internal virtual void GetWarnings(List<string> warnings)
        {
        }

        protected internal virtual bool HasErrors()
        {
            var errors = new List<string>();
            GetErrors(errors);
            return errors.Count > 0;
        }

        protected internal virtual bool HasWarnings()
        {
            var warnings = new List<string>();
            GetWarnings(warnings);
            return warnings.Count > 0;
        }

        internal virtual void OnValidate() { }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { OnBeforeSerialize(); }
        void ISerializationCallbackReceiver.OnAfterDeserialize() { OnAfterDeserialize(); }

        protected virtual void OnBeforeSerialize() { }
        protected virtual void OnAfterDeserialize() { }
    }
}
