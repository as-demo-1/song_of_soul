/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.ItemActions
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;

    /// <summary>
    /// Simple item action used to duplicate an item.
    /// </summary>
    [System.Serializable]
    public class DuplicateItemAction : ItemAction
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public DuplicateItemAction()
        {
            m_Name = "Duplicate";
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
            itemInfo = (1, itemInfo);

            itemInfo.ItemCollection?.AddItem(itemInfo);
        }
    }
}