using System;
using System.Net;
using ArtNet.Enums;
using ArtNet.Packets;
using UnityEngine;
using UnityEngine.Events;

namespace ArtNet
{
    [Serializable]
    internal class OnReceivedPollEvent : UnityEvent<ReceivedData<PollPacket>>
    {
    }

    [Serializable]
    internal class OnReceivedPollReplyEvent : UnityEvent<ReceivedData<PollReplyPacket>>
    {
    }

    [Serializable]
    internal class OnReceivedDmxEvent : UnityEvent<ReceivedData<DmxPacket>>
    {
    }

    [Serializable]
    internal class OnReceivedSyncEvent : UnityEvent<ReceivedData<SyncPacket>>
    {
    }

    [Serializable]
    internal class OnReceivedTimeCodeEvent : UnityEvent<ReceivedData<TimeCodePacket>>
    {
    }

    [Serializable]
    internal class OnReceivedAddressEvent : UnityEvent<ReceivedData<AddressPacket>>
    {
    }

    [Serializable]
    internal class OnReceivedTodRequestEvent : UnityEvent<ReceivedData<TodRequestPacket>>
    {
    }

    [Serializable]
    internal class OnReceivedTodDataEvent : UnityEvent<ReceivedData<TodDataPacket>>
    {
    }

    [Serializable]
    internal class OnReceivedTodControlEvent : UnityEvent<ReceivedData<TodControlPacket>>
    {
    }

    [Serializable]
    internal class OnReceivedRdmEvent : UnityEvent<ReceivedData<RdmPacket>>
    {
    }

    public class ArtNetReceiver : MonoBehaviour
    {
        public const int ArtNetPort = 6454;

        [SerializeField] private bool _autoStart = true;
        [SerializeField] private bool _invokeUnityEventWhenCSharpEventSubscribed;
        [SerializeField] private OnReceivedPollEvent _onReceivedPollEvent;
        [SerializeField] private OnReceivedPollReplyEvent _onReceivedPollReplyEvent;
        [SerializeField] private OnReceivedDmxEvent _onReceivedDmxEvent;
        [SerializeField] private OnReceivedSyncEvent _onReceivedSyncEvent;
        [SerializeField] private OnReceivedTimeCodeEvent _onReceivedTimeCodeEvent;
        [SerializeField] private OnReceivedAddressEvent _onReceivedAddressEvent;
        [SerializeField] private OnReceivedTodRequestEvent _onReceivedTodRequestEvent;
        [SerializeField] private OnReceivedTodDataEvent _onReceivedTodDataEvent;
        [SerializeField] private OnReceivedTodControlEvent _onReceivedTodControlEvent;
        [SerializeField] private OnReceivedRdmEvent _onReceivedRdmEvent;

        private UdpReceiver UdpReceiver { get; } = new(ArtNetPort);
        public DateTime LastReceivedAt { get; private set; }
        public bool IsConnected => LastReceivedAt.AddSeconds(1) > DateTime.Now;
        public event Action<ReceivedData<PollPacket>> OnReceivedPoll;
        public event Action<ReceivedData<PollReplyPacket>> OnReceivedPollReply;
        public event Action<ReceivedData<DmxPacket>> OnReceivedDmx;
        public event Action<ReceivedData<SyncPacket>> OnReceivedSync;
        public event Action<ReceivedData<TimeCodePacket>> OnReceivedTimeCode;
        public event Action<ReceivedData<AddressPacket>> OnReceivedAddress;
        public event Action<ReceivedData<TodRequestPacket>> OnReceivedTodRequest;
        public event Action<ReceivedData<TodDataPacket>> OnReceivedTodData;
        public event Action<ReceivedData<TodControlPacket>> OnReceivedTodControl;
        public event Action<ReceivedData<RdmPacket>> OnReceivedRdm;

        private void Awake()
        {
            UdpReceiver.OnReceivedPacket = OnReceivedPacket;
        }

        private void OnEnable()
        {
            if (_autoStart) UdpReceiver.StartReceive();
        }

        private void OnDisable()
        {
            UdpReceiver.StopReceive();
        }

        private void OnReceivedPacket(byte[] receiveBuffer, int length, EndPoint remoteEp)
        {
            var buffer = receiveBuffer.AsSpan(0, length);
            if (!ArtNetPacket.TryGetOpCode(buffer, out var opCode)) return;
            LastReceivedAt = DateTime.Now;

            switch (opCode)
            {
                case OpCode.Dmx:
                    DispatchDmx(buffer, remoteEp, OnReceivedDmx, _onReceivedDmxEvent);
                    break;
                case OpCode.Poll:
                    DispatchStandard(buffer, remoteEp, OnReceivedPoll, _onReceivedPollEvent);
                    break;
                case OpCode.PollReply:
                    DispatchStandard(buffer, remoteEp, OnReceivedPollReply, _onReceivedPollReplyEvent);
                    break;
                case OpCode.Sync:
                    DispatchStandard(buffer, remoteEp, OnReceivedSync, _onReceivedSyncEvent);
                    break;
                case OpCode.TimeCode:
                    DispatchStandard(buffer, remoteEp, OnReceivedTimeCode, _onReceivedTimeCodeEvent);
                    break;
                case OpCode.Address:
                    DispatchStandard(buffer, remoteEp, OnReceivedAddress, _onReceivedAddressEvent);
                    break;
                case OpCode.TodRequest:
                    DispatchStandard(buffer, remoteEp, OnReceivedTodRequest, _onReceivedTodRequestEvent);
                    break;
                case OpCode.TodData:
                    DispatchStandard(buffer, remoteEp, OnReceivedTodData, _onReceivedTodDataEvent);
                    break;
                case OpCode.TodControl:
                    DispatchStandard(buffer, remoteEp, OnReceivedTodControl, _onReceivedTodControlEvent);
                    break;
                case OpCode.Rdm:
                    DispatchStandard(buffer, remoteEp, OnReceivedRdm, _onReceivedRdmEvent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DispatchDmx(
            ReadOnlySpan<byte> buffer,
            EndPoint remoteEp,
            Action<ReceivedData<DmxPacket>> cSharpHandler,
            UnityEvent<ReceivedData<DmxPacket>> unityHandler)
        {
            if (!DmxPacket.TryParse(buffer, out var packet)) return;
            Dispatch(new ReceivedData<DmxPacket>(packet, remoteEp), cSharpHandler, unityHandler);
        }

        private void DispatchStandard<TPacket>(
            ReadOnlySpan<byte> buffer,
            EndPoint remoteEp,
            Action<ReceivedData<TPacket>> cSharpHandler,
            UnityEvent<ReceivedData<TPacket>> unityHandler) where TPacket : ArtNetPacket, new()
        {
            var packet = ArtNetPacket.FromByteArray<TPacket>(buffer, false);
            if (packet == null) return;
            Dispatch(new ReceivedData<TPacket>(packet, remoteEp), cSharpHandler, unityHandler);
        }

        private void Dispatch<TPacket>(
            ReceivedData<TPacket> receivedData,
            Action<ReceivedData<TPacket>> cSharpHandler,
            UnityEvent<ReceivedData<TPacket>> unityHandler) where TPacket : ArtNetPacket
        {
            var hasCSharpHandler = cSharpHandler != null;
            cSharpHandler?.Invoke(receivedData);
            if (_invokeUnityEventWhenCSharpEventSubscribed || !hasCSharpHandler)
            {
                unityHandler?.Invoke(receivedData);
            }
        }
    }
}
