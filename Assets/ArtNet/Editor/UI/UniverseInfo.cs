using System;
using UnityEngine.UIElements;

namespace ArtNet.Editor.UI
{
    public class UniverseInfo : Button
    {
        private readonly Label _universeLabel;
        private readonly Label _receivedAtLabel;
        private ushort _universe;

        public DateTime ReceivedAt
        {
            set => _receivedAtLabel.text = $"Received at: {value:HH:mm:ss.fff}";
        }

        private ushort Universe
        {
            get => _universe;
            set
            {
                _universe = value;
                _universeLabel.text = $"Universe: {_universe}";
            }
        }

        public UniverseInfo(ushort universe) : this()
        {
            Universe = universe;
        }

        private UniverseInfo()
        {
            _universeLabel = new Label();
            _universeLabel.AddToClassList("universe-label");
            Add(_universeLabel);
            _receivedAtLabel = new Label();
            _receivedAtLabel.AddToClassList("received-at-label");
            Add(_receivedAtLabel);
        }
    }
}
