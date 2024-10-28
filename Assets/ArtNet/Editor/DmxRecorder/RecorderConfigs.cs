using System.Collections.Generic;

namespace ArtNet.Editor.DmxRecorder
{
    public enum RecodeFormat
    {
        Binary = 0,
        AnimationClip = 1,
    }

    public class RecorderConfigs
    {
        public RecodeFormat RecordFormat { get; set; }

        public BinaryRecordConfig BinaryConfig { get; }
        public AnimationClipRecordConfig AnimationClipConfig { get; }

        public RecorderConfigs(RecodeFormat format, BinaryRecordConfig binaryConfig, AnimationClipRecordConfig
                animationClipConfig)
        {
            RecordFormat = format;
            BinaryConfig = binaryConfig;
            AnimationClipConfig = animationClipConfig;
        }

        private IRecordConfig Config => RecordFormat switch
        {
            RecodeFormat.Binary => BinaryConfig,
            RecodeFormat.AnimationClip => AnimationClipConfig,
            _ => throw new System.NotImplementedException()
        };

        public bool Validate() => ValidateErrors().Count == 0;
        public List<string> ValidateErrors() => Config.ValidateErrors();
    }

    public class BinaryRecordConfig : IRecordConfig
    {
        private const string Extension = ".dmx";

        public string Directory { get; set; }
        public string FileName { get; set; }

        public string OutputPath => $"{Directory}/{FileName}{Extension}";

        public List<string> ValidateErrors()
        {
            var errors = new List<string>();
            if (!ValidateDirectory()) errors.Add("Directory is not set");
            if (!ValidateFileName()) errors.Add("FileName is not set");

            return errors;
        }

        private bool ValidateDirectory() => !string.IsNullOrEmpty(Directory);
        private bool ValidateFileName() => !string.IsNullOrEmpty(FileName);
    }

    public class AnimationClipRecordConfig : IRecordConfig
    {
        public string OutputAnimationClipAssetPath { get; set; } = "Assets/Recording";

        public List<string> ValidateErrors()
        {
            return new List<string>();
        }
    }

    public interface IRecordConfig
    {
        public List<string> ValidateErrors();
    }
}
