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

            RDP(keys, 0, keys.Count - 1, errorThreshold * errorThreshold, keep);
            return keys.Where((t, i) => keep[i]);
        }

        private static void RDP(List<Keyframe> keys, int startIndex, int endIndex, float threshold, bool[] keep)
        {
            var maxDistance = 0f;
            var index = startIndex;

            for (var i = startIndex + 1; i < endIndex; i++)
            {
                var distance = PerpendicularDistanceSquared(keys[i], keys[startIndex], keys[endIndex]);
                if (distance <= maxDistance) continue;

                index = i;
                maxDistance = distance;
            }

            if (maxDistance <= threshold) return;

            keep[index] = true;
            RDP(keys, startIndex, index, threshold, keep);
            RDP(keys, index, endIndex, threshold, keep);
        }

        /// <summary>
        /// 垂線距離の2乗を計算
        /// </summary>
        private static float PerpendicularDistanceSquared(Keyframe point, Keyframe startPoint, Keyframe endPoint)
        {
            var dx = endPoint.time - startPoint.time;
            var dy = endPoint.value - startPoint.value;

            var denominator = dx * dx + dy * dy;

            if (denominator < 1e-6f)
            {
                var psx = point.time - startPoint.time;
                var psy = point.value - startPoint.value;
                return psx * psx + psy * psy;
            }

            var numerator = dy * point.time - dx * point.value + endPoint.time * startPoint.value - endPoint.value * startPoint.time;
            return (numerator * numerator) / denominator;
        }
    }
}
