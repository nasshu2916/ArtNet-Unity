using UnityEngine;

namespace ArtNet.Samples.Devices
{
    [RequireComponent(typeof(Light))]
    public class SimpleLight : MonoBehaviour, IDmxDevice
    {
        private enum Fixture
        {
            Red = 0,
            Green = 1,
            Blue = 2,
            Dimmer = 3
        }

        private Light _light;
        public byte ChannelNumber => 4;
        public byte Universe => 0;

        public byte StartAddress => 0;

        private const float MaxIntensity = 2f;

        private void Start()
        {
            _light = GetComponent<Light>();
        }

        public void DmxUpdate(byte[] dmx)
        {
            var r = dmx[(int)Fixture.Red] / 255f;
            var g = dmx[(int)Fixture.Green] / 255f;
            var b = dmx[(int)Fixture.Blue] / 255f;
            var intensity = dmx[(int)Fixture.Dimmer] / 255f * MaxIntensity;

            _light.color = new Color(r, g, b, 1f);
            _light.intensity = intensity;
        }
    }
}
