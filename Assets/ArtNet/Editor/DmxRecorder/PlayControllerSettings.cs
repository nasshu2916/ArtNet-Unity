using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    public class PlayControllerSettings : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private List<SendElement> _sendElements = new();
        [SerializeField] private bool _isLoop;
        [SerializeField] private float _speed = 1;

        public bool IsLoop { get => _isLoop; set => _isLoop = value; }
        public float Speed { get => _speed; set => _speed = value; }

        public IEnumerable<EndPoint> SendEndPoints()
        {
            return _sendElements.Where(e => e.IsEnabled).Select(e => e.EndPoint);
        }

        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize() { }
    }
}
