using System.Collections.Generic;
using System.Linq;
using ArtNet.Editor.DmxRecorder;
using ArtNet.Editor.UnityRecorder.Input;
using UnityEditor;
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

        private static List<UniverseData> FilterFrames(List<UniverseData> frames, UniverseFilter filter)
        {
            return frames.Where(f => filter.IsMatch((int) f.Universe)).ToList();
        }

        private static void BinaryWrite(List<UniverseData> frames, string absolutePath)
        {
            var binary = RecordData.SerializeUniverseData(frames);
            System.IO.File.WriteAllBytes(absolutePath, binary);
        }

        private static void AnimationClipWrite(List<UniverseData> frames, string absolutePath)
        {
            var clip = new AnimationClip();
            var clipName = absolutePath.Replace(FileNameGenerator.SanitizePath(Application.dataPath), "Assets");

            AssetDatabase.CreateAsset(clip, clipName);
            var timelineConverter = new TimelineConverter(frames);
            timelineConverter.SaveToClip(clip);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
