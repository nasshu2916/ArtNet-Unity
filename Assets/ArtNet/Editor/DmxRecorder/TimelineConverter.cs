using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
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

        public void SaveDmxTimelineClips(string directory)
        {
            if (System.IO.Directory.Exists(directory) == false)
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            var clip = new AnimationClip { name = "ArtNetDmx" };
            foreach (var timelineUniverse in Timelines)
            {
                var universe = timelineUniverse.Universe;
                timelineUniverse.ThinOutUnchangedFrames();
                var curves = timelineUniverse.AnimationCurves();
                for (var i = 0; i < curves.Length; i++)
                {
                    if (curves[i].keys.Length == 0) continue;
                    clip.SetCurve($"Universe{universe}", typeof(DmxData), $"Ch{i + 1:D3}", curves[i]);
                }
            }
            SaveAsset(clip, directory, "ArtNetDmx.anim");

            AssetDatabase.Refresh();
        }

        public List<UniverseData> ToUniverseData()
        {
            return Timelines.SelectMany(x => x.ToUniverseData()).OrderBy(x => x.Time).ToList();
        }

        private static void SaveAsset<T>(T asset, string directory, string fileName) where T : UnityEngine.Object
        {
            var path = $"{directory}/{fileName}";
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
        }
    }

    public class TimelineUniverse
    {
        public int Universe { get; }
        private List<DmxFrameData>[] ChannelDmxFrameData { get; }

        public TimelineUniverse(int groupKey, IReadOnlyCollection<UniverseData> universeData)
        {
            Universe = groupKey;
            ChannelDmxFrameData = new List<DmxFrameData>[512];

            for (var i = 0; i < ChannelDmxFrameData.Length; i++)
            {
                ChannelDmxFrameData[i] = universeData.Where(x => x.Values.Length > i)
                    .Select(x => new DmxFrameData((float) x.Time, x.Values[i]))
                    .OrderBy(x => x.Time).ToList();
            }
        }

        public TimelineUniverse(int universe, AnimationClip clip)
        {
            Universe = universe;
            var curveBindings = AnimationUtility.GetCurveBindings(clip);
            ChannelDmxFrameData = new List<DmxFrameData>[512];
            for (var i = 0; i < ChannelDmxFrameData.Length; i++)
            {
                var propertyName = $"Ch{i + 1:D3}";

                var curve = curveBindings
                    .Where(binding => binding.propertyName == propertyName)
                    .Select(binding => AnimationUtility.GetEditorCurve(clip, binding))
                    .FirstOrDefault();

                if (curve is null) continue;

                ChannelDmxFrameData[i] = curve.keys.Select(x => new DmxFrameData((int) (x.time * 1000), (byte) x.value)).ToList();
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
                curves[i] = new AnimationCurve(keyframes);
            }

            return curves;
        }

        public void ThinOutUnchangedFrames()
        {
            for (var i = 0; i < ChannelDmxFrameData.Length; i++)
            {
                var dmxFrameData = ChannelDmxFrameData[i];
                if (dmxFrameData.Count == 0) continue;

                var latest = dmxFrameData[0];
                var newDmxFrameData = new List<DmxFrameData> { dmxFrameData[0] };

                for (var j = 1; j < dmxFrameData.Count - 1; j++)
                {
                    var current = dmxFrameData[j];
                    var next = dmxFrameData[j + 1];
                    if (IsOmittedFrame(latest, current, next)) continue;

                    latest = current;
                    newDmxFrameData.Add(dmxFrameData[j]);
                }

                newDmxFrameData.Add(dmxFrameData[^1]);
                ChannelDmxFrameData[i] = newDmxFrameData;
            }
        }

        private static bool IsOmittedFrame(
            DmxFrameData prev,
            DmxFrameData current,
            DmxFrameData next,
            float tolerance = 0.01f)
        {
            var prevDiff = current.Value - prev.Value;
            var nextDiff = next.Value - current.Value;
            var prevDiffTime = current.Time - prev.Time;
            var nextDiffTime = next.Time - current.Time;

            return Math.Abs(prevDiff / prevDiffTime - nextDiff / nextDiffTime) <= tolerance;
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

                universeData.Add(new UniverseData(time, (uint) Universe, dmx));
            }

            return universeData;
        }
    }

    public struct DmxFrameData
    {
        public float Time { get; }
        public byte Value { get; }

        public DmxFrameData(float time, byte value)
        {
            Time = time;
            Value = value;
        }
    }
}
