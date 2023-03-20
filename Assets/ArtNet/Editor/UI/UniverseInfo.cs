using UnityEngine.UIElements;

namespace ArtNet.Editor.UI
{
    public class UniverseInfo : Button
    {
        private readonly Label _universeNumberLabel;

        private ushort _universeNumber;

        private ushort UniverseNumber
        {
            get => _universeNumber;
            set
            {
                _universeNumber = value;
                UpdateUniverseLabel(value);
            }
        }

        public UniverseInfo(ushort number) : this()
        {
            UniverseNumber = number;
        }

        private UniverseInfo()
        {
            _universeNumberLabel = new Label();
            Add(_universeNumberLabel);
        }

        private void UpdateUniverseLabel(ushort number)
        {
            _universeNumberLabel.text = $"Universe: {number + 1}";
        }
    }
}
