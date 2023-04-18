/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.ItemActions;
    using UnityEngine;

    /// <summary>
    /// This object is used to add Item Actions to an Item View slot container.
    /// </summary>
    public class ItemViewSlotsContainerItemActionBinding : ItemViewSlotsContainerItemActionBindingBase
    {
        [UnityEngine.Serialization.FormerlySerializedAs("m_CategoryItemActions")]
        [Tooltip("The categories item actions. Specifies the actions that can be performed on each item. Can be null.")]
        [SerializeField] public ItemActionSet m_ItemActionSet;

        public ItemActionSet ItemActionSet => m_ItemActionSet;

        /// <summary>
        /// Set the Item Action Set.
        /// </summary>
        /// <param name="itemActionSet">The ITem Action Set to set.</param>
        public void SetItemActionSet(ItemActionSet itemActionSet)
        {
            m_ItemActionSet = itemActionSet;
            m_ItemActionListSlice = InitializeItemActionList();
        }

        /// <summary>
        /// Initialize the item action list.
        /// </summary>
        /// <returns>The item action list.</returns>
        protected override ListSlice<ItemAction> InitializeItemActionList()
        {
            m_ItemActionSet.ItemActionCollection.Initialize(false);
            var itemActionArray = new ItemAction[m_ItemActionSet.ItemActionCollection.Count];
            for (int i = 0; i < itemActionArray.Length; i++) {
                itemActionArray[i] = m_ItemActionSet.ItemActionCollection[i];
                itemActionArray[i].Initialize(false);
            }

            return itemActionArray;
        }

        /// <summary>
        /// Can the Item use th action.
        /// </summary>
        /// <param name="itemSlotIndex">The slot index.</param>
        /// <returns>True if the item can use at least one action.</returns>
        public override bool CanItemUseAction(int itemSlotIndex)
        {
            var canUse = base.CanItemUseAction(itemSlotIndex);
            if (canUse == false) { return false; }

            var itemInfo = m_ItemViewSlotsContainer.ItemViewSlots[itemSlotIndex].ItemInfo;
            if (m_ItemActionSet.MatchItem(itemInfo.Item) == false) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            var actionsName = m_ItemActionSet == null ? "NULL" : m_ItemActionSet.name;
            return GetType().Name + ": " + actionsName;
        }
    }
}