using System.Collections.Generic;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    public class PlayControllerSetting : ControllerSettingBase
    {
        [SerializeField, NotNull, ItemNotNull] private List<SendDestination> _sendDestinations = new();
        [SerializeField] private bool _isLoop;
        [SerializeField] private float _speed = 1;

        [NotNull, ItemNotNull] public List<SendDestination> SendDestinations => _sendDestinations;
        public bool IsLoop { get => _isLoop; set => _isLoop = value; }
        public float Speed { get => _speed; set => _speed = value; }

        [NotNull, ItemNotNull]
        public IEnumerable<EndPoint> SendEndPoints()
        {
            return SendDestinations.Where(e => e.IsEnabled).Select(e => e.EndPoint);
        }

        public static PlayControllerSetting GetOrNewGlobalSetting()
        {
            return GetOrNewGlobalSetting<PlayControllerSetting>("DmxPlayerSettings");
        }

        protected override Object[] SaveObjects()
        {
            var sendElementsCopy = SendDestinations.ToArray();
            var objs = new Object[sendElementsCopy.Length + 1];
            objs[0] = this;

            for (var i = 0; i < sendElementsCopy.Length; ++i)
                objs[i + 1] = sendElementsCopy[i];
            return objs;
        }

        public void AddSendDestination([NotNull] SendDestination sendElement)
        {
            if (SendDestinations.Contains(sendElement)) return;

            EditorUtility.SetDirty(this);
            Undo.RegisterCompleteObjectUndo(this, "Add Send Destination");
            SendDestinations.Add(sendElement);

            Save();
        }

        public void RemoveSendDestination([NotNull] SendDestination sendElement)
        {
            if (!SendDestinations.Contains(sendElement)) return;

            EditorUtility.SetDirty(this);
            Undo.RegisterCompleteObjectUndo(this, "Remove Send Destination");
            SendDestinations.Remove(sendElement);

            Save();
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
