#pragma warning disable 0414

using System;
using System.Collections.Generic;
using System.Net;
using ArtNet.Common;
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
        private readonly Dictionary<ushort, byte[]> _dmxData = new();

        private readonly UdpReceiver _receiver = new(ArtNetReceiver.ArtNetPort);
        private readonly Dictionary<ushort, UniverseInfo> _universeInfos = new();
        private readonly Queue<ushort> _updatedUniverses = new();
        private DmxViewer _dmxViewer;
        private Button _receiveStartButton;
        private ushort _selectedUniverse;
        private ScrollView _universeSelector;

        private void Update()
        {
            lock (_updatedUniverses)
            {
                while (0 < _updatedUniverses.Count)
                {
                    var universe = _updatedUniverses.Dequeue();
                    if (!_universeInfos.ContainsKey(universe)) AddUniverseInfo(universe);

                    _universeInfos[universe].ReceivedAt = DateTime.Now;
                    if (universe == _selectedUniverse)
                    {
                        _dmxViewer.value = _dmxData[universe];
                    }
                }
            }
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

        [MenuItem(Const.Editor.MenuItemNamePrefix + "ArtNetTester", false, Const.Editor.Priority)]
        public static void ShowExample()
        {
            var wnd = GetWindow<ArtNetTester>();
            wnd.titleContent = new GUIContent("ArtNetTester");
        }


        private void OnReceivedPacket(byte[] receiveBuffer, int length, EndPoint remoteEp)
        {
            var packet = ArtNetPacket.Create(receiveBuffer);
            if (packet == null) return;

            _lastOpCode = packet.OpCode.ToString();
            _lastReceived = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");

            if (packet.OpCode == OpCode.Dmx)
            {
                OnDmxReceived((DmxPacket) packet);
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
            ArtNetLogger.DevLogDebug("ArtNet Tester", "Start Receive ArtNet Packet");
        }

        private void StopReceive()
        {
            _receiver.StopReceive();
            _receiverStatus = "Not Running";
            _receiveStartButton.text = "Start Receive ArtNet Packet";
            _receiveStartButton.RemoveFromClassList("selected");
            ArtNetLogger.DevLogDebug("ArtNet Tester", "Stop Receive ArtNet Packet");
        }

        private void AddUniverseInfo(ushort universe)
        {
            var universeInfo = new UniverseInfo(universe);
            universeInfo.clickable.clickedWithEventInfo += evt => OnUniverseSelected(universe, evt);
            if (_universeInfos.Count == 0)
            {
                _selectedUniverse = universe;
                universeInfo.AddToClassList("selected");
            }

            _universeInfos.Add(universe, universeInfo);
            _universeSelector.Add(universeInfo);
        }

        private void OnUniverseSelected(ushort universe, EventBase evt)
        {
            if (evt.target is not UniverseInfo universeInfo) return;
            _universeSelector.Q<UniverseInfo>(null, "selected")?.RemoveFromClassList("selected");
            universeInfo.AddToClassList("selected");
            _selectedUniverse = universe;
            _dmxViewer.value = _dmxData[universe];
        }
    }
}
