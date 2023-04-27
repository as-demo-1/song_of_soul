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
    /// The Item View Drop Same Container Condition.
    /// </summary>
    [Serializable]
    public class ItemViewDropSameContainerCondition : ItemViewDropCondition
    {
        [Tooltip("Are the source and destination the same Container?")]
        [SerializeField] protected bool m_Same;

        /// <summary>
        /// Can the item be dropped.
        /// </summary>
        /// <param name="itemViewDropHandler">The Item View Drop Handler.</param>
        /// <returns>True if it can be dropped.</returns>
        public override bool CanDrop(ItemViewDropHandler itemViewDropHandler)
        {
            var same = itemViewDropHandler.SourceContainer == itemViewDropHandler.DestinationContainer;
            return same == m_Same;
        }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return $"Same Container [{m_Same}]";
        }
    }
}