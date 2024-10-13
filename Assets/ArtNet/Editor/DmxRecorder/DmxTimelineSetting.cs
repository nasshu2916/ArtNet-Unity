using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    public class DmxTimelineSetting : ScriptableObject
    {
        [SerializeField] public List<DmxTimeline> DmxTimelines;
    }

    [Serializable]
    public class DmxTimeline
    {
        public AnimationClip DmxTimelineClip;
        [Range(1, 256)] public int Universe;
    }
}
