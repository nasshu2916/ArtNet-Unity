namespace ArtNet.Enums
{
    public enum OpCode : ushort
    {
        Poll = 0x2000,
        PollReply = 0x2100,
        Dmx = 0x5000
    }
}
