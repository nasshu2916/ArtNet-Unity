# ArtNet-Unity
A tool to receive ArtNet in Unity(C#).
Receive DMX512 via ArtNet.

Required Unity version is 2020 or later.

![dmx_receive](Docs/dmx_receive.gif)

### Listen for ArtNet packets example

```csharp
artClient = new ArtClient();
artClient.Open();
artClient.ReceiveEvent += (sender, e) =>
{
    switch (e.Packet.OpCode)
    {
        case OpCode.Dmx:
            DmxPacket dmxPacket = e.Packet as DmxPacket;
            Debug.Log($"Universe {dmxPacket.Universe + 1} DMX1: {dmxPacket.Dmx[0]}");
            break;
        default:
            Debug.Log("Not support OpCode: 0x" + e.Packet.OpCode.ToString("X"));
            break;
    }
};
```

### Support OpCode
- OpPoll
- OpDmx
