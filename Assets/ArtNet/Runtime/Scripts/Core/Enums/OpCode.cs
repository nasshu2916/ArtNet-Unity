namespace ArtNet.Enums
{
    public enum OpCode : ushort
    {
        Poll = 0x2000,
        PollReply = 0x2100,
        Dmx = 0x5000,
        Sync = 0x5200,
        Address = 0x6000,
        TodRequest = 0x8000,
        TodData = 0x8100,
        TimeCode = 0x9700
    }
}
