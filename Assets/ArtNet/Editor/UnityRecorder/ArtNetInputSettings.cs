using System;
using System.ComponentModel;
using UnityEditor.Recorder;

namespace ArtNet.UnityRecorder
{
    [DisplayName("ArtNet")]
    [Serializable]
    public class ArtNetInputSettings : RecorderInputSettings
    {
        protected override Type InputType => typeof(RecorderInput);
    }
}
