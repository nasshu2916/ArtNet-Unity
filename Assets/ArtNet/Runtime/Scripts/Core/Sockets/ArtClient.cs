using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ArtNet.Packets;
using UnityEngine;

namespace ArtNet.Sockets
{
    public class ArtClient
    {
        private const int ArtNetPort = 6454;

        private UdpClient _udpClient;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _task;
        public int Port { get; }
        public DateTime LastReceiveAt { get; private set; }
        public bool IsRunning => _task is { IsCanceled: false, IsCompleted: false };

        public event EventHandler<ReceiveEventArgs<ArtPacket>> ReceiveEvent;
        public ErrorOccuredEventHandler OnUdpStartFailed = _ => { };
        public ErrorOccuredEventHandler OnUdpReceiveFailed = _ => { };

        public delegate void ErrorOccuredEventHandler(Exception e);


        public ArtClient(int port = ArtNetPort)
        {
            Port = port;
        }

        ~ArtClient()
        {
            UdpStop();
        }

        public void UdpStart()
        {
            UdpStop();
            if (IsRunning) return;

            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _task = Task.Run(() => UdpTaskAsync(_cancellationTokenSource.Token));
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat($"[ArtNetClient] Udp start failed. {e.GetType()} : {e.Message}");
                OnUdpStartFailed?.Invoke(e);
            }
        }

        public void UdpStop()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _task = null;
            }

            if (_udpClient == null) return;
            _udpClient.Close();
            _udpClient = null;
        }

        private async void UdpTaskAsync(CancellationToken cancellationToken)
        {
            Debug.Log("[ArtClient] Udp task start.");
            _udpClient = new UdpClient(Port);

            while (!cancellationToken.IsCancellationRequested && _udpClient != null)
            {
                try
                {
                    var data = await _udpClient.ReceiveAsync();
                    LastReceiveAt = DateTime.Now;
                    var receivedData = new ReceivedData(data.Buffer, data.RemoteEndPoint.Address);
                    var packet = ArtPacket.Create(receivedData);

                    ReceiveEvent?.Invoke(this, new ReceiveEventArgs<ArtPacket>(packet, data.RemoteEndPoint));
                }
                catch (Exception e) when (e is SocketException or ObjectDisposedException)
                {
                    Debug.Log($"[ArtClient] Udp task failed. {e.Message} : {e.GetType()}");
                }
                catch (ArgumentException e)
                {
                    Debug.Log($"[ArtClient] Udp task failed. {e.Message} : {e.GetType()}");
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat($"[ArtClient] Udp receive failed. {e.Message} : {e.GetType()}");
                    UdpStop();
                    OnUdpReceiveFailed?.Invoke(e);
                    break;
                }
            }
        }
    }
}
