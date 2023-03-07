namespace ArtNet.Devices
{
    public interface IDmxDevice
    {
        byte ChannelNumber { get; }
        int Universe { get; }
        int StartAddress { get; }
        void DmxUpdate(byte[] dmx);
    }
}
