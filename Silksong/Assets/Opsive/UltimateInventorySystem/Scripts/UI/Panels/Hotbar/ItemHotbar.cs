/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.Hotbar
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.ItemActions;
    using Opsive.UltimateInventorySystem.UI.Item;
    using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers;
    using System;
    using UnityEngine;

    /// <summary>
    /// The hot item bar component allows you to use an item action for an item that was added to the hot bar.
    /// </summary>
    public class ItemHotbar : ItemViewSlotsContainer
    {
        [Tooltip("Use the item assigned to this slot when clicked.")]
        [SerializeField] protected ItemViewSlotsContainerItemActionBindingBase m_ItemActionsBinding;

        public ItemUser ItemUser => m_ItemActionsBinding.ItemUser;

        /// <summary>
        /// Initialize before setting the inventory.
        /// </summary>
        protected override void OnInitializeBeforeSettingInventory()
        {
            base.OnInitializeBeforeSettingInventory();

            if (m_ItemActionsBinding == null) {
                m_ItemActionsBinding = GetComponent<ItemViewSlotsContainerItemActionBindingBase>();
                if (m_ItemActionsBinding == null) {
                    Debug.LogError($"The item hotbar '{name}' is missing an item view slots container item action binding", gameObject);
                    return;
                } else {
                    if (m_ItemActionsBinding.ItemViewSlotsContainer != this) {
                        m_ItemActionsBinding.BindItemViewSlotContainer(this);
                    }
                }
            }
        }

        /// <summary>
        /// Handle the inventory being changed.
        /// </summary>
        /// <param name="previousInventory">The previous inventory.</param>
        /// <param name="newInventory">The new inventory.</param>
        protected override void OnInventoryChanged(Inventory previousInventory, Inventory newInventory)
        {
            base.OnInventoryChanged(previousInventory, newInventory);

            if (m_Inventory == null) { return; }

            m_ItemActionsBinding.SetItemUser(m_Inventory.ItemUser);
        }

        /// <summary>
        /// Draw the Item Hotbar and refresh all the views.
        /// </summary>
        protected override void DrawInternal()
        {
            var slots = m_ItemViewSlots;
            for (int i = 0; i < m_ItemViewSlots.Length; i++) {
                var itemInfo = slots[i].ItemInfo;

                if (itemInfo.Item == null) { continue; }

                var amount = itemInfo.ItemCollection?.GetItemAmount(itemInfo.Item) ?? 0;

                ItemInfo newItemInfo;
                if (amount == 0) {
                    var result = itemInfo.Inventory?.GetItemInfo(itemInfo.Item);

                    if (result.HasValue) { newItemInfo = result.Value; } else {
                        newItemInfo = (0, itemInfo);
                    }
                } else { newItemInfo = (itemInfo.Item, amount, itemInfo.ItemCollection); }

                slots[i].SetItemInfo(newItemInfo);
            }

            base.DrawInternal();
        }

        /// <summary>
        /// Use an item from the hot bar.
        /// </summary>
        /// <param name="itemSlotIndex">The item slot index of the item to use.</param>
        /// <param name="itemUser">The item user.</param>
        public virtual void UseItem(int itemSlotIndex)
        {
            m_ItemActionsBinding.TriggerItemAction(itemSlotIndex);
        }

        /// <summary>
        /// Due to the nature of the item hotbar, it cannot give items.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="slotIndex">The slot index.</param>
        /// <returns>False.</returns>
        public override bool CanGiveItem(ItemInfo itemInfo, int slotIndex)
        {
            return false;
        }

        /// <summary>
        /// Toggle whether to assign or unassign the item to the slot.
        /// </summary>
        /// <param name="itemInfo">The item.</param>
        /// <param name="slot">The item slot.</param>
        public override void ToggleAssignItemToSlot(ItemInfo itemInfo, int slot)
        {
            var matchOnSameSlot = GetItemAt(slot) == itemInfo;

            if (GetItemIndex(itemInfo) != -1) {
                UnassignItemFromSlots(itemInfo);
            }

            if (matchOnSameSlot == false) {
                AssignItemToSlot(itemInfo, slot);
            }
        }
    }
}
