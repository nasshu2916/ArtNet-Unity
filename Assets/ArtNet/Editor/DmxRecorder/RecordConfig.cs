using System.Collections.Generic;

namespace ArtNet.Editor.DmxRecorder
{
    public class RecordConfig
    {
        private const string Extension = ".dmx";

        public string Directory;
        public string FileName;

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
