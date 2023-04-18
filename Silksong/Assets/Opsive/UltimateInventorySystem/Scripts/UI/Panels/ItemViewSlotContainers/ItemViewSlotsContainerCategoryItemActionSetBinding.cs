/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.ItemActions;
    using Opsive.UltimateInventorySystem.UI.DataContainers;
    using UnityEngine;

    public class ItemViewSlotsContainerCategoryItemActionSetBinding : ItemViewSlotsContainerItemActionBindingBase
    {
        [Tooltip("The categories item actions. Specifies the actions that can be performed on each item. Can be null.")]
        [SerializeField] public CategoryItemActionSet m_CategoryItemActionSet;
        [Tooltip("The maximum amount of actions displayed at one time to set the action array size.")]
        [SerializeField] protected int m_MaxNumberOfActions = 5;

        protected ItemAction[] m_ItemActionArray;

        public CategoryItemActionSet CategoryItemActionSet => m_CategoryItemActionSet;

        /// <summary>
        /// Set the Item Action Set.
        /// </summary>
        /// <param name="categoryItemActionSet">The ITem Action Set to set.</param>
        public void SetCategoryItemActionSet(CategoryItemActionSet categoryItemActionSet)
        {
            m_CategoryItemActionSet = categoryItemActionSet;
        }
        
        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="force">Force Initialize.</param>
        public override void Initialize(bool force)
        {
            if (m_IsInitialized && force == false) { return; }
            base.Initialize(force);
        }

        /// <summary>
        /// Initialize the item action list.
        /// </summary>
        /// <returns>The item action list slice.</returns>
        protected override ListSlice<ItemAction> InitializeItemActionList()
        {
            m_ItemActionArray = new ItemAction[m_MaxNumberOfActions];
            return m_ItemActionArray;
        }

        /// <summary>
        /// Refresh the item actions.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        protected virtual void RefreshItemActions(ItemInfo itemInfo)
        {
            m_ItemActionListSlice = m_CategoryItemActionSet.GetItemActionsForItem(itemInfo.Item, ref m_ItemActionArray);
        }

        /// <summary>
        /// Use an item from the hot bar.
        /// </summary>
        /// <param name="itemSlotIndex">The item slot index of the item to use.</param>
        /// <param name="itemUser">The item user.</param>
        public override void UseAllItemActions(int itemSlotIndex)
        {
            if (CanItemUseAction(itemSlotIndex) == false) {
                return;
            }

            var itemInfo = m_ItemViewSlotsContainer.ItemViewSlots[itemSlotIndex].ItemInfo;
            RefreshItemActions(itemInfo);

            for (int i = 0; i < m_ItemActionListSlice.Count; i++) {
                InvokeActionInternal(itemSlotIndex, i);
            }
        }

        /// <summary>
        /// Use an item from the hot bar.
        /// </summary>
        /// <param name="itemSlotIndex">The item slot index of the item to use.</param>
        /// <param name="itemActionIndex">The item action index.</param>
        public override void UseItemAction(int itemSlotIndex, int itemActionIndex)
        {
            if (CanItemUseAction(itemSlotIndex) == false) {
                return;
            }

            var itemInfo = m_ItemViewSlotsContainer.ItemViewSlots[itemSlotIndex].ItemInfo;
            RefreshItemActions(itemInfo);

            InvokeActionInternal(itemSlotIndex, itemActionIndex);
        }

        /// <summary>
        /// Open the item action panel.
        /// </summary>
        /// <param name="itemInfo">The item info selected.</param>
        /// <param name="index">The index.</param>
        protected override void OpenItemAction(ItemInfo itemInfo, int index)
        {
            if (CanOpenItemActionPanel(itemInfo)) {
                return;
            }

            if (m_ItemViewSlotsContainer.Panel == null) {
                Debug.LogWarning("Parent Panel is not set.");
            }

            RefreshItemActions(itemInfo);
            m_ActionPanel.AssignActions(m_ItemActionListSlice, itemInfo, m_ItemUser, m_ItemViewSlotsContainer, index);

            m_ActionPanel.Open(m_ItemViewSlotsContainer.Panel, m_ItemViewSlotsContainer.GetItemViewSlot(index));
        }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            var actionsName = m_CategoryItemActionSet == null ? "NULL" : m_CategoryItemActionSet.name;
            return GetType().Name + ": " + actionsName;
        }
    }
}