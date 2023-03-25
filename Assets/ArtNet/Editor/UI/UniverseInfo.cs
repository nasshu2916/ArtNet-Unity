using System;
using UnityEngine.UIElements;

namespace ArtNet.Editor.UI
{
    public class UniverseInfo : Button
    {
        private readonly Label _universeNumberLabel;
        private readonly Label _receivedAtLabel;
        private ushort _universeNumber;

        public DateTime ReceivedAt
        {
            set => _receivedAtLabel.text = $"Received at: {value:HH:mm:ss.fff}";
        }

        private ushort UniverseNumber
        {
            get => _universeNumber;
            set
            {
                _universeNumber = value;
                _universeNumberLabel.text = $"Universe: {_universeNumber + 1}";
            }
        }

        public UniverseInfo(ushort number) : this()
        {
            UniverseNumber = number;
        }

        private UniverseInfo()
        {
            _universeNumberLabel = new Label();
            _universeNumberLabel.AddToClassList("universe-number-label");
            Add(_universeNumberLabel);
            _receivedAtLabel = new Label();
            _receivedAtLabel.AddToClassList("received-at-label");
            Add(_receivedAtLabel);
        }
    }
}
