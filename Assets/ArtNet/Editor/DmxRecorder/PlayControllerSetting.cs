using System.Collections.Generic;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    public class PlayControllerSetting : ControllerSettingBase
    {
        [SerializeField, NotNull, ItemNotNull] private List<SendElement> _sendElements = new();
        [SerializeField] private bool _isLoop;
        [SerializeField] private float _speed = 1;

        public bool IsLoop { get => _isLoop; set => _isLoop = value; }
        public float Speed { get => _speed; set => _speed = value; }

        [NotNull, ItemNotNull]
        public IEnumerable<EndPoint> SendEndPoints()
        {
            return _sendElements.Where(e => e.IsEnabled).Select(e => e.EndPoint);
        }

        public static PlayControllerSetting GetOrNewGlobalSetting()
        {
            return GetOrNewGlobalSetting<PlayControllerSetting>("DmxPlayerSettings");
        }

        protected override Object[] SaveObjects()
        {
            var sendElementsCopy = _sendElements.ToArray();
            var objs = new Object[sendElementsCopy.Length + 1];
            objs[0] = this;

            for (var i = 0; i < sendElementsCopy.Length; ++i)
                objs[i + 1] = sendElementsCopy[i];
            return objs;
        }

        public int CalcDeltaTime(int deltaTime)
        {
            var addTime = deltaTime * Speed;
            var addTimeInt = (int) addTime;

            // float の端数をランダムで追加
            if (addTime - addTimeInt > new System.Random().NextDouble())
            {
                addTimeInt += 1;
            }

            return addTimeInt;
        }
    }
}
