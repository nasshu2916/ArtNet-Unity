using System.Collections.Generic;
using System.Linq;
using ArtNet.Packets;
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

        public AnimationClip ToAnimationClip()
        {
            var curves = ConvertAnimationCurves();
            var clip = new AnimationClip
            {
                name = $"Universe{Universe + 1}"
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
                var keyframes = ChannelDmxFrameData[i].Select(data => new Keyframe(data.Millisecond / 1000f, data.Value)).ToArray();
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
                var newDmxFrameData = new List<DmxFrameData> {dmxFrameData[0]};

                for (var j = 1; j < dmxFrameData.Count - 1; j++)
                {
                    var current = dmxFrameData[j];
                    var next = dmxFrameData[j + 1];
                    if (latest.Value == current.Value && current.Value == next.Value) continue;

                    latest = current;
                    newDmxFrameData.Add(dmxFrameData[j]);
                }

                newDmxFrameData.Add(dmxFrameData[^1]);
                ChannelDmxFrameData[i] = newDmxFrameData;
            }
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
