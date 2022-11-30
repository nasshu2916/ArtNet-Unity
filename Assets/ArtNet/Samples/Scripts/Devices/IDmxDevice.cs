namespace ArtNet.Samples.Devices
{
    public interface IDmxDevice
    {
        byte ChannelNumber { get; }
        byte Universe { get; }
        byte StartAddress { get; }

        void DmxUpdate(byte[] dmx);
    }
}
