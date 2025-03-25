using System.Collections.Generic;
using ArtNet.Editor.DmxRecorder.IO;
using ArtNet.Editor.DmxRecorder.Util;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    public class BinaryRecorderSettings : RecorderSettings
    {
        protected internal override string Extension => "dmx";
        internal override string DefaultName => "Binary";
        protected internal override Texture Icon => IconHelper.Icon("DefaultAsset Icon", true);

        [SerializeField] private bool _isCompressBinary = true;

        public override void StoreUniverseData(IEnumerable<UniverseData> universeData)
        {
            FileGenerator.CreateDirectory();
            var path = OutputAbsolutePath;

            BinaryDmx.Export(universeData, path, _isCompressBinary);
        }
    }
}
