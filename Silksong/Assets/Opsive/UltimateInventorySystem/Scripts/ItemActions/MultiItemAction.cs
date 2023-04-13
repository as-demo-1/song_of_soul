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
    /// An item Action that lets you use multiple ItemActions
    /// </summary>
    [System.Serializable]
    public class MultiItemAction : ItemAction
    {
        [SerializeField] protected bool m_AllConditionsMustPass;
        [SerializeField] protected ItemActionSet m_ItemActionSet;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MultiItemAction()
        {
            m_Name = "Use";
        }

        /// <summary>
        /// True if the item is not null.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        /// <returns>True if item is not null.</returns>
        protected override bool CanInvokeInternal(ItemInfo itemInfo, ItemUser itemUser)
        {
            if (m_AllConditionsMustPass == false) {
                return true;
            }
            
            m_ItemActionSet.ItemActionCollection.Initialize(false);
            for (int i = 0; i < m_ItemActionSet.ItemActionCollection.Count; i++) {
                var canInvoke = m_ItemActionSet.ItemActionCollection[i].CanInvoke(itemInfo, itemUser);
                if (canInvoke == false) {
                    return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// prints all the attributes of the item.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        protected override void InvokeActionInternal(ItemInfo itemInfo, ItemUser itemUser)
        {
            for (int i = 0; i < m_ItemActionSet.ItemActionCollection.Count; i++) {
                m_ItemActionSet.ItemActionCollection[i].InvokeAction(itemInfo, itemUser);
            }
        }
    }
}