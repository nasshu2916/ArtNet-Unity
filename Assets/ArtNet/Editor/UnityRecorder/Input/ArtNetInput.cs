using System.Collections.Generic;
using ArtNet.Editor.DmxRecorder;
using UnityEditor.Recorder;
using UnityEngine;

namespace ArtNet.Editor.UnityRecorder.Input
{
    public class ArtNetInput : RecorderInput
    {
        internal class DmxRecorder
        {
            private readonly DmxManager _dmxManager;
            internal List<UniverseData> Frames { get; } = new();

            public DmxRecorder(DmxManager dmxManager)
            {
                _dmxManager = dmxManager;
            }

            public void RecordFrame(double time)
            {
                var universes = _dmxManager.Universes();
                foreach (var universe in universes)
                {
                    var values = _dmxManager.DmxValues(universe);
                    Frames.Add(new UniverseData(time, universe, values));
                }
            }
        }

        internal DmxRecorder Recorder;
        private double _startTime;

        protected override void BeginRecording(RecordingSession session)
        {
            var artNetSettings = (ArtNetInputSettings) settings;
            var dmxManager = artNetSettings.DmxManager;
            if (dmxManager == null) return;

            Recorder = new DmxRecorder(dmxManager);
            _startTime = Time.time;
        }


        protected override void NewFrameReady(RecordingSession session)
        {
            Recorder?.RecordFrame(Time.time - _startTime);
        }
    }
}
