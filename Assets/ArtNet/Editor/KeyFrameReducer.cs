using System;
using System.Collections.Generic;

namespace ArtNet.Editor
{
    public static class KeyFrameReducer
    {
        public static List<KeyFrameData> Reduce(List<KeyFrameData> keyFrameData)
        {
            if (keyFrameData.Count <= 2) return keyFrameData;

            var newDmxFrameData = new List<KeyFrameData> { keyFrameData[0] };
            var latest = keyFrameData[0];

            for (var i = 1; i < keyFrameData.Count - 1; i++)
            {
                var current = keyFrameData[i];
                var next = keyFrameData[i + 1];
                if (IsOmittedFrame(latest, current, next)) continue;

                newDmxFrameData.Add(current);
                latest = current;
            }

            newDmxFrameData.Add(keyFrameData[^1]);
            return newDmxFrameData;
        }

        private static bool IsOmittedFrame(
            KeyFrameData prev,
            KeyFrameData current,
            KeyFrameData next,
            float tolerance = 0.01f)
        {
            var prevDiffValue = current.Value - prev.Value;
            var prevDiffTime = current.Time - prev.Time;
            var nextDiffValue = next.Value - current.Value;
            var nextDiffTime = next.Time - current.Time;

            return Math.Abs(prevDiffValue / prevDiffTime - nextDiffValue / nextDiffTime) <= tolerance;
        }
    }
}
