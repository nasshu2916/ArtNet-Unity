using System;
using UnityEditor;
using UnityEditor.Presets;

namespace ArtNet.Editor.DmxRecorder
{
    public class PresetRecorder : PresetSelectorReceiver
    {
        private RecorderSettings _target;
        private Preset _initialValue;
        private Action _onSelectionChanged;
        private Action _onSelectionClosed;

        internal void Init(RecorderSettings target, Action onSelectionChanged = null, Action onSelectionClosed = null)
        {
            _onSelectionChanged = onSelectionChanged;
            _onSelectionClosed = onSelectionClosed;
            _target = target;
            _initialValue = new Preset(target);
        }

        public override void OnSelectionChanged(Preset selection)
        {
            if (selection != null)
            {
                Undo.RecordObject(_target, "Apply Preset " + selection.name);
                selection.ApplyTo(_target);
            }
            else
            {
                Undo.RecordObject(_target, "Cancel Preset");
                _initialValue.ApplyTo(_target);
            }

            _onSelectionChanged?.Invoke();
        }

        public override void OnSelectionClosed(Preset selection)
        {
            OnSelectionChanged(selection);

            _target.OnAfterDuplicate();
            _onSelectionClosed?.Invoke();

            DestroyImmediate(this);
        }
    }
}
