#pragma warning disable 0414

using System;
using System.Collections.Generic;
using ArtNet.Editor.UI;
using ArtNet.Enums;
using ArtNet.Packets;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtNet.Editor
{
    public class ArtNetTester : EditorWindow
    {
        [SerializeField] private string _receiverStatus;
        [SerializeField] private string _lastReceived;
        [SerializeField] private string _lastOpCode;

        private readonly UdpReceiver _receiver = new(ArtNetReceiver.ArtNetPort);
        private readonly Dictionary<ushort, byte[]> _dmxData = new();
        private readonly Queue<ushort> _updatedUniverses = new();
        private readonly Dictionary<ushort, UniverseInfo> _universeInfos = new();
        private Button _receiveStartButton;
        private ScrollView _universeSelector;
        private DmxViewer _dmxViewer;
        private ushort _selectedUniverseNum;

        [MenuItem("ArtNet/ArtNetTester")]
        public static void ShowExample()
        {
            var wnd = GetWindow<ArtNetTester>();
            wnd.titleContent = new GUIContent("ArtNetTester");
        }

        public void CreateGUI()
        {
            minSize = new Vector2(750, 400);
            _receiver.OnReceivedPacket = OnReceivedPacket;

            var root = rootVisualElement;
            var visualTree = Resources.Load<VisualTreeAsset>("ArtNetTester");
            var visualElement = visualTree.Instantiate();
            root.Add(visualElement);

            var styleSheet = Resources.Load<StyleSheet>("ArtNetTester");
            root.styleSheets.Add(styleSheet);

            _receiverStatus = "Not Running";
            _receiveStartButton = root.Q<Button>("ReceiveStartButton");
            _receiveStartButton.text = "Start Receive ArtNet Packet";
            _receiveStartButton.clicked += OnClickReceiveStartButton;
            _universeSelector = root.Q<ScrollView>("UniverseSelector");
            _dmxViewer = root.Q<DmxViewer>("DmxViewer");

            root.Bind(new SerializedObject(this));
        }

        private void Update()
        {
            lock (_updatedUniverses)
            {
                while (0 < _updatedUniverses.Count)
                {
                    var universe = _updatedUniverses.Dequeue();
                    if (!_universeInfos.ContainsKey(universe)) AddUniverseInfo(universe);

                    _universeInfos[universe].ReceivedAt = DateTime.Now;
                    if (universe == _selectedUniverseNum)
                    {
                        _dmxViewer.value = _dmxData[universe];
                    }
                }
            }
        }


        private void OnReceivedPacket(byte[] receiveBuffer, int length, System.Net.EndPoint remoteEp)
        {
            var packet = ArtNetPacket.Create(receiveBuffer);
            if (packet == null) return;

            _lastOpCode = packet.OpCode.ToString();
            _lastReceived = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");

            if (packet.OpCode == OpCode.Dmx)
            {
                OnDmxReceived((DmxPacket)packet);
            }
        }

        private void OnDmxReceived(DmxPacket packet)
        {
            var universe = packet.Universe;
            if (!_dmxData.ContainsKey(universe)) _dmxData.Add(universe, packet.Dmx);
            Buffer.BlockCopy(packet.Dmx, 0, _dmxData[universe], 0, 512);
            lock (_updatedUniverses)
            {
                if (_updatedUniverses.Contains(universe)) return;
                _updatedUniverses.Enqueue(universe);
            }
        }

        private void OnClickReceiveStartButton()
        {
            if (_receiver.IsRunning)
            {
                StopReceive();
            }
            else
            {
                StartReceive();
            }
        }

        private void StartReceive()
        {
            _receiver.StartReceive();
            _receiverStatus = "Running";
            _receiveStartButton.text = "Stop Receive ArtNet Packet";
            _receiveStartButton.AddToClassList("selected");
        }

        private void StopReceive()
        {
            _receiver.StopReceive();
            _receiverStatus = "Not Running";
            _receiveStartButton.text = "Start Receive ArtNet Packet";
            _receiveStartButton.RemoveFromClassList("selected");
        }

        private void AddUniverseInfo(ushort universe)
        {
            var universeInfo = new UniverseInfo(universe);
            universeInfo.clickable.clickedWithEventInfo += evt => OnUniverseSelected(universe, evt);
            if (_universeInfos.Count == 0)
            {
                _selectedUniverseNum = universe;
                universeInfo.AddToClassList("selected");
            }

            _universeInfos.Add(universe, universeInfo);
            _universeSelector.Add(universeInfo);
        }

        private void OnUniverseSelected(ushort universeNumber, EventBase evt)
        {
            if (evt.target is not UniverseInfo universeInfo) return;
            _universeSelector.Q<UniverseInfo>(null, "selected")?.RemoveFromClassList("selected");
            universeInfo.AddToClassList("selected");
            _selectedUniverseNum = universeNumber;
            _dmxViewer.value = _dmxData[universeNumber];
        }
    }
}
