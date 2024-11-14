using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArtNet.Editor
{
    public class KeyframeReducer
    {
        private readonly float _threshold;

        public KeyframeReducer(float errorThreshold)
        {
            _threshold = errorThreshold * errorThreshold;
        }

        public List<Keyframe> Reduce(List<Keyframe> keys)
        {
            if (keys.Count <= 2) return keys;

            return Rdm(keys, 0, keys.Count - 1);
        }

        private List<Keyframe> Rdm(List<Keyframe> keys, int startIndex, int endIndex)
        {
            var maxDistance = 0f;
            var index = startIndex;

            // 最大距離点を探索
            for (var i = startIndex + 1; i < endIndex; i++)
            {
                var distance = PerpendicularDistanceSquared(keys[i], keys[startIndex], keys[endIndex]);
                if (distance <= maxDistance) continue;

                index = i;
                maxDistance = distance;
            }

            // 最大距離が閾値未満なら直線を返す
            if (maxDistance < _threshold)
            {
                return new List<Keyframe> { keys[startIndex], keys[endIndex] };
            }

            var result1 = Rdm(keys, startIndex, index);
            var result2 = Rdm(keys, index, endIndex);

            // 重複を除く
            result1.RemoveAt(result1.Count - 1);
            result1.AddRange(result2);

            return result1;
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
