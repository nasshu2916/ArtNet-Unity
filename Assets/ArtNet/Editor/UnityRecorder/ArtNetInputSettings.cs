using System;
using System.ComponentModel;

namespace UnityEditor.Recorder.Input
{
    [DisplayName("ArtNet")]
    [Serializable]
    public class ArtNetInputSettings : RecorderInputSettings
    {
        protected override Type InputType => typeof(RecorderInput);
    }
}
