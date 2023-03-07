using System.Linq;
using ArtNet.Devices;
using UnityEngine;

namespace ArtNet.Samples
{
    public class DmxFixtureManager : MonoBehaviour
    {
        private IDmxDevice[] _dmxDevices;
        [SerializeField] private DmxDataManager dmxDataManager;

        private void OnEnable()
        {
            _dmxDevices = FindObjectsOfType<GameObject>().SelectMany(o => o.GetComponents<IDmxDevice>()).ToArray();
        }

        private void Update()
        {
            foreach (var device in _dmxDevices)
            {
                device.DmxUpdate(dmxDataManager.DmxValues(device.Universe).Skip(device.StartAddress)
                    .Take(device.ChannelNumber).ToArray());
            }
        }
    }
}
