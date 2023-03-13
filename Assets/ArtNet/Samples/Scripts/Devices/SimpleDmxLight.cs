using System;
using ArtNet.Devices;
using UnityEngine;

namespace ArtNet.Samples.Devices
{
    [RequireComponent(typeof(Light))]
    public class SimpleDmxLight : DmxDeviceBase
    {
        private enum Properties
        {
            Red,
            Green,
            Blue,
            Dimmer
        }

        public override byte ChannelNumber { get; } = (byte) Enum.GetNames(typeof(Properties)).Length;

        private Light _light;
        private const float MaxIntensity = 2f;

        protected override void InitFixture()
        {
            _light = GetComponent<Light>();
        }

        protected override void UpdateProperties()
        {
            var r = DmxData[(int) Properties.Red] / 255f;
            var g = DmxData[(int) Properties.Green] / 255f;
            var b = DmxData[(int) Properties.Blue] / 255f;
            var intensity = DmxData[(int) Properties.Dimmer] / 255f * MaxIntensity;

            _light.color = new Color(r, g, b, 1f);
            _light.intensity = intensity;
        }
    }
}
