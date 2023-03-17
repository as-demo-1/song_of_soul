/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.DragAndDrop.DropConditions
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Condition to dop an Item depending on its amount.
    /// </summary>
    [Serializable]
    public class ItemViewDropItemAmountCondition : ItemViewDropCondition
    {
        [Tooltip("The minimum or maximum amount (inclusive) allowed.")]
        [SerializeField] protected int m_SourceAmount = 1;
        [Tooltip("Is the amount above the minimum amount or the maximum amount.")]
        [SerializeField] protected bool m_MinAmount = true;

        /// <summary>
        /// Can the item be dropped.
        /// </summary>
        /// <param name="itemViewDropHandler">The Item View Drop Handler.</param>
        /// <returns>True if it can be dropped.</returns>
        public override bool CanDrop(ItemViewDropHandler itemViewDropHandler)
        {
            var amount = itemViewDropHandler.SourceItemInfo.Amount;

            if (m_MinAmount && (amount < m_SourceAmount)) {
                return false;
            }

            if (!m_MinAmount && (amount > m_SourceAmount)) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            var minMax = m_MinAmount ? "Min" : "Max;";
            return $"{minMax}_Amount[{m_SourceAmount}]";
        }
    }
}