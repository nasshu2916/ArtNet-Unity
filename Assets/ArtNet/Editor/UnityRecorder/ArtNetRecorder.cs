using UnityEditor.Recorder;

namespace ArtNet.UnityRecorder
{
    public class ArtNetRecorder : GenericRecorder<ArtNetRecorderSettings>
    {
        protected override void RecordFrame(RecordingSession ctx)
        {
        }

        protected override void EndRecording(RecordingSession session)
        {
            var settings = (ArtNetRecorderSettings) session.settings;


            base.EndRecording(session);
        }
    }
}
