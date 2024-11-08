using System.Linq;
using ArtNet.UnityRecorder.Input;
using UnityEditor.Recorder;
using UnityEngine;

namespace ArtNet.UnityRecorder
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
            }

            base.EndRecording(session);
        }
    }
}
