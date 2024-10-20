using System;
using System.Collections.Generic;
using System.Linq;
using ArtNet.Packets;
using UnityEditor;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    public class TimelineConverter
    {
        public List<TimelineUniverse> Timelines { get; } = new();

        public TimelineConverter(IReadOnlyCollection<(int time, DmxPacket packet)> packets)
        {
            var groupedUniversePackets = packets.GroupBy(x => x.packet.Universe);

            foreach (var group in groupedUniversePackets)
            {
                Timelines.Add(new TimelineUniverse(group.Key, group.ToList()));
            }
        }

        public TimelineConverter(DmxTimelineSetting dmxTimelineSetting)
        {
            foreach (var timelineElement in dmxTimelineSetting.DmxTimelines)
            {
                Timelines.Add(new TimelineUniverse(timelineElement.Universe, timelineElement.DmxTimelineClip));
            }
        }

        public void SaveDmxTimelineClips(string directory)
        {
            if (System.IO.Directory.Exists(directory) == false)
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            var dmxTimelines = new List<DmxTimeline>(Timelines.Count);
            foreach (var timelineUniverse in Timelines)
            {
                var universe = timelineUniverse.Universe;
                timelineUniverse.ThinOutUnchangedFrames();
                var clip = timelineUniverse.ToAnimationClip();
                SaveAsset(clip, directory, $"Universe{universe}.anim");

                var timelineElement = new DmxTimeline
                {
                    DmxTimelineClip = clip,
                    Universe = universe
                };
                dmxTimelines.Add(timelineElement);
            }

            var dmxTimelineAsset = ScriptableObject.CreateInstance<DmxTimelineSetting>();
            dmxTimelineAsset.DmxTimelines = dmxTimelines;
            SaveAsset(dmxTimelineAsset, directory, "DmxTimeline.asset");

            AssetDatabase.Refresh();
        }

        public List<(int time, DmxPacket packet)> ToDmxPackets()
        {
            return Timelines.SelectMany(x => x.ToDmxPackets()).OrderBy(x => x.time).ToList();
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

        public TimelineUniverse(int universe, IReadOnlyCollection<(int time, DmxPacket packet)> packets)
        {
            Universe = universe;
            ChannelDmxFrameData = new List<DmxFrameData>[512];

            for (var i = 0; i < ChannelDmxFrameData.Length; i++)
            {
                ChannelDmxFrameData[i] = packets.Where(x => x.packet.Dmx.Length > i)
                    .Select(x => new DmxFrameData(x.time, x.packet.Dmx[i]))
                    .OrderBy(x => x.Millisecond).ToList();
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

        public IEnumerable<int> AllFrameTimes()
        {
            return ChannelDmxFrameData.SelectMany(x => x.Select(frameData => frameData.Millisecond)).Distinct();
        }

        public byte FrameValue(int channel, int time)
        {
            var dmxFrameData = ChannelDmxFrameData[channel];

            // If there is a frame data at the exact time, return it
            foreach (var frameData in dmxFrameData.Where(frameData => frameData.Millisecond == time))
            {
                return frameData.Value;
            }

            // if there is no frame data, return 0
            if (dmxFrameData.Count == 0) return 0;

            // if time is out of range, return the first or last value
            if (time < dmxFrameData[0].Millisecond) return dmxFrameData[0].Value;
            if (time > dmxFrameData[^1].Millisecond) return dmxFrameData[^1].Value;

            // return the estimated value from frames around the specified time.

            // Find the frame data before and after the specified time
            var prev = dmxFrameData[0];
            var next = dmxFrameData[0];
            foreach (var frameData in dmxFrameData)
            {
                if (frameData.Millisecond > time)
                {
                    next = frameData;
                    break;
                }

                prev = frameData;
            }

            // Calculate the estimated value
            var prevDiff = next.Value - prev.Value;
            var prevDiffTime = next.Millisecond - prev.Millisecond;
            var timeDiff = time - prev.Millisecond;
            return (byte) (prev.Value + (prevDiff * timeDiff / prevDiffTime));
        }

        public AnimationClip ToAnimationClip()
        {
            var curves = ConvertAnimationCurves();
            var clip = new AnimationClip
            {
                name = $"Universe{Universe}"
            };
            for (var i = 0; i < curves.Length; i++)
            {
                if (curves[i].keys.Length == 0) continue;
                clip.SetCurve("", typeof(DmxData), $"Ch{i + 1:D3}", curves[i]);
            }

            return clip;
        }

        private AnimationCurve[] ConvertAnimationCurves()
        {
            var curves = new AnimationCurve[ChannelDmxFrameData.Length];
            for (var i = 0; i < ChannelDmxFrameData.Length; i++)
            {
                var keyframes = ChannelDmxFrameData[i]
                    .Select(data => new Keyframe(data.Millisecond / 1000f, data.Value)).ToArray();
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
            var prevDiffTime = current.Millisecond - prev.Millisecond;
            var nextDiffTime = next.Millisecond - current.Millisecond;

            return Math.Abs((float) prevDiff / prevDiffTime - (float) nextDiff / nextDiffTime) <= tolerance;
        }

        public IEnumerable<(int time, DmxPacket packet)> ToDmxPackets()
        {
            byte sequence = 0;
            var packets = new List<(int time, DmxPacket packet)>();
            var allFrameTimes = AllFrameTimes().OrderBy(x => x).ToList();

            foreach (var time in allFrameTimes)
            {
                var dmx = new byte[512];
                for (var i = 0; i < ChannelDmxFrameData.Length; i++)
                {
                    dmx[i] = FrameValue(i, time);
                }

                var packet = new DmxPacket { Sequence = sequence, Universe = (ushort) Universe, Dmx = dmx };
                packets.Add((time, packet));
                if (sequence >= 255)
                {
                    sequence = 0;
                }
                else
                {
                    sequence++;
                }
            }

            return packets;
        }
    }

    public struct DmxFrameData
    {
        public int Millisecond { get; }
        public byte Value { get; }

        public DmxFrameData(int millisecond, byte value)
        {
            Millisecond = millisecond;
            Value = value;
        }
    }
}
