using System;
using UnityEngine;

namespace ArtNet
{
    internal partial class DmxData : MonoBehaviour
    {
        private int[] _dmxValues = new int[512];

        public int this[int index]
        {
            get => _dmxValues[index];
            set
            {
                var newValue = value;
                newValue = Math.Clamp(newValue, 0, 255);
                _dmxValues[index] = newValue;
                if (index is >= 1 and <= 512)
                {
                   GetType().GetField($"Ch{index:D3}").SetValue(this, newValue);
                }
            }
        }

        public int[] DmxValues
        {
            get => _dmxValues;
            set => _dmxValues = value;
        }
    }
}
