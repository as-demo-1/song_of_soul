/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.DragAndDrop.DropActions
{
    using Opsive.UltimateInventorySystem.ItemActions;
    using System;
    using UnityEngine;

    /// <summary>
    /// An Item View Drop Action that allows you to use an Item Action.
    /// </summary>
    [Serializable]
    public class ItemViewDropActionToItemAction : ItemViewDropAction
    {
        [Tooltip("The action index.")]
        [SerializeField] protected int m_ActionIndex;
        [Tooltip("The item action set.")]
        [SerializeField] protected ItemActionSet m_ItemActionSet;

        /// <summary>
        /// Drop Action.
        /// </summary>
        /// <param name="itemViewDropHandler">The Item View Drop Handler.</param>
        public override void Drop(ItemViewDropHandler itemViewDropHandler)
        {
            if (m_ItemActionSet == null) {
                return;
            }

            var itemInfo = itemViewDropHandler.SourceItemInfo;
            if (m_ItemActionSet.MatchItem(itemInfo.Item) == false) {
                return;
            }

            m_ItemActionSet.ItemActionCollection.Initialize(false);

            if (m_ActionIndex < 0 || m_ActionIndex >= m_ItemActionSet.ItemActionCollection.Count) {
                Debug.LogWarning($"The index {m_ActionIndex} is out of range for the item action set Drop action.");
            }

            var itemAction = m_ItemActionSet.ItemActionCollection[m_ActionIndex];

            itemAction.Initialize(false);

            ItemUser itemUser = null;
            if (itemViewDropHandler.SourceContainer != null) {
                itemUser = itemViewDropHandler.SourceContainer.Inventory.ItemUser;
            }

            itemAction.InvokeAction(itemInfo, itemUser);
        }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return "To Item Action";
        }
    }
}