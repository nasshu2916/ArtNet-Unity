using System.Linq;
using ArtNet.Samples.Devices;
using UnityEngine;

namespace ArtNet.Samples
{
    public class DmxManager : MonoBehaviour
    {
        private IDmxDevice[] _dmxDevices;
        [SerializeField]
        private ArtNetReceiver _artNetReceiver;

        private void Start()
        {
            _dmxDevices = FindObjectsOfType<GameObject>().SelectMany(o => o.GetComponents<IDmxDevice>()).ToArray();
        }

        private void Update()
        {
            foreach (var device in _dmxDevices)
            {
                device.DmxUpdate(_artNetReceiver.GetDmx(device.Universe).Skip(device.StartAddress).Take(device.ChannelNumber).ToArray());
            }
        }
    }
}
