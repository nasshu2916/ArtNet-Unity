using System.Collections.Generic;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace ArtNet.UnityRecorder
{
    [RecorderSettings(typeof(ArtNetRecorder), "Art-Net")]
    public class ArtNetRecorderSettings : RecorderSettings
    {
        [SerializeField] private ArtNetInputSettings _artNetInputSettings = new();

        protected override string Extension => "dmx";

        public override IEnumerable<RecorderInputSettings> InputsSettings
        {
            get { yield return _artNetInputSettings; }
        }
    }
}
