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
                if (latest.Value == current.Value) continue;

                newDmxFrameData.Add(current);
                latest = current;
            }

            newDmxFrameData.Add(keyFrameData[^1]);
            return newDmxFrameData;
        }
    }
}
