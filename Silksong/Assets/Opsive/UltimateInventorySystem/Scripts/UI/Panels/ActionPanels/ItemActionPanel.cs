/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.ActionPanels
{
    using System;
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.ItemActions;
    using Opsive.UltimateInventorySystem.UI.Item;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The item action panel.
    /// </summary>
    public class ItemActionPanel : ActionPanel<ItemAction>
    {
        [Tooltip("Hide Items which cannot be invoked.")]
        [SerializeField] protected bool m_HideCannotInvokeActions;
        
        protected List<ItemAction> m_ItemActions;

        protected ItemInfo m_ItemInfo;
        protected ItemUser m_ItemUser;
        protected ItemViewSlotsContainerBase m_ItemViewSlotsContainer;

        /// <summary>
        /// Assign a list of item actions.
        /// </summary>
        /// <param name="itemActions">The item actions.</param>
        /// <param name="itemInfo">The item.</param>
        /// <param name="itemUser">The item user.</param>
        /// <param name="itemViewSlotsContainer">The item View slot Container.</param>
        /// <param name="itemViewSlotIndex">The index of the Item View Slot selected.</param>
        public void AssignActions(ListSlice<ItemAction> itemActions, ItemInfo itemInfo,
            ItemUser itemUser, ItemViewSlotsContainerBase itemViewSlotsContainer, int itemViewSlotIndex)
        {

            m_ItemInfo = itemInfo;
            m_ItemUser = itemUser;

            m_ItemViewSlotsContainer = itemViewSlotsContainer;
            if (m_ItemActions == null) { m_ItemActions = new List<ItemAction>(); }

            m_ItemActions.Clear();
            for (int i = 0; i < itemActions.Count; i++) {
                var itemAction = itemActions[i];
                
                itemAction.Setup(itemInfo, itemUser);
                itemAction.Initialize(false);
                
                if (itemAction is ItemViewSlotsContainerItemAction itemActionWithContainer) {
                    itemActionWithContainer.SetViewSlotsContainer(m_ItemViewSlotsContainer, itemViewSlotIndex);
                }
                
                if (m_HideCannotInvokeActions) {
                    if (itemAction.CanInvoke(itemInfo, itemUser) == false) {
                        continue;
                    }
                }
                
                m_ItemActions.Add(itemAction);
            }

            AssignActions(m_ItemActions);
        }

        /// <summary>
        /// Invoke the action.
        /// </summary>
        /// <param name="index">The action index.</param>
        protected override void InvokeActionInternal(int index)
        {
            var itemAction = m_ItemActions[index];
            itemAction.InvokeAction(m_ItemInfo, m_ItemUser);
        }
    }
}