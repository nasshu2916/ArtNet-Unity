using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder.IO
{
    public class TimelineConverter
    {
        public List<TimelineUniverse> Timelines { get; } = new();

        public TimelineConverter(IEnumerable<UniverseData> universeData)
        {
            var groupedUniverseData = universeData.GroupBy(x => x.Universe);

            foreach (var group in groupedUniverseData)
            {
                Timelines.Add(new TimelineUniverse((int) group.Key, group.ToList()));
            }
        }

        public TimelineConverter(AnimationClip clip)
        {
            var curveBindings = AnimationUtility.GetCurveBindings(clip);
            var universePaths = curveBindings.Select(x => x.path).Distinct();
            var universeRegex = new System.Text.RegularExpressions.Regex(@"Universe(\d+)");
            foreach (var universePath in universePaths)
            {
                var match = universeRegex.Match(universePath);
                if (match.Success == false) continue;

                var universe = int.Parse(match.Groups[1].Value);
                Timelines.Add(new TimelineUniverse(universe, clip));
            }
        }

        public void SaveToClip(AnimationClip clip)
        {
            foreach (var timelineUniverse in Timelines)
            {
                var universe = timelineUniverse.Universe;
                timelineUniverse.ThinOutUnchangedFrames();
                var curves = timelineUniverse.AnimationCurves();
                for (var i = 0; i < curves.Length; i++)
                {
                    var curve = curves[i];
                    if (curve.keys.Length == 0) continue;

                    clip.SetCurve($"Universe{universe}", typeof(DmxData), $"Ch{i + 1:D3}", curve);
                }
            }
        }

        public List<UniverseData> ToUniverseData()
        {
            return Timelines.SelectMany(x => x.ToUniverseData()).OrderBy(x => x.Time).ToList();
        }
    }

    public class TimelineUniverse
    {
        public int Universe { get; }
        private List<KeyFrameData>[] ChannelDmxFrameData { get; }

        public TimelineUniverse(int groupKey, IReadOnlyCollection<UniverseData> universeData)
        {
            Universe = groupKey;
            ChannelDmxFrameData = new List<KeyFrameData>[512];

            for (var i = 0; i < ChannelDmxFrameData.Length; i++)
            {
                ChannelDmxFrameData[i] = universeData.Where(x => x.Values.Length > i)
                    .Select(x => new KeyFrameData(x.Time, x.Values[i]))
                    .OrderBy(x => x.Time).ToList();
            }
        }

        public TimelineUniverse(int universe, AnimationClip clip)
        {
            Universe = universe;
            var curveBindings = AnimationUtility.GetCurveBindings(clip);
            ChannelDmxFrameData = new List<KeyFrameData>[512];
            for (var i = 0; i < ChannelDmxFrameData.Length; i++)
            {
                var propertyName = $"Ch{i + 1:D3}";

                var curve = curveBindings
                    .Where(binding => binding.propertyName == propertyName)
                    .Select(binding => AnimationUtility.GetEditorCurve(clip, binding))
                    .FirstOrDefault();

                if (curve is null) continue;

                ChannelDmxFrameData[i] = curve.keys.Select(x => new KeyFrameData(x.time, (byte) x.value))
                    .ToList();
            }
        }

        public IEnumerable<float> AllFrameTimes()
        {
            return ChannelDmxFrameData.SelectMany(x => x.Select(frameData => frameData.Time)).Distinct();
        }

        public byte FrameValue(int channel, float time)
        {
            var dmxFrameData = ChannelDmxFrameData[channel];

            // If there is a frame data at the exact time, return it
            foreach (var frameData in dmxFrameData.Where(frameData => Mathf.Approximately(frameData.Time, time)))
            {
                return frameData.Value;
            }

            // if there is no frame data, return 0
            if (dmxFrameData.Count == 0) return 0;

            // if time is out of range, return the first or last value
            if (time < dmxFrameData[0].Time) return dmxFrameData[0].Value;
            if (time > dmxFrameData[^1].Time) return dmxFrameData[^1].Value;

            // return the estimated value from frames around the specified time.

            // Find the frame data before and after the specified time
            var prev = dmxFrameData[0];
            var next = dmxFrameData[0];
            foreach (var frameData in dmxFrameData)
            {
                if (frameData.Time > time)
                {
                    next = frameData;
                    break;
                }

                prev = frameData;
            }

            // Calculate the estimated value
            var prevDiff = next.Value - prev.Value;
            var prevDiffTime = next.Time - prev.Time;
            var timeDiff = time - prev.Time;
            return (byte) (prev.Value + (prevDiff * timeDiff / prevDiffTime));
        }

        public AnimationCurve[] AnimationCurves()
        {
            var curves = new AnimationCurve[ChannelDmxFrameData.Length];
            for (var i = 0; i < ChannelDmxFrameData.Length; i++)
            {
                var keyframes = ChannelDmxFrameData[i]
                    .Select(data => new Keyframe(data.Time, data.Value)).ToArray();
                var curve = new AnimationCurve(keyframes);
                for (var j = 0; j < curve.keys.Length; j++)
                {
                    AnimationUtility.SetKeyLeftTangentMode(curve, j, AnimationUtility.TangentMode.Constant);
                    AnimationUtility.SetKeyRightTangentMode(curve, j, AnimationUtility.TangentMode.Constant);
                }

                curves[i] = curve;
            }

            return curves;
        }

        public void ThinOutUnchangedFrames()
        {
            for (var i = 0; i < ChannelDmxFrameData.Length; i++)
            {
                var dmxFrameData = ChannelDmxFrameData[i];
                ChannelDmxFrameData[i] = KeyFrameReducer.Reduce(dmxFrameData);
            }
        }

        public IEnumerable<UniverseData> ToUniverseData()
        {
            var universeData = new List<UniverseData>();
            var allFrameTimes = AllFrameTimes().OrderBy(x => x).ToList();

            foreach (var time in allFrameTimes)
            {
                var dmx = new byte[512];
                for (var i = 0; i < ChannelDmxFrameData.Length; i++)
                {
                    dmx[i] = FrameValue(i, time);
                }

                universeData.Add(new UniverseData((long) (time * 1000), (ushort) Universe, dmx));
            }

            return universeData;
        }
    }
}
