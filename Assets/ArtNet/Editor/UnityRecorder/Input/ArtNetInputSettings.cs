using System;
using System.ComponentModel;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEngine;

namespace ArtNet.Editor.UnityRecorder.Input
{
    [DisplayName("ArtNet")]
    [Serializable]
    public class ArtNetInputSettings : RecorderInputSettings
    {
        protected override Type InputType => typeof(ArtNetInput);

        [SerializeField] private string _bindingID;

        public GameObject GameObject
        {
            get
            {
                if (string.IsNullOrEmpty(_bindingID)) return null;

                return BindingManager.Get(_bindingID) as GameObject;
            }
            set
            {
                if (string.IsNullOrEmpty(_bindingID))
                    _bindingID = GenerateBindingId();

                BindingManager.Set(_bindingID, value);
            }
        }

        public DmxManager DmxManager
        {
            get
            {
                var gameObject = GameObject;
                return gameObject?.GetComponent<DmxManager>();
            }
        }

        public void DuplicateExposedReference()
        {
            if (string.IsNullOrEmpty(_bindingID)) return;

            var src = _bindingID;
            var dst = GenerateBindingId();
            _bindingID = dst;

            BindingManager.Duplicate(src, dst);
        }

        private static string GenerateBindingId()
        {
            return GUID.Generate().ToString();
        }
    }
}
