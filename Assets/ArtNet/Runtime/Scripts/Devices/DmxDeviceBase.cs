using System;
using System.Linq;
using ArtNet.Common;
using UnityEngine;

namespace ArtNet.Devices
{
    public abstract class DmxDeviceBase : MonoBehaviour, IDmxDevice
    {
        [SerializeField, Range(0, 255)] private ushort universe;
        [SerializeField, Range(0, 511)] private ushort startAddress;

        protected byte[] DmxData;

        public ushort Universe => universe;
        public ushort StartAddress => startAddress;

        public abstract byte ChannelNumber { get; }

        protected abstract void InitFixture();
        protected abstract void UpdateProperties();

        private void Start()
        {
            DmxData = new byte[ChannelNumber];
            InitFixture();
        }


        public void DmxUpdate(byte[] dmx)
        {
            if (dmx.Length < ChannelNumber)
            {
                ArtNetLogger.LogError($"DMX data is too short. Expected {ChannelNumber} bytes, got {dmx.Length} bytes.");
                return;
            }

            if (dmx.SequenceEqual(DmxData)) return;
            Buffer.BlockCopy(dmx, 0, DmxData, 0, ChannelNumber);

            UpdateProperties();
        }
    }
}
