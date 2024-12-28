using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ArtNet
{
    public partial class DmxData : MonoBehaviour
    {
        private readonly byte[] _dmxValues = new byte[512];
#if UNITY_EDITOR
        private readonly Dictionary<int, FieldInfo> _fieldCache = new();
#endif

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
#if UNITY_EDITOR
                    var field = GetField(index);
                    if (field != null)
                    {
                        field.SetValue(this, value);
                    }
#endif
                }
                else
                {
                    throw new IndexOutOfRangeException("DMX channel must be between 0 and 511");
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            for (var i = 0; i < 512; i++)
            {
                var fieldName = $"Ch{(i + 1):D3}";
                var field = GetField(i);

                if (field != null)
                {
                    var fieldValue = (int) field.GetValue(this);
                    if (fieldValue == _dmxValues[i]) continue;
                    _dmxValues[i] = (byte) fieldValue;
                }
                else
                {
                    Debug.LogWarning($"Field {fieldName} not found");
                }
            }
        }

        private FieldInfo GetField(int index)
        {
            var fieldName = $"Ch{(index + 1):D3}";
            if (_fieldCache.TryGetValue(index, out var field))
            {
                return field;
            }

            field = GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            _fieldCache[index] = field;
            return field;
        }
#endif
    }
}
