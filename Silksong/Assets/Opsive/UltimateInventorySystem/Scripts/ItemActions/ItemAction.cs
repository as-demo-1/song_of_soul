/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.ItemActions
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using UnityEngine;

    /// <summary>
    /// Abstract class to create actions that can be invoked on Items.
    /// </summary>
    [System.Serializable]
    public abstract class ItemAction : ActionElement, IItemUserAction
    {
        protected ItemInfo m_BoundItemInfo;
        protected ItemUser m_BoundItemUser;

        public ItemInfo BoundItemInfo => m_BoundItemInfo;
        public ItemUser BoundItemUser => m_BoundItemUser ?? m_BoundItemInfo.Inventory?.ItemUser;

        /// <summary>
        /// Setup the item and the inventory needed to perform the action
        /// </summary>
        /// <param name="itemInfo">The item Info.</param>
        /// <param name="itemUser">The item user.</param>
        public virtual void Setup(ItemInfo itemInfo, ItemUser itemUser)
        {
            m_BoundItemInfo = itemInfo;
            m_BoundItemUser = itemUser;
        }

        /// <summary>
        /// Can the item action be invoked with the item info and the item user. 
        /// </summary>
        /// <returns>True if it can be invoked.</returns>
        protected override bool CanInvokeInternal()
        {
            return CanInvokeInternal(m_BoundItemInfo, BoundItemUser);
        }

        /// <summary>
        /// Checks if the action can be invoked on the item.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        /// <returns>True if the item can be used.</returns>
        public virtual bool CanInvoke(ItemInfo itemInfo, ItemUser itemUser)
        {
            if (m_Initialized == false) {
                Debug.LogWarning($"The action {GetType().Name} is not initialized.");
                return false;
            }

            if (itemUser == null && itemInfo.Inventory != null) { itemUser = itemInfo.Inventory.ItemUser; }

            return CanInvokeInternal(itemInfo, itemUser);
        }

        /// <summary>
        /// Checks if the action can be invoked on the item.
        /// </summary>
        /// <param name="itemInfo">The item.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        /// <returns>True if the item can be used.</returns>
        protected abstract bool CanInvokeInternal(ItemInfo itemInfo, ItemUser itemUser);

        /// <summary>
        /// Invoke the item action with the itemInfo and itemUser set up.
        /// </summary>
        public override void InvokeAction()
        {
            InvokeAction(m_BoundItemInfo, BoundItemUser);
        }

        /// <summary>
        /// Invoke the item action with the itemInfo and itemUser set up.
        /// </summary>
        protected override void InvokeActionInternal()
        {
            InvokeActionInternal(m_BoundItemInfo, BoundItemUser);
        }

        /// <summary>
        /// Invoke the action on the item that is inside the inventory after it has been checked that it can be invoked.
        /// </summary>
        /// <param name="itemInfo">The item.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        public virtual void InvokeAction(ItemInfo itemInfo, ItemUser itemUser)
        {
            if (itemUser == null && itemInfo.Inventory != null) { itemUser = itemInfo.Inventory.ItemUser; }

            if (CanInvoke(itemInfo, itemUser) == false) { return; }

            InvokeActionInternal(itemInfo, itemUser);
        }

        /// <summary>
        /// Internal Invoke, is used by the default InvokeAction.
        /// </summary>
        /// <param name="itemInfo">The item.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        protected abstract void InvokeActionInternal(ItemInfo itemInfo, ItemUser itemUser);
    }
}
