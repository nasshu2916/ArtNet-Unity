namespace ArtNet.Devices
{
    public interface IDmxDevice
    {
        byte ChannelNumber { get; }
        ushort Universe { get; }
        ushort StartAddress { get; }
        void DmxUpdate(byte[] dmx);
    }
}
