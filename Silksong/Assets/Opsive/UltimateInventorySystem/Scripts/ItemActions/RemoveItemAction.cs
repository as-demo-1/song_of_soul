/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.ItemActions
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using UnityEngine;

    /// <summary>
    /// Simple item action used to remove items.
    /// </summary>
    [System.Serializable]
    public class RemoveItemAction : ItemAction
    {
        [Tooltip("If the value is 0 or lower the action will remove the item completely.")]
        [SerializeField] protected int m_RemoveAmount = -1;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RemoveItemAction()
        {
            m_Name = "Remove";
        }

        /// <summary>
        /// Check if the action can be invoked.
        /// </summary>
        /// <param name="itemInfo">The item.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        /// <returns>True if the action can be invoked.</returns>
        protected override bool CanInvokeInternal(ItemInfo itemInfo, ItemUser itemUser)
        {
            return true;
        }

        /// <summary>
        /// Invoke the action.
        /// </summary>
        /// <param name="itemInfo">The item.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        protected override void InvokeActionInternal(ItemInfo itemInfo, ItemUser itemUser)
        {
            if (m_RemoveAmount >= 1) { itemInfo = (Mathf.Min(m_RemoveAmount, itemInfo.Amount), itemInfo); }

            itemInfo.ItemCollection?.RemoveItem(itemInfo);
        }
    }
}