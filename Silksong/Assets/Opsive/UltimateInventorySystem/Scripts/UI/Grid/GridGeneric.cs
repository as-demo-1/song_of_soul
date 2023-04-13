/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Grid
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Input;
    using Opsive.UltimateInventorySystem.UI.Views;
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;

    /// <summary>
    /// The Generic abstract class used to create grids.
    /// </summary>
    /// <typeparam name="T">The grid element type.</typeparam>
    public abstract class GridGeneric<T> : GridBase
    {
        public event Action<T, int> OnElementSelected;
        public event Action<T, int> OnElementClicked;
        public event Action<int> OnEmptySelected;
        public event Action<int> OnEmptyClicked;
        public event Action<T, int, PointerEventData> OnElementPointerDownE;
        public event Action<T, int, PointerEventData> OnElementBeginDragE;
        public event Action<T, int, PointerEventData> OnElementEndDragE;
        public event Action<T, int, PointerEventData> OnElementDragE;
        public event Action<T, T, int, PointerEventData> OnElementDropE;

        [Tooltip("The filter sorter.")]
        [SerializeField] protected internal FilterSorter m_FilterSorterBase;

        protected ViewDrawer<T> m_ViewDrawer;
        protected ResizableArray<T> m_Elements;
        protected IFilterSorter<T> m_FilterSorter;

        public ViewDrawer<T> ViewDrawer => m_ViewDrawer;

        public ResizableArray<T> Elements => m_Elements;
        public override int ElementCount => m_Elements?.Count ?? 0;
        public IFilterSorter<T> FilterSorter => m_FilterSorter;

        /// <summary>
        /// Initialize the grid.
        /// </summary>
        public override void Initialize(bool force)
        {
            if (m_IsInitialized && !force) { return; }
            base.Initialize(force);

            m_Elements = new ResizableArray<T>();
            m_ViewDrawer = m_ViewDrawerBase as ViewDrawer<T>;
            if (m_ViewDrawer == null) {
                m_ViewDrawer = GetComponent<ViewDrawer<T>>();
                if (m_ViewDrawer == null) {
                    Debug.LogWarning($"Grid Box Drawer is null for {m_ParentPanel}.");
                }
            }
            m_ViewDrawer.Initialize(false);

            if (m_FilterSorterBase != null) {
                if (m_FilterSorterBase is IFilterSorter<T> gridFilter) {
                    BindGridFilterSorter(gridFilter);
                } else {
                    Debug.LogWarning("The grid filter assigned does not match the generic type.", gameObject);
                }
            }

        }

        /// <summary>
        /// Get the element of the grid at index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The element.</returns>
        public T GetElementAt(int index)
        {
            return m_Elements[index];
        }

        /// <summary>
        /// Get the element of the grid at index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The element.</returns>
        public View<T> GetViewAt(int index)
        {
            if (index < 0 || index >= m_ViewDrawer.Views.Count) { return null; }
            return m_ViewDrawer.Views[index];
        }

        /// <summary>
        /// Get the selected element.
        /// </summary>
        /// <returns>The selected element.</returns>
        public T GetSelectedElement()
        {
            var selectedIndex = GridEventSystem.GetSelectedButtonIndex();
            if (selectedIndex < 0 || selectedIndex >= m_Elements.Count) { return default; }
            return GetElementAt(selectedIndex);
        }

        /// <summary>
        /// Get the selected View.
        /// </summary>
        /// <returns>The selected View.</returns>
        public View<T> GetSelectedView()
        {
            var selectedIndex = GridEventSystem.GetSelectedButtonIndex();
            return GetViewAt(selectedIndex);
        }

        /// <summary>
        /// Try and get an element at the index provided.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="element">The element output.</param>
        /// <returns>True if it exists.</returns>
        protected virtual bool TryGetElementAt(int index, out T element)
        {
            if (m_Elements.Count <= StartIndex + index) {
                element = default;
                return false;
            }

            element = GetElementAt(StartIndex + index);
            return true;
        }

        /// <summary>
        /// Select the view at index.
        /// </summary>
        /// <param name="index">The index.</param>
        protected override void ViewSelected(int index)
        {
            m_ViewDrawer.Select(index);

            if (TryGetElementAt(index, out var element)) {
                OnElementSelected?.Invoke(element, index);
                return;
            }

            OnEmptySelected?.Invoke(index);
        }

        /// <summary>
        /// Click the box at the index.
        /// </summary>
        /// <param name="index">The index.</param>
        protected override void ViewClicked(int index)
        {
            m_ViewDrawer.Click(index);

            if (m_Elements.Count <= StartIndex + index) {
                OnEmptyClicked?.Invoke(index);
                return;
            }

            var selectedElements = GetElementAt(StartIndex + index);

            OnElementClicked?.Invoke(selectedElements, index);
        }

        /// <summary>
        /// Handle Pointer down on button.
        /// </summary>
        /// <param name="index">The button index.</param>
        /// <param name="eventData">The event data.</param>
        protected override void ViewOnPointerDown(int index, PointerEventData eventData)
        {
            if (m_Elements.Count <= StartIndex + index) {
                return;
            }

            var selectedElement = GetElementAt(StartIndex + index);

            OnElementPointerDown(selectedElement, index, eventData);
        }

        /// <summary>
        /// Handle an element being pointer downed.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="index">the button index.</param>
        /// <param name="eventData">The event data.</param>
        protected virtual void OnElementPointerDown(T element, int index, PointerEventData eventData)
        {
            OnElementPointerDownE?.Invoke(element, index, eventData);
        }

        /// <summary>
        /// Handle a button begin drag.
        /// </summary>
        /// <param name="index">The button index.</param>
        /// <param name="eventData">The event data.</param>
        protected override void ViewBeginDrag(int index, PointerEventData eventData)
        {
            if (m_Elements.Count <= StartIndex + index) {
                return;
            }

            var selectedElements = GetElementAt(StartIndex + index);

            OnElementBeginDrag(selectedElements, index, eventData);
        }

        /// <summary>
        /// Handle an element being dragged.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="index">the button index.</param>
        /// <param name="eventData">The event data.</param>
        protected virtual void OnElementBeginDrag(T element, int index, PointerEventData eventData)
        {
            OnElementBeginDragE?.Invoke(element, index, eventData);
        }

        /// <summary>
        /// Handle button ending the drag.
        /// </summary>
        /// <param name="index">The button index.</param>
        /// <param name="eventData">The event data.</param>
        protected override void ViewEndDrag(int index, PointerEventData eventData)
        {
            if (m_Elements.Count <= StartIndex + index) {
                OnElementEndDrag(default, index, eventData);
                return;
            }

            var selectedElements = GetElementAt(StartIndex + index);

            OnElementEndDrag(selectedElements, index, eventData);
        }

        /// <summary>
        /// Handle an element ending the dragged.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="index">the button index.</param>
        /// <param name="eventData">The event data.</param>
        protected virtual void OnElementEndDrag(T element, int index, PointerEventData eventData)
        {
            OnElementEndDragE?.Invoke(element, index, eventData);
        }

        /// <summary>
        /// Handle button being dragged.
        /// </summary>
        /// <param name="index">The button index.</param>
        /// <param name="eventData">The event data.</param>
        protected override void ViewDrag(int index, PointerEventData eventData)
        {
            if (m_Elements.Count <= StartIndex + index) {
                return;
            }

            var selectedElement = GetElementAt(StartIndex + index);

            OnElementDrag(selectedElement, index, eventData);
        }

        /// <summary>
        /// Handle an element being dragged.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="index">the button index.</param>
        /// <param name="eventData">The event data.</param>
        protected virtual void OnElementDrag(T element, int index, PointerEventData eventData)
        {
            OnElementDragE?.Invoke(element, index, eventData);
        }

        /// <summary>
        /// Handle button being dropped.
        /// </summary>
        /// <param name="index">The button index.</param>
        /// <param name="eventData">The event data.</param>
        protected override void ViewDrop(int index, PointerEventData eventData)
        {
            var droppedObject = eventData.pointerDrag;
            if (droppedObject == null) { return; }

            var droppedBoxObject = droppedObject.GetComponent<View<T>>();
            if (droppedBoxObject == null) {
                var boxParent = droppedObject.GetComponent<IViewSlot>();
                if (boxParent == null) { return; }

                droppedBoxObject = boxParent.View as View<T>;
                if (droppedBoxObject == null) { return; }
            }

            var droppedElement = droppedBoxObject.CurrentValue;

            if (m_Elements.Count <= StartIndex + index) {
                OnElementDrop(droppedElement, default, index, eventData);
                return;
            }

            var selectedElement = GetElementAt(StartIndex + index);

            OnElementDrop(droppedElement, selectedElement, index, eventData);
        }

        /// <summary>
        /// Handle an element being dropped.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="index">the button index.</param>
        /// <param name="eventData">The event data.</param>
        protected virtual void OnElementDrop(T elementDropped, T currentElement, int index, PointerEventData eventData)
        {
            OnElementDropE?.Invoke(elementDropped, currentElement, index, eventData);
        }

        /// <summary>
        /// Draw the box using the box drawer.
        /// </summary>
        protected override void DrawInternal()
        {
            var startIndex = StartIndex;
            var endIndex = EndIndex;

            m_ViewDrawer.DrawViews(startIndex, endIndex, m_Elements);

            var selectedButton = m_GridEventSystem.GetSelectedButton();
            if (selectedButton != null) {
                selectedButton.Select();
            }
        }

        /// <summary>
        /// Set the elements that should be drawn
        /// </summary>
        /// <param name="newElements">The elements.</param>
        public virtual void SetElements(ListSlice<T> newElements, bool ignoreSorterFilter = false)
        {
            m_Elements.Clear();

            if (ignoreSorterFilter == false && m_FilterSorter != null) {
                var pooledArray = GenericObjectPool.Get<T[]>();
                newElements = m_FilterSorter.Filter(newElements, ref pooledArray);
                GenericObjectPool.Return(pooledArray);
            }

            for (int i = 0; i < newElements.Count; i++) {
                m_Elements.Add(newElements[i]);
            }

            ElementsUpdated();
        }

        /// <summary>
        /// Set the grid filter.
        /// </summary>
        /// <param name="filterSorter">The grid filter.</param>
        /// <returns>Return the previous grid filter.</returns>
        public IFilterSorter<T> BindGridFilterSorter(IFilterSorter<T> filterSorter)
        {
            var previousFilter = m_FilterSorter;
            m_FilterSorter = filterSorter;
            return previousFilter;
        }

    }
}