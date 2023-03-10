using UnityEngine;

namespace ArtNet.Devices
{
    public abstract class DmxDeviceBase : MonoBehaviour, IDmxDevice
    {
        [SerializeField, Range(0, 255)] private ushort universe;
        [SerializeField, Range(0, 511)] private ushort startAddress;

        public ushort Universe => universe;
        public ushort StartAddress => startAddress;

        public abstract byte ChannelNumber { get; }

        public virtual void DmxUpdate(byte[] dmx)
        {
        }
    }
}
