using System.Collections.Generic;
using System.Linq;
using ArtNet.Common;
using ArtNet.Editor.DmxRecorder;
using ArtNet.Editor.DmxRecorder.IO;
using ArtNet.Editor.UnityRecorder.Input;
using UnityEditor.Recorder;
using UnityEngine;

namespace ArtNet.Editor.UnityRecorder
{
    public class ArtNetRecorder : GenericRecorder<ArtNetRecorderSettings>
    {
        protected override void RecordFrame(RecordingSession session)
        {
        }

        protected override void EndRecording(RecordingSession session)
        {
            var settings = (ArtNetRecorderSettings) session.settings;

            foreach (var input in m_Inputs)
            {
                var artNetInput = (ArtNetInput) input;
                if (artNetInput.Recorder == null)
                    continue;

                var frames = artNetInput.Recorder.Frames;
                var groupedFrames = frames.GroupBy(f => f.Universe);
                foreach (var group in groupedFrames)
                {
                    ArtNetLogger.DevLogDebug($"Universe {group.Key}, Frames: {group.Count()}");
                }

                settings.FileNameGenerator.CreateDirectory(session);
                var absolutePath = settings.FileNameGenerator.BuildAbsolutePath(session);
                absolutePath = FileNameGenerator.SanitizePath(absolutePath);

                var filteredFrames = settings.UniverseFilter.Filter(frames, f => f.Universe);
                switch (settings.OutputFormat)
                {
                    case ArtNetRecorderSettings.ArtNetRecorderOutputFormat.Binary:
                        BinaryWrite(filteredFrames, absolutePath, settings.IsCompressBinary);
                        break;
                    case ArtNetRecorderSettings.ArtNetRecorderOutputFormat.AnimationClip:
                        AnimationClipWrite(filteredFrames, absolutePath);
                        break;
                    default:
                        throw new System.ArgumentOutOfRangeException();
                }

                base.EndRecording(session);
            }
        }

        private static void BinaryWrite(IEnumerable<UniverseData> frames, string absolutePath, bool isCompress)
        {
            BinaryDmx.Export(frames, absolutePath, isCompress);
        }

        private static void AnimationClipWrite(IEnumerable<UniverseData> frames, string absolutePath)
        {
            var assetsPath = absolutePath.Replace(FileNameGenerator.SanitizePath(Application.dataPath), "Assets");
            AnimationClipDmx.Export(frames, assetsPath);
        }
    }
}
