using ArtNet.Enums;
using ArtNet.IO;

namespace ArtNet.Packets
{
    public class PollReplyPacket : ArtNetPacket
    {
        public PollReplyPacket() : base(OpCode.PollReply)
        {
        }

        public PollReplyPacket(byte[] buffer) : base(buffer, OpCode.PollReply)
        {
        }

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

        protected override void ReadData(ArtNetReader netReader)
        {
            IpAddress = netReader.ReadBytes(4);
            Port = netReader.ReadUInt16();
            VersionInfo = netReader.ReadNetworkUInt16();
            NetSwitch = netReader.ReadByte();
            SubSwitch = netReader.ReadByte();
            Oem = netReader.ReadNetworkUInt16();
            UbeaVersion = netReader.ReadByte();
            Status1 = netReader.ReadByte();
            EstaCode = netReader.ReadNetworkUInt16();
            ShortName = netReader.ReadNetworkString(18);
            LongName = netReader.ReadNetworkString(64);
            NodeReport = netReader.ReadNetworkString(64);
            NumPorts = netReader.ReadNetworkUInt16();
            PortTypes = netReader.ReadBytes(4);
            InputStatus = netReader.ReadBytes(4);
            OutputStatus = netReader.ReadBytes(4);
            InputSubSwitch = netReader.ReadBytes(4);
            OutputSubSwitch = netReader.ReadBytes(4);
            SwVideo = netReader.ReadByte();
            SwMacro = netReader.ReadByte();
            SwRemote = netReader.ReadByte();
            Spares = netReader.ReadBytes(3);
            Style = netReader.ReadByte();
            MacAddress = netReader.ReadBytes(6);
            BindIpAddress = netReader.ReadBytes(4);
            BindIndex = netReader.ReadByte();
            Status2 = netReader.ReadByte();
            Filter = netReader.ReadBytes(26);
        }

        protected override void WriteData(ArtNetWriter netWriter)
        {
            base.WriteData(netWriter);
            netWriter.Write(IpAddress);
            netWriter.Write(Port);
            netWriter.WriteNetwork(VersionInfo);
            netWriter.Write(NetSwitch);
            netWriter.Write(SubSwitch);
            netWriter.WriteNetwork(Oem);
            netWriter.Write(UbeaVersion);
            netWriter.Write(Status1);
            netWriter.Write(EstaCode);
            netWriter.WriteNetwork(ShortName, 18);
            netWriter.WriteNetwork(LongName, 64);
            netWriter.WriteNetwork(NodeReport, 64);
            netWriter.Write(NumPorts);
            netWriter.Write(PortTypes);
            netWriter.Write(InputStatus);
            netWriter.Write(OutputStatus);
            netWriter.Write(InputSubSwitch);
            netWriter.Write(OutputSubSwitch);
            netWriter.Write(SwVideo);
            netWriter.Write(SwMacro);
            netWriter.Write(SwRemote);
            netWriter.Write(Spares);
            netWriter.Write(Style);
            netWriter.Write(MacAddress);
            netWriter.Write(BindIpAddress);
            netWriter.Write(BindIndex);
            netWriter.Write(Status2);
            netWriter.Write(Filter);
        }
    }
}
