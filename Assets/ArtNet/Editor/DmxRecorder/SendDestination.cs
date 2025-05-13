using System.Collections.Generic;
using System.Net;
using JetBrains.Annotations;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    public class SendDestination : ScriptableObject
    {
        [SerializeField] private string _ip = "127.0.0.1";
        [SerializeField] private int _port = ArtNetReceiver.ArtNetPort;
        [SerializeField] private bool _isSend = false;

        public string Ip { get => _ip; private set => _ip = value; }

        public int Port
        {
            get => _port;
            private set
            {
                if (value is < 1 or > 0xFFFF)
                {
                    throw new System.ArgumentOutOfRangeException(
                        $"Port number must be between 1 and 65535: value={value}");
                }

                _port = value;
            }
        }

        public bool IsSend { get => _isSend; set => _isSend = value; }

        public EndPoint EndPoint { get; private set; }
        public bool IsValidated { get; private set; }

        [NotNull] public static string DefaultName => "Destination";


        public bool IsEnabled => IsValidated && IsSend;

        public SendDestination()
        {
            if (SetEndpoint(Ip, Port) == false)
            {
                SetInvalidEndpoint();
            }
        }

        private void OnValidate()
        {
            Port = Mathf.Clamp(_port, 1, 0xFFFF);

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

        public bool HasErrors()
        {
            return !IsValidated;
        }

        public List<string> GetErrors()
        {
            var errors = new List<string>();
            if (EndPoint == null)
            {
                errors.Add("Invalid IP address or port number");
            }

            return errors;
        }
    }
}
