using ArtNet.Editor.DmxRecorder.Util;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    public class BinaryRecorderSettings : RecorderSettings
    {
        protected internal override string Extension => "dmx";
        internal override string DefaultName => "Binary";
        protected internal override Texture Icon => IconHelper.Icon("DefaultAsset Icon", true);
    }
}
