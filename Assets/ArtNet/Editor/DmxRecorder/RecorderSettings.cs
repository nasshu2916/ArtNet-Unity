using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    public abstract class RecorderSettings : ScriptableObject, ISerializationCallbackReceiver
    {
        private const int MaxPathLength = 259;

        [SerializeField] private bool _enabled = true;
        [SerializeField] protected UniverseFilter _universeFilter = new();
        [SerializeField] protected FileGenerator _fileGenerator;
        [SerializeField] private int _take = 1;

        protected internal abstract string Extension { get; }
        protected internal abstract Texture Icon { get; }

        internal abstract string DefaultName { get; }
        public string OutputAbsolutePath => FileGenerator.AbsolutePath();
        public string OutputAssetPath => FileGenerator.AssetsRelativePath();

        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        public FileGenerator FileGenerator => _fileGenerator;
        public UniverseFilter UniverseFilter => _universeFilter;

        public int Take
        {
            get => _take;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException($"The take number must be positive");
                _take = value;
            }
        }

        protected RecorderSettings()
        {
            _fileGenerator = new FileGenerator(this);
        }

        protected internal virtual void GetErrors(List<string> errors)
        {
            _universeFilter.GetErrors(errors);

            if (string.IsNullOrEmpty(FileGenerator.FileName))
            {
                errors.Add("Save path is empty");
            }
            else if (FileGenerator.FileName.Length > MaxPathLength)
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

        internal virtual void OnValidate()
        {
            _take = Mathf.Max(0, _take);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { OnBeforeSerialize(); }
        void ISerializationCallbackReceiver.OnAfterDeserialize() { OnAfterDeserialize(); }

        protected virtual void OnBeforeSerialize() { }
        protected virtual void OnAfterDeserialize() { }
        public virtual void OnAfterDuplicate() { }

        public abstract void StoreUniverseData(IEnumerable<UniverseData> universeData);
    }
}
