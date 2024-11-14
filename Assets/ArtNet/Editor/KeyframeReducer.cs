using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArtNet.Editor
{
    public static class KeyframeReducer
    {
        public static IEnumerable<Keyframe> Reduce(List<Keyframe> keys, float errorThreshold)
        {
            if (keys.Count <= 2) return keys;

            var keep = new bool[keys.Count];
            keep[0] = true;
            keep[keys.Count - 1] = true;

            RDP(keys, 0, keys.Count - 1, errorThreshold, keep);
            return keys.Where((t, i) => keep[i]);
        }

        private static void RDP(List<Keyframe> keys, int startIndex, int endIndex, float threshold, bool[] keep)
        {
            var maxDistance = 0f;
            var index = startIndex;

            for (var i = startIndex + 1; i < endIndex; i++)
            {
                var distance = PointDistance(keys[i], keys[startIndex], keys[endIndex]);
                if (distance <= maxDistance) continue;

                index = i;
                maxDistance = distance;
            }

            if (maxDistance <= threshold) return;

            keep[index] = true;
            RDP(keys, startIndex, index, threshold, keep);
            RDP(keys, index, endIndex, threshold, keep);
        }

        private static float PointDistance(Keyframe point, Keyframe startPoint, Keyframe endPoint)
        {
            var dx = endPoint.time - startPoint.time;
            var dy = endPoint.value - startPoint.value;

            var magnitude = dx * dx + dy * dy;
            if (magnitude > 0.0001f)
            {
                magnitude = Mathf.Sqrt(magnitude);
                dx /= magnitude;
                dy /= magnitude;
            }

            var pvx = point.time - startPoint.time;
            var pvy = point.value - startPoint.value;

            var dot = dx * pvx + dy * pvy;
            var ax = pvx - dot * dx;
            var ay = pvy - dot * dy;

            return ax * ax + ay * ay;
        }
    }
}
