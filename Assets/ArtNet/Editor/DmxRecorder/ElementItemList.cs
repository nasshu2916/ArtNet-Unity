using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtNet.Editor.DmxRecorder
{
    internal class ElementItemList<T> : VisualElement where T : VisualElement
    {
        public event Action OnSelectionChanged;
        public event Action OnContextMenu;
        public event Action<T> OnItemContextMenu;
        public event Action<T> OnItemRename;

        private int _selectIndex;

        private readonly ScrollView _scrollView;

        public List<T> Items { get; } = new();

        public int SelectedIndex
        {
            get => _selectIndex;
            set
            {
                _selectIndex = value;
                OnSelectionChanged?.Invoke();
            }
        }

        public T Selection
        {
            get
            {
                if (SelectedIndex < 0 || SelectedIndex > Items.Count - 1)
                {
                    return null;
                }

                return Items[SelectedIndex];
            }
            set
            {
                if (Selection == value) return;

                SelectedIndex = Items.IndexOf(value);
            }
        }


        protected ElementItemList()
        {
            _scrollView = new ScrollView
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    flexGrow = 1.0f
                }
            };

            _scrollView.contentContainer.style.left = 0;
            _scrollView.contentContainer.style.right = 0;

            Add(_scrollView);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        public void Reload(IReadOnlyCollection<T> itemList)
        {
            _scrollView.Clear();
            Items.Clear();

            foreach (var item in itemList)
            {
                Add(item);
            }

            if (_selectIndex < 0 || _selectIndex >= itemList.Count)
            {
                _selectIndex = itemList.Any() ? 0 : -1;
            }

            SelectedIndex = _selectIndex;
        }

        public void Add(T item)
        {
            item.RegisterCallback<MouseDownEvent>(OnItemMouseDown);
            item.RegisterCallback<MouseUpEvent>(OnItemMouseUp);

            _scrollView.Add(item);
            Items.Add(item);
        }

        public void Remove(T item)
        {
            var isSelected = Selection == item;

            _scrollView.Remove(item);
            Items.Remove(item);

            if (isSelected)
            {
                SelectedIndex = Math.Min(SelectedIndex, Items.Count - 1);
            }
        }

        private bool HasFocus()
        {
            return focusController.focusedElement == this;
        }

        private void OnItemMouseDown(MouseDownEvent evt)
        {
            if (evt.clickCount != 1) return;
            if (evt.button != (int) MouseButton.LeftMouse && evt.button != (int) MouseButton.RightMouse) return;

            var item = (T) evt.currentTarget;

            if (evt.modifiers == EventModifiers.None)
            {
                var alreadySelected = Selection == item;
                if (evt.button == (int) MouseButton.LeftMouse && alreadySelected)
                {
                    if (HasFocus()) OnItemRename?.Invoke(item);
                }
                else
                {
                    Selection = item;
                }
            }

            evt.StopImmediatePropagation();
        }

        private void OnItemMouseUp(MouseUpEvent evt)
        {
            if (evt.clickCount != 1) return;
            if (evt.modifiers != EventModifiers.None || evt.button != (int) MouseButton.RightMouse) return;

            OnItemContextMenu?.Invoke((T) evt.currentTarget);

            evt.StopImmediatePropagation();
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (evt.clickCount != 1) return;

            if (evt.button == (int) MouseButton.RightMouse)
                OnContextMenu?.Invoke();

            evt.StopImmediatePropagation();
        }
    }
}
