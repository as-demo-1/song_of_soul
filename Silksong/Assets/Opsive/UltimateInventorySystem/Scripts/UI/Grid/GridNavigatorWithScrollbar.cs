/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Grid
{
    using Opsive.UltimateInventorySystem.UI.CompoundElements;
    using UnityEngine;

    /// <summary>
    /// Inventory list UI adds a scrollbar to an inventory grid ui.
    /// </summary>
    public class GridNavigatorWithScrollbar : GridNavigatorBase
    {
        [Tooltip("The scroll bar with buttons.")]
        [SerializeField] internal ScrollbarWithButtons m_Scrollbar;
        [Tooltip("The scroller is horizontal or vertical?")]
        [SerializeField] internal bool m_Vertical = true;

        protected override int Step => m_Vertical ? m_Grid.GridSize.x : m_Grid.GridSize.y;

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="force">Force Intialize.</param>
        public override void Initialize(bool force)
        {
            if (m_IsInitialized && force == false) { return; }

            m_Scrollbar.OnScrollIndexChanged += ScrollIndexChanged;

            // Make the buttons null so that they do not listen to the click event
            m_NextButton = null;
            m_PreviousButton = null;

            base.Initialize(force);

            // Assign the scroll buttons to hide them when unusable.
            if (m_NextButton == null) { m_NextButton = m_Scrollbar.PositiveButton; }
            if (m_PreviousButton == null) { m_PreviousButton = m_Scrollbar.NegativeButton; }
        }

        /// <summary>
        /// Register to the grid system movements.
        /// </summary>
        protected override void RegisterGridSystemMoves()
        {
            if (m_Vertical) {
                m_Grid.GridEventSystem.OnUnavailableNavigationUp += () => PreviousSlice();
                m_Grid.GridEventSystem.OnUnavailableNavigationDown += () => NextSlice();
            } else {
                m_Grid.GridEventSystem.OnUnavailableNavigationLeft += () => PreviousSlice();
                m_Grid.GridEventSystem.OnUnavailableNavigationRight += () => NextSlice();
            }

        }

        /// <summary>
        /// Change the scroll value.
        /// </summary>
        /// <param name="index">The new index.</param>
        protected virtual void ScrollIndexChanged(int index)
        {
            SetIndexInternal(index);
        }

        /// <summary>
        /// Refresh the arrows.
        /// </summary>
        public override void RefreshArrows()
        {
            base.RefreshArrows();

            m_Scrollbar.SetupScrollbar(MaxSliceIndex);
            m_Scrollbar.SetScrollStep(m_Index, true, false);
        }
    }
}