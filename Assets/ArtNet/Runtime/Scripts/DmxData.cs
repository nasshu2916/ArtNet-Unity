using System;
using UnityEngine;

namespace ArtNet
{
    public partial class DmxData : MonoBehaviour
    {
        private byte[] _dmxValues = new byte[512];

        public byte this[int index]
        {
            get
            {
                if (index is >= 0 and < 512)
                {
                    return _dmxValues[index];
                }
                return 0;
            }
            set
            {
                if (index is >= 0 and < 512)
                {
                    _dmxValues[index] = value;
                    GetType().GetField($"Ch{(index + 1):D3}").SetValue(this, value);
                }
                else
                {
                    throw new IndexOutOfRangeException("DMX channel must be between 0 and 511");
                }
            }
        }

        public byte[] DmxValues
        {
            get => _dmxValues;
            set => _dmxValues = value;
        }
    }
}
