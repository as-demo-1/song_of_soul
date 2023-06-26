/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.InventoryCollections
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.DataContainers;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// An ItemCollection that sends items to other ItemCollections by checking where it fits.
    /// </summary>
    [Serializable]
    public class ItemTransactionCollection : ItemCollection
    {
        [Tooltip(
            "The item collection where the item will be transferred to, Make sure to add restrictions to does collections.")]
        [SerializeField] protected List<string> m_ItemCollectionNames;
        [Tooltip(
            "Return the item amount that was actually added in the right item collection or return also the amount that was rejected (in case it was added externally)?")]
        [SerializeField] protected bool m_ReturnRealAddedItemAmount;
        [Tooltip("Prevent adding items if they do not match any collections.")]
        [SerializeField] protected bool m_PreventAddingWhenNoMatch = true;

        protected ItemCollection m_AddingToCollection;
        public ItemCollection AddingToCollection => m_AddingToCollection;

        /// <summary>
        /// Check if the item can be added to this item collection.
        /// </summary>
        /// <param name="itemInfo">The item info to add.</param>
        /// <returns>The item info to add.</returns>
        public override ItemInfo? CanAddItem(ItemInfo itemInfo)
        {
            for (int i = 0; i < m_ItemCollectionNames.Count; i++) {
                var itemCollection = m_Inventory.GetItemCollection(m_ItemCollectionNames[i]);
                var result = itemCollection.CanAddItem(itemInfo);
                if (result.HasValue && result.Value.Amount != 0) {
                    m_AddingToCollection = itemCollection;
                    return itemInfo;
                }
            }

            //The item cannot be added.
            m_AddingToCollection = null;
            
            return m_PreventAddingWhenNoMatch ? (ItemInfo?)null : itemInfo;
        }
        
        /// <summary>
        /// Check if the item can be added to this item collection.
        /// </summary>
        /// <param name="itemInfo">The item info to add.</param>
        /// <returns>The item info to add.</returns>
        public ItemCollection GetItemCollectionToTransactionTo(ItemInfo itemInfo)
        {
            var canAddItem = CanAddItem(itemInfo);
            return m_AddingToCollection;
        }

        /// <summary>
        /// Add the item to the item collection where it fits.
        /// </summary>
        /// <param name="itemInfo">The item info to add.</param>
        /// <param name="stackTarget">The item stack where it should be added.</param>
        /// <param name="notifyAdd">Notify that the item was added?</param>
        /// <returns></returns>
        protected override ItemInfo AddInternal(ItemInfo itemInfo, ItemStack stackTarget, bool notifyAdd = true)
        {
            ItemInfo itemInfoAdded = (0, itemInfo);
            if (m_AddingToCollection == null) {
                HandleItemOverflow(itemInfo, ref itemInfoAdded);

                if (m_ReturnRealAddedItemAmount) { return (ItemInfo) (0, itemInfo); }

                return itemInfo;
            }

            var itemInfoToAdd = new ItemInfo(itemInfo.ItemAmount);
            itemInfoAdded = m_AddingToCollection.AddItem(itemInfoToAdd);

            if (itemInfoAdded.Amount < itemInfo.Amount) {
                HandleItemOverflow(itemInfo, ref itemInfoAdded);
            }

            if (m_ReturnRealAddedItemAmount) { return itemInfoAdded; } else {
                return (ItemInfo) (itemInfo.Amount, itemInfoAdded);
            }
        }

        /// <summary>
        /// Remove the item.
        /// </summary>
        /// <param name="itemInfo">THe item to remove.</param>
        /// <returns>The item removed.</returns>
        protected override ItemInfo RemoveInternal(ItemInfo itemInfo)
        {
            var amountToRemove = itemInfo.Amount;
            var AmountRemoved = 0;
            ItemInfo itemInfoRemoved = new ItemInfo(itemInfo.ItemAmount);
            for (int i = 0; i < m_ItemCollectionNames.Count; i++) {
                var itemCollection = m_Inventory.GetItemCollection(m_ItemCollectionNames[i]);
                itemInfoRemoved = itemCollection.RemoveItem((amountToRemove, itemInfo));
                AmountRemoved += itemInfoRemoved.Amount;
                amountToRemove -= itemInfoRemoved.Amount;

                if (amountToRemove <= 0) { return (itemInfo.Amount, itemInfoRemoved); }
            }

            return (AmountRemoved, itemInfoRemoved);
        }

        /// <summary>
        /// Get the item info in the linked item collections.
        /// </summary>
        /// <param name="item">The item to get.</param>
        /// <returns>The item info.</returns>
        public override ItemInfo? GetItemInfo(Item item)
        {
            for (int i = 0; i < m_ItemCollectionNames.Count; i++) {
                var itemCollection = m_Inventory.GetItemCollection(m_ItemCollectionNames[i]);

                var itemInfo = itemCollection.GetItemInfo(item);

                if (itemInfo.HasValue) { return itemInfo; }
            }

            return null;
        }
    }
}