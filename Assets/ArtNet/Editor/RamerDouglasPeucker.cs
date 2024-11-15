using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArtNet.Editor
{
    public class RamerDouglasPeucker
    {
        private readonly float _threshold;

        public RamerDouglasPeucker(float errorThreshold)
        {
            _threshold = errorThreshold * errorThreshold;
        }

        public List<Keyframe> Reduce(ReadOnlySpan<Keyframe> keys)
        {
            return Execute(keys, 0, keys.Length - 1);
        }

        private List<Keyframe> Execute(ReadOnlySpan<Keyframe> keys, int startIndex, int endIndex)
        {
            if (endIndex - startIndex < 2)
            {
                return new List<Keyframe> { keys[startIndex], keys[endIndex] };
            }

            // 最大距離のKeyframeを探索
            var maxDistance = 0f;
            var maxIndex = startIndex;
            for (var i = startIndex + 1; i < endIndex; i++)
            {
                var distance = PerpendicularDistanceSquared(keys[i], keys[startIndex], keys[endIndex]);
                if (distance <= maxDistance) continue;

                maxIndex = i;
                maxDistance = distance;
            }

            // 最大距離が閾値未満なら直線を返す
            if (maxDistance < _threshold)
            {
                return new List<Keyframe> { keys[startIndex], keys[endIndex] };
            }

            // 最大距離の点で再帰的に処理
            var result1 = Execute(keys, startIndex, maxIndex);
            var result2 = Execute(keys, maxIndex, endIndex);

            // 重複を取り除いて結合
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
