using System.Linq;
using ArtNet.Editor.DmxRecorder;
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

                var binary = RecordData.SerializeUniverseData(frames);
                System.IO.File.WriteAllBytes(absolutePath, binary);
            }

            base.EndRecording(session);
        }
    }
}
