using ArtNet.Enums;
using ArtNet.IO;
using ArtNet.Sockets;

namespace ArtNet.Packets
{
    public class ArtPollReplyPacket : ArtPacket
    {
        public ArtPollReplyPacket() : base(OpCode.PollReply)
        {
        }

        public ArtPollReplyPacket(ReceivedData data) : base(data)
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

        protected override void ReadData(ArtReader reader)
        {
            IpAddress = reader.ReadBytes(4);
            Port = reader.ReadUInt16();
            VersionInfo = reader.ReadNetworkUInt16();
            NetSwitch = reader.ReadByte();
            SubSwitch = reader.ReadByte();
            Oem = reader.ReadNetworkUInt16();
            UbeaVersion = reader.ReadByte();
            Status1 = reader.ReadByte();
            EstaCode = reader.ReadNetworkUInt16();
            ShortName = reader.ReadNetworkString(18);
            LongName = reader.ReadNetworkString(64);
            NodeReport = reader.ReadNetworkString(64);
            NumPorts = reader.ReadNetworkUInt16();
            PortTypes = reader.ReadBytes(4);
            InputStatus = reader.ReadBytes(4);
            OutputStatus = reader.ReadBytes(4);
            InputSubSwitch = reader.ReadBytes(4);
            OutputSubSwitch = reader.ReadBytes(4);
            SwVideo = reader.ReadByte();
            SwMacro = reader.ReadByte();
            SwRemote = reader.ReadByte();
            Spares = reader.ReadBytes(3);
            Style = reader.ReadByte();
            MacAddress = reader.ReadBytes(6);
            BindIpAddress = reader.ReadBytes(4);
            BindIndex = reader.ReadByte();
            Status2 = reader.ReadByte();
            Filter = reader.ReadBytes(26);
        }

        protected override void WriteData(ArtWriter writer)
        {
            base.WriteData(writer);
            writer.Write(IpAddress);
            writer.Write(Port);
            writer.WriteNetwork(VersionInfo);
            writer.Write(NetSwitch);
            writer.Write(SubSwitch);
            writer.WriteNetwork(Oem);
            writer.Write(UbeaVersion);
            writer.Write(Status1);
            writer.Write(EstaCode);
            writer.WriteNetwork(ShortName, 18);
            writer.WriteNetwork(LongName, 64);
            writer.WriteNetwork(NodeReport, 64);
            writer.Write(NumPorts);
            writer.Write(PortTypes);
            writer.Write(InputStatus);
            writer.Write(OutputStatus);
            writer.Write(InputSubSwitch);
            writer.Write(OutputSubSwitch);
            writer.Write(SwVideo);
            writer.Write(SwMacro);
            writer.Write(SwRemote);
            writer.Write(Spares);
            writer.Write(Style);
            writer.Write(MacAddress);
            writer.Write(BindIpAddress);
            writer.Write(BindIndex);
            writer.Write(Status2);
            writer.Write(Filter);
        }
    }
}
