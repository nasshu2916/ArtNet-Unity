using System.Collections.Generic;
using ArtNet.Editor.DmxRecorder;
using ArtNet.Editor.UnityRecorder.Input;
using UnityEditor.Recorder;
using UnityEngine;
using DefaultWildcard = UnityEditor.Recorder.DefaultWildcard;
using RecorderSettings = UnityEditor.Recorder.RecorderSettings;

namespace ArtNet.Editor.UnityRecorder
{
    [RecorderSettings(typeof(ArtNetRecorder), "Art-Net")]
    public class ArtNetRecorderSettings : RecorderSettings
    {
        public enum ArtNetRecorderOutputFormat
        {
            Binary,
            AnimationClip
        }

        [SerializeField] private ArtNetInputSettings _artNetInputSettings = new();

        public ArtNetInputSettings ArtNetInputSettings => _artNetInputSettings;
        public override IEnumerable<RecorderInputSettings> InputsSettings
        {
            get { yield return _artNetInputSettings; }
        }

        [SerializeField] private ArtNetRecorderOutputFormat _outputFormat = ArtNetRecorderOutputFormat.Binary;
        [SerializeField] private UniverseFilter _universeFilter = new();
        [SerializeField] private bool _isCompressBinary = true;

        public ArtNetRecorderOutputFormat OutputFormat
        {
            get => _outputFormat;
            set => _outputFormat = value;
        }

        protected override string Extension
        {
            get
            {
                return _outputFormat switch
                {
                    ArtNetRecorderOutputFormat.Binary => "dmx",
                    ArtNetRecorderOutputFormat.AnimationClip => "anim",
                    _ => throw new System.ArgumentOutOfRangeException()
                };
            }
        }

        public UniverseFilter UniverseFilter => _universeFilter;
        public bool IsCompressBinary => _isCompressBinary;

        public ArtNetRecorderSettings()
        {
            FileNameGenerator.AddWildcard(DefaultWildcard.GeneratePattern("GameObject"), GameObjectNameResolver);
            FileNameGenerator.AddWildcard(DefaultWildcard.GeneratePattern("GameObjectScene"), GameObjectSceneNameResolver);

            FileNameGenerator.ForceAssetsFolder = true;
            FileNameGenerator.Root = OutputPath.Root.AssetsFolder;
            FileNameGenerator.FileName = "artnet_dmx_" + DefaultWildcard.Take;
        }

        private string GameObjectNameResolver(RecordingSession session)
        {
            var dmxManager = ArtNetInputSettings.DmxManager;
            return dmxManager != null ? dmxManager.gameObject.name : "None";
        }

        private string GameObjectSceneNameResolver(RecordingSession session)
        {
            var dmxManager = ArtNetInputSettings.DmxManager;
            return dmxManager != null ? dmxManager.gameObject.scene.name : "None";
        }

        protected override void GetErrors(List<string> errors)
        {
            base.GetErrors(errors);
            _universeFilter.GetErrors(errors);

            if (ArtNetInputSettings.GameObject == null)
                errors.Add("No assigned game object to record");
            else if (ArtNetInputSettings.DmxManager == null)
                errors.Add($"No DmxManager component found on {ArtNetInputSettings.GameObject.name}");
        }

        public override void OnAfterDuplicate()
        {
            ArtNetInputSettings.DuplicateExposedReference();
        }
    }
}
