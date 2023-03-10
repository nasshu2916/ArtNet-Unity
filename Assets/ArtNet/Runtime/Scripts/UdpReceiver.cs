using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ArtNet
{
    public abstract class UdpReceiver : MonoBehaviour
    {
        private Socket _socket;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _task;
        private byte[] _receiveBuffer = new byte[1500];

        public int Port { get; protected set; }
        public bool IsRunning => _task is {IsCanceled: false, IsCompleted: false};

        public ErrorOccuredEventHandler OnUdpStartFailed = _ => { };
        public ErrorOccuredEventHandler OnUdpReceiveFailed = _ => { };

        public delegate void ErrorOccuredEventHandler(Exception e);

        private void OnApplicationQuit()
        {
            StopReceive();
        }

        public void StartReceive()
        {
            StopReceive();
            if (IsRunning) return;

            if (Port == 0) Debug.LogWarning("[UdpReceiver] Port is not set.");

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
                Debug.LogErrorFormat($"[UdpReceiver] Udp start failed. {e.GetType()} : {e.Message}");
                OnUdpStartFailed?.Invoke(e);
            }
        }

        private async Task UdpTaskAsync(CancellationToken token)
        {
            Debug.Log($"[UdpReceiver] Udp Receive task start. port: {Port}");

            while (!token.IsCancellationRequested && _socket != null)
            {
                try
                {
                    EndPoint remoteEp = new IPEndPoint(IPAddress.Any, 0);
                    var result = await _socket.ReceiveFromAsync(_receiveBuffer, SocketFlags.None, remoteEp);

                    if (result.ReceivedBytes != 0)
                    {
                        OnReceivedPacket(_receiveBuffer, result.ReceivedBytes, result.RemoteEndPoint);
                    }
                }
                catch (Exception e) when (e is SocketException or ObjectDisposedException)
                {
                    Debug.Log($"[UdpReceiver] Udp Receive task failed. {e.Message} : {e.GetType()}");
                    OnUdpReceiveFailed?.Invoke(e);
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat($"[UdpReceiver] Udp receive failed. {e.Message} : {e.GetType()}");
                    OnRaiseError(e);
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

        protected virtual void OnRaiseError(Exception e)
        {
            StopReceive();
            OnUdpReceiveFailed?.Invoke(e);
        }

        protected abstract void OnReceivedPacket(byte[] receiveBuffer, int length, EndPoint remoteEp);
    }
}
