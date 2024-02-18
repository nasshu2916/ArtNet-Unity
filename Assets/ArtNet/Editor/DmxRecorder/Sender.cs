using System.Collections.Generic;
using ArtNet.Packets;

namespace ArtNet.Editor.DmxRecorder
{
    public class Sender
    {
        private readonly Dictionary<int, byte> _sequenceMap = new();
        private readonly UdpSender _udpSender = new(ArtNetReceiver.ArtNetPort);
        public SenderConfig Config { get; } = new();
        private List<(int, DmxPacket)> DmxPackets { get; set; } = new();

        public bool IsPlaying { get; private set; }
        public float CurrentTime { get; private set; }
        public int MaxTime => DmxPackets.Count > 0 ? DmxPackets[^1].Item1 : 0;


        public void Load(string path)
        {
            if (!System.IO.File.Exists(path)) return;

            var data = System.IO.File.ReadAllBytes(path);
            DmxPackets = RecordData.Deserialize(data);
        }

        public void Play()
        {
            if (IsPlaying) return;
            IsPlaying = true;
        }

        public void Stop()
        {
            if (!IsPlaying) return;
            IsPlaying = false;
        }

        public void ChangePlayTime(float time)
        {
            CurrentTime = time;
        }

        public void Update(float deltaTime)
        {
            if (!IsPlaying) return;
            var oldTime = CurrentTime;

            CurrentTime += deltaTime;

            var isReset = false;
            if (CurrentTime > MaxTime)
            {
                if (!Config.IsLoop)
                {
                    IsPlaying = false;
                }

                isReset = true;
                CurrentTime = MaxTime;
            }

            var dmx = DmxPackets.FindLast(packet =>
                packet.Item1 <= (int) CurrentTime && packet.Item1 > (int) oldTime);
            if (dmx != default) SendDmx(dmx.Item2);

            if (isReset) CurrentTime = 0;
        }

        private void SendDmx(DmxPacket packet)
        {
            var universe = packet.Universe;
            if (!Config.IsRecordSequence)
            {
                var sequence = _sequenceMap.GetValueOrDefault(universe, (byte) 0);
                sequence = sequence == byte.MaxValue ? (byte) 0 : (byte) (sequence + 1);
                packet.Sequence = sequence;
                _sequenceMap[universe] = sequence;
            }
            else
            {
                _sequenceMap[universe] = packet.Sequence;
            }

            var data = packet.ToByteArray();
            _udpSender.Send(data, Config.Ip);
        }
    }
}
