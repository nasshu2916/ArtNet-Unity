using System.Collections.Generic;
using ArtNet.Editor.DmxRecorder.IO;
using ArtNet.Editor.DmxRecorder.Util;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    public class AnimationRecorderSettings : RecorderSettings
    {
        protected internal override string Extension => "anim";
        internal override string DefaultName => "Animation";
        protected internal override Texture Icon => IconHelper.Icon("Animation Icon", true);

        public override void StoreUniverseData(IEnumerable<UniverseData> universeData)
        {
            FileGenerator.CreateDirectory();
            var path = OutputAssetPath;

            AnimationClipDmx.Export(universeData, path);
        }
    }
}
