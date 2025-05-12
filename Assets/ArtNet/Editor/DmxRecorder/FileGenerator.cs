using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    internal class Wildcard
    {
        private readonly Func<string> _resolver;

        public string Pattern { get; }
        public string Label { get; }

        internal Wildcard(string pattern, Func<string> resolver)
        {
            Pattern = pattern;
            Label = Pattern;
            _resolver = resolver;
        }

        internal string Resolve()
        {
            return _resolver == null ? string.Empty : _resolver();
        }
    }

    public static class DefaultWildcard
    {
        /// <summary>
        /// The Recorder name.
        /// </summary>
        public static readonly string Recorder = GeneratePattern("Recorder");

        /// <summary>
        /// The date when the recording session started (in the yyyy-MM-dd format).
        /// </summary>
        public static readonly string Date = GeneratePattern("Date");

        /// <summary>
        /// The time the recording session started (in the 00h00m format).
        /// </summary>
        public static readonly string Time = GeneratePattern("Time");

        /// <summary>
        /// The take number (which is incremented every time a new session is started).
        /// </summary>
        public static readonly string Take = GeneratePattern("Take");

        /// <summary>
        /// The file extension of the output format.
        /// </summary>
        public static readonly string Extension = GeneratePattern("Extension");

        private static string GeneratePattern(string tag)
        {
            return "<" + tag + ">";
        }
    }

    /// <summary>
    /// A class that provides a way to generate names of output files, with support for wildcards.
    /// </summary>
    [Serializable]
    public class FileGenerator
    {
        [SerializeField] private string _directory = "Recordings";
        [SerializeField] private string _fileName = $"{DefaultWildcard.Recorder}_{DefaultWildcard.Take}";

        private readonly List<Wildcard> _wildcards;
        internal IEnumerable<Wildcard> Wildcards => _wildcards;


        public string Directory
        {
            get => _directory;
            set => _directory = value;
        }

        public string FileName
        {
            get => _fileName;
            set => _fileName = value;
        }

        internal RecorderSettings RecorderSettings { get; private set; }

        internal FileGenerator(RecorderSettings recorderSettings)
        {
            RecorderSettings = recorderSettings;
            _wildcards = new List<Wildcard>
            {
                new(DefaultWildcard.Recorder, RecorderResolver),
                new(DefaultWildcard.Date, DateResolver),
                new(DefaultWildcard.Time, TimeResolver),
                new(DefaultWildcard.Take, TakeResolver),
                new(DefaultWildcard.Extension, ExtensionResolver)
            };
        }

        private string RecorderResolver()
        {
            return SanitizeInvalidName(RecorderSettings.name);
        }

        private static string DateResolver()
        {
            var date = DateTime.Now;
            return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private static string TimeResolver()
        {
            var date = DateTime.Now;
            return $"{date:HH}h{date:mm}m";
        }

        private string TakeResolver()
        {
            return RecorderSettings.Take.ToString("000");
        }

        private string ExtensionResolver()
        {
            return RecorderSettings.Extension;
        }

        public string AbsolutePath()
        {
            return OutputDirectory() + OutputFileName();
        }

        public string AssetsRelativePath()
        {
            var path = OutputDirectoryPath();
            return "Assets/" + path + OutputFileName();
        }

        public string OutputDirectory()
        {
            var path = OutputDirectoryPath();
            return Application.dataPath + Path.DirectorySeparatorChar + path;
        }

        private string OutputDirectoryPath()
        {
            var path = ApplyWildcards(Directory);
            if (!string.IsNullOrEmpty(path) && !path.EndsWith("/"))
                path += "/";
            return path;
        }

        public string OutputFileName()
        {
            return ApplyWildcards(SanitizeFileName(FileName)) + "." + ExtensionResolver();
        }

        public void CreateDirectory()
        {
            var path = OutputDirectory();
            if (!string.IsNullOrEmpty(path) && !System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Replaces any invalid path character by "_".
        /// </summary>
        internal static string SanitizeInvalidName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return invalidChars.Aggregate(fileName, (current, c) => current.Replace(c, '_'));
        }

        /// <summary>
        /// Replaces any occurrence of "/" or "\" in file name with "_".
        /// </summary>
        internal static string SanitizeFileName(string fileName)
        {
            return Regex.Replace(fileName, @"[\\|/]", "_");
        }

        private string ApplyWildcards(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            foreach (var w in Wildcards)
            {
                str = str.Replace(w.Pattern, w.Resolve());
            }

            return str;
        }
    }
}
