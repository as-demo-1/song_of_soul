/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Grid
{
    using Opsive.UltimateInventorySystem.UI.CompoundElements;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Inventory list UI adds a scrollbar to an inventory grid ui.
    /// </summary>
    public class GridNavigatorWithScrollView : GridNavigatorBase
    {
        [Tooltip("The scroll bar with buttons.")]
        [SerializeField] internal ScrollbarWithButtons m_Scrollbar;
        [Tooltip("The content transform with the grid elements.")]
        [SerializeField] internal RectTransform m_Content;
        [Tooltip("The scroller is horizontal or vertical?")]
        [SerializeField] internal bool m_Vertical = true;
        [Tooltip("The grid element size in the scroll axis + the layout space in that axis, if 0 or less the grid element size is computed from the Grid Layout Group.")]
        [SerializeField] protected float m_GridElementSize = -1;

        protected override int Step => m_Vertical ? m_Grid.GridSize.x : m_Grid.GridSize.y;

        Vector2 MinAnchorPoint => Vector2.zero;
        Vector2 MaxAnchorPoint => new Vector2(m_Vertical ? 0 : -m_GridElementSize, m_Vertical ? m_GridElementSize : 0);

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="force">Force Initialize.</param>
        public override void Initialize(bool force)
        {
            if (m_IsInitialized && force == false) { return; }

            m_Scrollbar.DiscreteSteps = false;
            m_Scrollbar.OnScrollIndexChanged += ScrollIndexChanged;
            m_Scrollbar.OnScrollValueChanged += ScrollValueChanged;

            // Make the buttons null so that they do not listen to the click event
            m_NextButton = null;
            m_PreviousButton = null;

            base.Initialize(force);

            // Assign the scroll buttons to hide them when unusable.
            if (m_NextButton == null) { m_NextButton = m_Scrollbar.PositiveButton; }
            if (m_PreviousButton == null) { m_PreviousButton = m_Scrollbar.NegativeButton; }

            // Set the grid element size by checking the grid layout
            if (m_GridElementSize <= 0) {
                var gridLayout = m_Grid.GridLayoutGroup;
                if (gridLayout == null) {
                    Debug.LogWarning("Cannot compute Grid Elements Size because the Grid Layout Group component is missing", gameObject);
                    m_GridElementSize = 135;//Default value.
                    return;
                }

                if (m_Vertical == false) {
                    m_GridElementSize = gridLayout.cellSize.x + gridLayout.spacing.x;
                } else {
                    m_GridElementSize = gridLayout.cellSize.y + gridLayout.spacing.y;
                }
            }
        }

        /// <summary>
        /// Register the grid system movements.
        /// </summary>
        protected override void RegisterGridSystemMoves()
        {
            m_Grid.GridEventSystem.OnGridElementSelectedE += HandleGridElementSelected;
            if (m_Vertical) {
                m_Grid.GridEventSystem.OnUnavailableNavigationUp += () => PreviousSlice();
                m_Grid.GridEventSystem.OnUnavailableNavigationDown += () => NextSlice();
            } else {
                m_Grid.GridEventSystem.OnUnavailableNavigationLeft += () => PreviousSlice();
                m_Grid.GridEventSystem.OnUnavailableNavigationRight += () => NextSlice();
            }
        }

        /// <summary>
        /// Handle grid element being selected.
        /// </summary>
        /// <param name="index">The index of the selected element.</param>
        private void HandleGridElementSelected(int index)
        {
            var col = index % m_Grid.GridSize.x;
            var row = index / m_Grid.GridSize.x;

            var offset = m_Vertical ? m_Content.anchoredPosition.y : m_Content.anchoredPosition.x;

            if (offset < m_GridElementSize / 2f) {

                if (m_Vertical && row == m_Grid.GridSize.y - 1
                    || !m_Vertical && col == m_Grid.GridSize.x - 1) {

                    m_Content.anchoredPosition = MaxAnchorPoint;

                    var oneStep = 1f / (float)(MaxSliceIndex + 1);
                    var diff = (m_GridElementSize - offset) / m_GridElementSize;
                    var scrollOffset = oneStep * diff;
                    var newScrollValue = Mathf.Clamp(m_Scrollbar.Scrollbar.value + scrollOffset, 0f, 1f);

                    m_Scrollbar.Scrollbar.SetValueWithoutNotify(newScrollValue);
                }

            } else if (offset > m_GridElementSize / 2f) {

                if (m_Vertical && row == 0 || !m_Vertical && col == 0) {

                    m_Content.anchoredPosition = MinAnchorPoint;

                    var oneStep = 1f / (float)(MaxSliceIndex + 1);
                    var diff = -offset / m_GridElementSize;
                    var scrollOffset = oneStep * diff;
                    var newScrollValue = Mathf.Clamp(m_Scrollbar.Scrollbar.value + scrollOffset, 0f, 1f);

                    m_Scrollbar.Scrollbar.SetValueWithoutNotify(newScrollValue);
                }
            }
        }

        /// <summary>
        /// The scroll value was changed.
        /// </summary>
        /// <param name="value">The scroll value.</param>
        private void ScrollValueChanged(float value)
        {
            var index = m_Index / (float)(MaxSliceIndex + 1);
            var normalizedOffset = value - index;
            var offset = normalizedOffset * m_GridElementSize * (float)(MaxSliceIndex + 1);

            m_Content.anchoredPosition = new Vector2(
                m_Vertical ? 0 : -offset,
                m_Vertical ? offset : 0);
        }

        /// <summary>
        /// Change the scroll value.
        /// </summary>
        /// <param name="index">The new index.</param>
        protected virtual void ScrollIndexChanged(int index)
        {
            if (index == 0) {
                if (m_Content.anchoredPosition == MaxAnchorPoint) {
                    m_Content.anchoredPosition = MinAnchorPoint;
                    index += 1;
                }
            }

            if (index == MaxSliceIndex) {
                if (m_Content.anchoredPosition == MinAnchorPoint) {
                    m_Content.anchoredPosition = MaxAnchorPoint;
                    index -= 1;
                }
            }

            SetIndexInternal(index);
        }

        /// <summary>
        /// Refresh the arrow buttons.
        /// </summary>
        public override void RefreshArrows()
        {
            base.RefreshArrows();

            // Set the scroll bar size.
            m_Scrollbar.SetupScrollbar(MaxSliceIndex + 1);
            // Set the index of the scroll bar and notify the scroll index change.
            m_Scrollbar.SetScrollStep(m_Index, true, true);

            // Set the scroll index taking into accoun the anchor offset.
            var indexWithOffset = m_Index + (m_Content.anchoredPosition == MinAnchorPoint ? 0 : 1);
            m_Scrollbar.Scrollbar.SetValueWithoutNotify(indexWithOffset / (float)(MaxSliceIndex + 1));
        }
    }
}