using System.Collections.Generic;

namespace ArtNet.Editor.DmxRecorder
{
    public enum RecodeFormat
    {
        Binary = 0,
        AnimationClip = 1,
    }

    public class RecordConfig
    {
        private const string Extension = ".dmx";

        public string Directory;
        public string FileName;

        public RecodeFormat OutputFormat { get; set; } = RecodeFormat.Binary;

        public string OutputPath => $"{Directory}/{FileName}{Extension}";

        public bool Validate()
        {
            return ValidateDirectory() && ValidateFileName();
        }

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
}
