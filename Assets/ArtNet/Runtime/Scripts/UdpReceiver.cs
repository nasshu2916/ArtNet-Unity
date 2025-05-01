using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ArtNet.Common;

namespace ArtNet
{
    public sealed class UdpReceiver
    {
        private Socket _socket;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _task;
        private byte[] _receiveBuffer = new byte[1500];

        public int Port { get; }
        public bool IsRunning => _task is { IsCanceled: false, IsCompleted: false };

        public ReceivedPacketEventHandler OnReceivedPacket = (_, _, _) => { };
        public ErrorOccurredEventHandler OnUdpStartFailed = _ => { };
        public ErrorOccurredEventHandler OnUdpReceiveFailed = _ => { };
        public ErrorOccurredEventHandler OnUdpReceiveRaiseException = _ => { };

        public delegate void ReceivedPacketEventHandler(byte[] receiveBuffer, int length, EndPoint remoteEp);
        public delegate void ErrorOccurredEventHandler(Exception e);
        public UdpReceiver(int port)
        {
            Port = port;
        }

        ~UdpReceiver()
        {
            StopReceive();
        }

        public void StartReceive()
        {
            StopReceive();
            if (IsRunning) return;

            if (Port == 0) ArtNetLogger.LogError("ArtNetReceiver", "Port is not set.");

            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _socket.Bind(new IPEndPoint(IPAddress.Any, Port));

                _cancellationTokenSource = new CancellationTokenSource();
                _task = Task.Run(() => UdpTaskAsync(_cancellationTokenSource.Token));
            }
            catch (Exception e)
            {
                ArtNetLogger.LogError("ArtNetReceiver", $"UDP start failed. {e.GetType()} : {e.Message}");
                OnUdpStartFailed?.Invoke(e);
            }
        }

        private async Task UdpTaskAsync(CancellationToken token)
        {
            ArtNetLogger.LogInfo("ArtNetReceiver", $"UDP Receive task start. port: {Port}");

            while (!token.IsCancellationRequested && _socket != null)
            {
                try
                {
                    EndPoint remoteEp = new IPEndPoint(IPAddress.Any, 0);
                    var result = await _socket.ReceiveFromAsync(_receiveBuffer, SocketFlags.None, remoteEp);

                    if (result.ReceivedBytes != 0)
                    {
                        OnReceivedPacket?.Invoke(_receiveBuffer, result.ReceivedBytes, result.RemoteEndPoint);
                    }
                }
                catch (Exception e) when (e is SocketException or ObjectDisposedException)
                {
                    ArtNetLogger.LogInfo("ArtNetReceiver", $"UDP Receive task failed. {e.Message} : {e.GetType()}");
                    OnUdpReceiveFailed?.Invoke(e);
                }
                catch (Exception e)
                {
                    ArtNetLogger.LogError("ArtNetReceiver", $"UDP receive failed. {e.Message} : {e.GetType()}");
                    OnUdpReceiveRaiseException?.Invoke(e);
                }
            }
        }

        public void StopReceive()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _task = null;
            }

            if (_socket == null) return;
            _socket.Close();
            _socket = null;
        }
    }
}
