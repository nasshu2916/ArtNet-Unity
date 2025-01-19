using System.Collections.Generic;
using System.Linq;
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
                    Debug.Log($"Universe {group.Key}, Frames: {group.Count()}");
                }

                settings.FileNameGenerator.CreateDirectory(session);
                var absolutePath = settings.FileNameGenerator.BuildAbsolutePath(session);
                absolutePath = FileNameGenerator.SanitizePath(absolutePath);

                var filteredFrames = FilterFrames(frames, settings.UniverseFilter);
                switch (settings.OutputFormat)
                {
                    case ArtNetRecorderSettings.ArtNetRecorderOutputFormat.Binary:
                        BinaryWrite(filteredFrames, absolutePath);
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

        private static IEnumerable<UniverseData> FilterFrames(IEnumerable<UniverseData> frames, UniverseFilter filter)
        {
            if (filter.Enabled == false || filter.Invalid()) return frames;

            var filterUniverse = filter.FilterUniverse();
            return frames.Where(f => filterUniverse.Contains(f.Universe));
        }

        private static void BinaryWrite(IEnumerable<UniverseData> frames, string absolutePath)
        {
            BinaryDmx.Export(frames, absolutePath);
        }

        private static void AnimationClipWrite(IEnumerable<UniverseData> frames, string absolutePath)
        {
            var assetsPath = absolutePath.Replace(FileNameGenerator.SanitizePath(Application.dataPath), "Assets");
            AnimationClipDmx.Export(frames, assetsPath);
        }
    }
}
