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
    using Opsive.UltimateInventorySystem.UI.Item;
    using Opsive.UltimateInventorySystem.UI.Panels.ActionPanels;
    using System;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using UnityEngine;

    /// <summary>
    /// Base class to add actions to the item view slots container.
    /// </summary>
    public abstract class ItemViewSlotsContainerItemActionBindingBase : ItemViewSlotsContainerBinding
    {
        public event Action OnItemUserAssigned;
        
        [Tooltip("The item user. Defaults to the Inventory Item User if null.")]
        [SerializeField] protected ItemUser m_ItemUser;
        [Tooltip("The action panel will open when clicking an item, displaying the actions that can be used on it. Can be null.")]
        [SerializeField] internal ItemActionPanel m_ActionPanel;
        [Tooltip("Use the item on click.")]
        [SerializeField] protected bool m_UseItemOnClick = true;
        [Tooltip("Which item action to use when triggered, -1 will use all item actions.")]
        [SerializeField] protected int m_UseItemActionIndex = -1;
        [Tooltip("Allow item actions to be called on empty slots.")]
        [SerializeField] protected bool m_DisableActionOnEmptySlots;
        [Tooltip("Set the Item User to the Inventory Item User.")] 
        [SerializeField] protected bool m_AutoSetItemUser = true;

        protected ListSlice<ItemAction> m_ItemActionListSlice;

        public ItemUser ItemUser => m_ItemUser;

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="force">Force Initialize.</param>
        public override void Initialize(bool force)
        {
            if (m_IsInitialized && force == false) { return; }
            base.Initialize(force);

            if (m_ActionPanel != null) { m_ActionPanel.Close(); }

            m_ItemActionListSlice = InitializeItemActionList();
        }

        /// <summary>
        /// Create the Item Action List Slice.
        /// </summary>
        /// <returns>The List slice of item actions.</returns>
        protected abstract ListSlice<ItemAction> InitializeItemActionList();

        /// <summary>
        /// On bind.
        /// </summary>
        protected override void OnBindItemViewSlotContainer()
        {
            OnInventoryChanged(m_ItemViewSlotsContainer.Inventory);
            
            m_ItemViewSlotsContainer.OnBindInventory += OnInventoryChanged;
            m_ItemViewSlotsContainer.OnResetDraw += ResetDraw;
            m_ItemViewSlotsContainer.OnItemViewSlotClicked += HandleItemClicked;
            if (m_ActionPanel != null) {
                m_ActionPanel.OnAfterAnyItemActionInvoke += HandleItemActionInvoked;
            }

        }

        /// <summary>
        /// Update the Item User when a new Inventory is set.
        /// </summary>
        /// <param name="newInventory"></param>
        private void OnInventoryChanged(Inventory newInventory)
        {
            if (m_AutoSetItemUser) {
                SetItemUser(m_ItemViewSlotsContainer.Inventory?.ItemUser);
            }
        }

        /// <summary>
        /// Reset the state of the Item Action binding.
        /// </summary>
        protected virtual void ResetDraw()
        {
            CloseItemAction(false);
        }

        /// <summary>
        /// On UnBind.
        /// </summary>
        protected override void OnUnbindItemViewSlotContainer()
        {
            m_ItemViewSlotsContainer.OnBindInventory -= OnInventoryChanged;
            m_ItemViewSlotsContainer.OnResetDraw -= ResetDraw;
            m_ItemViewSlotsContainer.OnItemViewSlotClicked -= HandleItemClicked;
            if (m_ActionPanel != null) {
                m_ActionPanel.OnAfterAnyItemActionInvoke -= HandleItemActionInvoked;
            }
        }

        /// <summary>
        /// Set the item user.
        /// </summary>
        /// <param name="itemUser">The item user.</param>
        public void SetItemUser(ItemUser itemUser)
        {
            m_ItemUser = itemUser;
            OnItemUserAssigned?.Invoke();
        }

        /// <summary>
        /// Handle the item clicked.
        /// </summary>
        /// <param name="eventdata">The slot event data.</param>
        private void HandleItemClicked(ItemViewSlotEventData eventdata)
        {
            if (m_UseItemOnClick == false) { return; }

            TriggerItemAction();
        }

        /// <summary>
        /// Handle the item action being invoked.
        /// </summary>
        /// <param name="itemActionIndex">The index of the item action.</param>
        private void HandleItemActionInvoked(int itemActionIndex)
        {
            m_ItemViewSlotsContainer.Draw();
        }

        /// <summary>
        /// Trigger the item action for the selected slot..
        /// </summary>
        public void TriggerItemAction()
        {
            TriggerItemAction(m_ItemViewSlotsContainer.GetSelectedSlot());
        }

        /// <summary>
        /// Trigger the item action on the slot of the index provided.
        /// </summary>
        /// <param name="slotIndex">The item slot index.</param>
        public void TriggerItemAction(int slotIndex)
        {
            var slotCount = m_ItemViewSlotsContainer.GetItemViewSlotCount();
            if (slotIndex < 0 && slotIndex >= slotCount || slotIndex >= m_ItemViewSlotsContainer.ItemViewSlots.Count) {
                Debug.LogWarning("The slot index you are trying to use is out of range " + slotIndex + " / " + slotCount);
                return;
            }

            TriggerItemAction(m_ItemViewSlotsContainer.ItemViewSlots[slotIndex]);
        }

        /// <summary>
        /// Trigger the item action of the item view slot.
        /// </summary>
        /// <param name="itemViewSlot">The item view slot.</param>
        public void TriggerItemAction(ItemViewSlot itemViewSlot)
        {
            if (itemViewSlot == null) { return; }

            if (m_ActionPanel != null) {
                OpenItemAction(itemViewSlot.ItemInfo, itemViewSlot.Index);
                return;
            }

            if (m_UseItemActionIndex == -1) {
                UseAllItemActions(itemViewSlot.Index);
            } else {
                UseItemAction(itemViewSlot.Index, m_UseItemActionIndex);
            }
        }

        /// <summary>
        /// Can the item use the action.
        /// </summary>
        /// <param name="itemSlotIndex">The item slot index.</param>
        /// <returns>True if the item can use the action.</returns>
        public virtual bool CanItemUseAction(int itemSlotIndex)
        {
            if (itemSlotIndex < 0 && itemSlotIndex >= m_ItemViewSlotsContainer.GetItemViewSlotCount()) { return false; }

            var itemInfo = m_ItemViewSlotsContainer.ItemViewSlots[itemSlotIndex].ItemInfo;

            if (m_DisableActionOnEmptySlots && (itemInfo.Item == null || itemInfo.Amount <= 0)) { return false; }

            return true;
        }

        /// <summary>
        /// Use an item from the hot bar.
        /// </summary>
        /// <param name="itemSlotIndex">The item slot index of the item to use.</param>
        /// <param name="itemUser">The item user.</param>
        public virtual void UseAllItemActions(int itemSlotIndex)
        {
            if (CanItemUseAction(itemSlotIndex) == false) {
                return;
            }

            for (int i = 0; i < m_ItemActionListSlice.Count; i++) {
                InvokeActionInternal(itemSlotIndex, i);
            }
        }

        /// <summary>
        /// Use an item from the hot bar.
        /// </summary>
        /// <param name="itemActionIndex">The item action index.</param>
        public virtual void UseItemActionOnSelectedSlot(int itemActionIndex)
        {
            var selectedSlot = m_ItemViewSlotsContainer.GetSelectedSlot();
            if (selectedSlot == null) { return; }

            UseItemAction(selectedSlot.Index, itemActionIndex);
        }

        /// <summary>
        /// Use an item from the hot bar.
        /// </summary>
        /// <param name="itemSlotIndex">The item slot index of the item to use.</param>
        /// <param name="itemActionIndex">The item action index.</param>
        public virtual void UseItemAction(int itemSlotIndex, int itemActionIndex)
        {
            if (CanItemUseAction(itemSlotIndex) == false) {
                return;
            }

            InvokeActionInternal(itemSlotIndex, itemActionIndex);
        }

        /// <summary>
        /// Use an item from the hot bar.
        /// </summary>
        /// <param name="itemSlotIndex">The item slot index of the item to use.</param>
        /// <param name="itemActionIndex">The item action index.</param>
        protected virtual void InvokeActionInternal(int itemSlotIndex, int itemActionIndex)
        {
            if (itemActionIndex < 0 || itemActionIndex >= m_ItemActionListSlice.Count) { return; }

            var itemAction = m_ItemActionListSlice[itemActionIndex];

            if (itemAction == null) { return; }

            var itemViewSlot = m_ItemViewSlotsContainer.ItemViewSlots[itemSlotIndex];
            var itemInfo = itemViewSlot.ItemInfo;

            itemAction.Initialize(false);

            if (itemAction is IActionWithPanel actionWithPanel) {
                actionWithPanel.SetParentPanel(m_ItemViewSlotsContainer.Panel, itemViewSlot, m_ItemViewSlotsContainer.transform);
            }

            if (itemAction is ItemViewSlotsContainerItemAction actionViewSlotsContainer) {
                actionViewSlotsContainer.SetViewSlotsContainer(m_ItemViewSlotsContainer, itemSlotIndex);
            }

            itemAction.InvokeAction(itemInfo, m_ItemUser);
        }

        /// <summary>
        /// Open the item action panel.
        /// </summary>
        /// <param name="itemInfo">The item info selected.</param>
        /// <param name="index">The index.</param>
        protected virtual void OpenItemAction(ItemInfo itemInfo, int index)
        {
            if (CanOpenItemActionPanel(itemInfo)) {
                return;
            }

            m_ActionPanel.AssignActions(m_ItemActionListSlice, itemInfo, m_ItemUser, m_ItemViewSlotsContainer, index);

            if (m_ItemViewSlotsContainer.Panel == null) {
                Debug.LogWarning("Parent Panel is not set.");
            }
            m_ActionPanel.Open(m_ItemViewSlotsContainer.Panel, m_ItemViewSlotsContainer.GetItemViewSlot(index));
        }

        /// <summary>
        /// Can the item action panel be opened.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <returns>True.</returns>
        protected virtual bool CanOpenItemActionPanel(ItemInfo itemInfo)
        {
            if (m_ActionPanel == null) { return true; }

            if (m_DisableActionOnEmptySlots && (itemInfo.Item == null || itemInfo.Amount <= 0)) { return true; }

            return false;
        }

        /// <summary>
        /// Close the item action panel.
        /// </summary>
        public virtual void CloseItemAction(bool selectPrevious)
        {
            if (m_ActionPanel == null) { return; }

            m_ActionPanel.Close(selectPrevious);
        }
    }
}