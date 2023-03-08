using System;
using ArtNet.Devices;
using UnityEngine;

namespace ArtNet.Samples.Devices
{
    [RequireComponent(typeof(Light))]
    public class SimpleDmxLight : DmxDeviceBase
    {
        private enum Fixtures
        {
            Red,
            Green,
            Blue,
            Dimmer
        }

        public override byte ChannelNumber { get; } = (byte) Enum.GetNames(typeof(Fixtures)).Length;

        private Light _light;
        private const float MaxIntensity = 2f;

        private void Start()
        {
            _light = GetComponent<Light>();
        }

        public override void DmxUpdate(byte[] dmx)
        {
            var r = dmx[(int) Fixtures.Red] / 255f;
            var g = dmx[(int) Fixtures.Green] / 255f;
            var b = dmx[(int) Fixtures.Blue] / 255f;
            var intensity = dmx[(int) Fixtures.Dimmer] / 255f * MaxIntensity;

            _light.color = new Color(r, g, b, 1f);
            _light.intensity = intensity;
        }
    }
}
