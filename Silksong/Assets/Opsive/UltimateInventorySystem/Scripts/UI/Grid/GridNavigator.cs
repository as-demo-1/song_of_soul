/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Grid
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public delegate void IndexChangeEvent(int previousIndex, int nextIndex);

    [Serializable]
    public enum SlicerStepOption
    {
        Page,       //The step moves by the grid size amount.
        Vertical,   //The step moves row by row.
        Horizontal, //The step moves column by column.
        Amount      //The step moves by a constant amount.
    }

    /// <summary>
    /// A component used to virtually scroll through a grid.
    /// </summary>
    public class GridNavigator : GridNavigatorBase
    {
        [Tooltip("The step option to choose how the items are navigated through.")]
        [SerializeField] protected SlicerStepOption m_StepOption;
        [Tooltip("The custom amount, only used when the 'Amount' slicer step option is used.")]
        [SerializeField] protected int m_CustomAmount;

        protected int m_Step;

        protected override int Step => m_Step != 0 ? m_Step : GetStep();

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="force">Force the initialize.</param>
        public override void Initialize(bool force)
        {
            if (m_IsInitialized && force == false) { return; }

            m_Step = GetStep();

            base.Initialize(force);
        }

        /// <summary>
        /// Get the step, the amount of elements that are available per index.
        /// </summary>
        /// <returns>The step amount.</returns>
        protected int GetStep()
        {
            switch (m_StepOption) {
                case SlicerStepOption.Page:
                    return m_Grid.GridSizeCount;
                case SlicerStepOption.Vertical:
                    return m_Grid.GridSize.x;
                case SlicerStepOption.Horizontal:
                    return m_Grid.GridSize.y;
                case SlicerStepOption.Amount:
                    return m_CustomAmount;
            }

            return 0;
        }

        /// <summary>
        /// Go to the next slice.
        /// </summary>
        /// <returns>True if it was able to move slice.</returns>
        public override bool NextSlice()
        {
            var result = base.NextSlice();

            if (result == false) { return false; }

            //Set the selected view slot opposite to the current one.
            if (m_StepOption == SlicerStepOption.Page) {
                if (m_Grid.GridLayoutGroup.startAxis == GridLayoutGroup.Axis.Horizontal) {
                    m_Grid.GridEventSystem.SelectColumnButton(false);
                } else {
                    m_Grid.GridEventSystem.SelectRowButton(false);
                }
            }

            return result;
        }

        /// <summary>
        /// Previous slice.
        /// </summary>
        /// <returns>True if it was able to go back a slice.</returns>
        public override bool PreviousSlice()
        {
            var result = base.PreviousSlice();

            if (result == false) { return false; }

            //Set the selected view slot opposite to the current one.
            if (m_StepOption == SlicerStepOption.Page) {
                if (m_Grid.GridLayoutGroup.startAxis == GridLayoutGroup.Axis.Horizontal) {
                    m_Grid.GridEventSystem.SelectColumnButton(true);
                } else {
                    m_Grid.GridEventSystem.SelectRowButton(true);
                }
            }

            return result;
        }
    }
}