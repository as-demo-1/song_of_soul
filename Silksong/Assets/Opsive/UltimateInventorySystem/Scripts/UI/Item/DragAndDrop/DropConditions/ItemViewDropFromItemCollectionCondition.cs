/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.DragAndDrop.DropConditions
{
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using System;
    using UnityEngine;

    /// <summary>
    /// The Item View Drop From Item Collection Condition.
    /// </summary>
    [Serializable]
    public class ItemViewDropFromItemCollectionCondition : ItemViewDropCondition
    {
        [Tooltip("The source item collection id.")]
        [SerializeField] protected ItemCollectionID m_SourceItemCollectionID;

        /// <summary>
        /// Can the item be dropped.
        /// </summary>
        /// <param name="itemViewDropHandler">The Item View Drop Handler.</param>
        /// <returns>True if it can be dropped.</returns>
        public override bool CanDrop(ItemViewDropHandler itemViewDropHandler)
        {
            return m_SourceItemCollectionID.Compare(itemViewDropHandler.SourceItemInfo.ItemCollection);
        }

        /// <summary>
        /// To String.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return base.ToString() + string.Format("[{0}]", m_SourceItemCollectionID);
        }
    }

}