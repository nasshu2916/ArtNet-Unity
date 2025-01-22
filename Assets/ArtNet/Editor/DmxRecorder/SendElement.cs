using System;
using System.Net;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    public class SendElement : ScriptableObject
    {
        [SerializeField] private string _ip = "127.0.0.1";
        [SerializeField] private int _port = ArtNetReceiver.ArtNetPort;
        [SerializeField] private bool _isSend = false;

        public string Ip { get => _ip; private set => _ip = value; }
        public int Port { get => _port; private set => _port = value; }
        public bool IsSend { get => _isSend; set => _isSend = value; }

        public EndPoint EndPoint { get; private set; }
        public bool IsValidated { get; private set; }


        public bool IsEnabled => IsValidated && IsSend;

        public SendElement()
        {
            if (SetEndpoint(Ip, Port) == false)
            {
                SetInvalidEndpoint();
            }
        }

        public bool SetIp(string ip)
        {
            Ip = ip;
            return SetEndpoint(Ip, Port);
        }

        public bool SetPort(int port)
        {
            Port = port;
            return SetEndpoint(Ip, Port);
        }

        private bool SetEndpoint(string ip, int port)
        {
            if ((port is >= 0 and <= 65535) && IPAddress.TryParse(ip, out var ipAddress))
            {
                EndPoint = new IPEndPoint(ipAddress, port);
                IsValidated = true;
                return true;
            }

            SetInvalidEndpoint();
            return false;
        }

        private void SetInvalidEndpoint()
        {
            EndPoint = null;
            IsValidated = false;
        }
    }
}
