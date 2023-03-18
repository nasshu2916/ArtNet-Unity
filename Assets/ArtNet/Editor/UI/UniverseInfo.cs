using UnityEngine.UIElements;

namespace ArtNet.Editor.UI
{
    public class UniverseInfo : VisualElement
    {
        private readonly Label _universeNumberLabel;

        private int _universeNumber;

        public int UniverseNumber
        {
            get => _universeNumber;
            set
            {
                _universeNumber = value;
                UpdateUniverseLabel(value);
            }
        }

        public UniverseInfo()
        {
            _universeNumberLabel = new Label();
            UpdateUniverseLabel(_universeNumber);
            Add(_universeNumberLabel);
        }

        private void UpdateUniverseLabel(int number)
        {
            _universeNumberLabel.text = $"Universe: {number}";
        }
    }
}
