using ArtNet.Enums;
using ArtNet.IO;

namespace ArtNet.Packets
{
    public class PollReplyPacket : ArtNetPacket
    {
        public override OpCode OpCode => OpCode.PollReply;
        protected override int MinimumBodyLength => 197;

        public byte[] IpAddress { get; set; } = new byte[4];
        public ushort Port { get; set; }
        public ushort VersionInfo { get; set; }
        public byte NetSwitch { get; set; }
        public byte SubSwitch { get; set; }
        public ushort Oem { get; set; }
        public byte UbeaVersion { get; set; }
        public byte Status1 { get; set; }
        public ushort EstaCode { get; set; }
        public string ShortName { get; set; } = string.Empty;
        public string LongName { get; set; } = string.Empty;
        public string NodeReport { get; set; } = string.Empty;
        public ushort NumPorts { get; set; }
        public byte[] PortTypes { get; set; } = new byte[4];
        public byte[] InputStatus { get; set; } = new byte[4];
        public byte[] OutputStatus { get; set; } = new byte[4];
        public byte[] InputSubSwitch { get; set; } = new byte[4];
        public byte[] OutputSubSwitch { get; set; } = new byte[4];
        public byte SwVideo { get; set; }
        public byte SwMacro { get; set; }
        public byte SwRemote { get; set; }
        public byte[] Spares { get; set; } = new byte[3];
        public byte Style { get; set; }
        public byte[] MacAddress { get; set; } = new byte[6];
        public byte[] BindIpAddress { get; set; } = new byte[4];
        public byte BindIndex { get; set; }
        public byte Status2 { get; set; }
        public byte[] Filter { get; set; } = new byte[26];

        protected override bool DeserializeBody(ArtNetReader artNetReader)
        {
            IpAddress = artNetReader.ReadBytes(4);
            Port = artNetReader.ReadUInt16();
            VersionInfo = artNetReader.ReadNetworkUInt16();
            NetSwitch = artNetReader.ReadByte();
            SubSwitch = artNetReader.ReadByte();
            Oem = artNetReader.ReadNetworkUInt16();
            UbeaVersion = artNetReader.ReadByte();
            Status1 = artNetReader.ReadByte();
            EstaCode = artNetReader.ReadNetworkUInt16();
            ShortName = artNetReader.ReadString(18);
            LongName = artNetReader.ReadString(64);
            NodeReport = artNetReader.ReadString(64);
            NumPorts = artNetReader.ReadNetworkUInt16();
            PortTypes = artNetReader.ReadBytes(4);
            InputStatus = artNetReader.ReadBytes(4);
            OutputStatus = artNetReader.ReadBytes(4);
            InputSubSwitch = artNetReader.ReadBytes(4);
            OutputSubSwitch = artNetReader.ReadBytes(4);
            SwVideo = artNetReader.ReadByte();
            SwMacro = artNetReader.ReadByte();
            SwRemote = artNetReader.ReadByte();
            Spares = artNetReader.ReadBytes(3);
            Style = artNetReader.ReadByte();
            MacAddress = artNetReader.ReadBytes(6);
            BindIpAddress = artNetReader.ReadBytes(4);
            BindIndex = artNetReader.ReadByte();
            Status2 = artNetReader.ReadByte();
            Filter = artNetReader.ReadBytes(26);
            return true;
        }

        protected override void SerializeBody(ArtNetWriter artNetWriter)
        {
            artNetWriter.Write(IpAddress);
            artNetWriter.Write(Port);
            artNetWriter.WriteNetwork(VersionInfo);
            artNetWriter.Write(NetSwitch);
            artNetWriter.Write(SubSwitch);
            artNetWriter.WriteNetwork(Oem);
            artNetWriter.Write(UbeaVersion);
            artNetWriter.Write(Status1);
            artNetWriter.Write(EstaCode);
            artNetWriter.WriteNetwork(ShortName, 18);
            artNetWriter.WriteNetwork(LongName, 64);
            artNetWriter.WriteNetwork(NodeReport, 64);
            artNetWriter.Write(NumPorts);
            artNetWriter.Write(PortTypes);
            artNetWriter.Write(InputStatus);
            artNetWriter.Write(OutputStatus);
            artNetWriter.Write(InputSubSwitch);
            artNetWriter.Write(OutputSubSwitch);
            artNetWriter.Write(SwVideo);
            artNetWriter.Write(SwMacro);
            artNetWriter.Write(SwRemote);
            artNetWriter.Write(Spares);
            artNetWriter.Write(Style);
            artNetWriter.Write(MacAddress);
            artNetWriter.Write(BindIpAddress);
            artNetWriter.Write(BindIndex);
            artNetWriter.Write(Status2);
            artNetWriter.Write(Filter);
        }

        protected override bool Validate()
        {
            return true;
        }
    }
}
