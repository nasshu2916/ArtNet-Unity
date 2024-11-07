using System.Collections.Generic;
using ArtNet.UnityRecorder.Input;
using UnityEditor.Recorder;
using UnityEngine;

namespace ArtNet.UnityRecorder
{
    [RecorderSettings(typeof(ArtNetRecorder), "Art-Net")]
    public class ArtNetRecorderSettings : RecorderSettings
    {
        [SerializeField] private ArtNetInputSettings _artNetInputSettings = new();

        protected override string Extension => "dmx";

        public ArtNetInputSettings ArtNetInputSettings => _artNetInputSettings;
        public override IEnumerable<RecorderInputSettings> InputsSettings
        {
            get { yield return _artNetInputSettings; }
        }

        public ArtNetRecorderSettings()
        {
            FileNameGenerator.AddWildcard(DefaultWildcard.GeneratePattern("GameObject"), GameObjectNameResolver);
            FileNameGenerator.AddWildcard(DefaultWildcard.GeneratePattern("GameObjectScene"), GameObjectSceneNameResolver);

            FileNameGenerator.ForceAssetsFolder = false;
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
