/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.InventoryCollections
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using System;
    using UnityEngine;

    [Serializable]
    public class ItemCollectionStackAmountRestriction : IItemRestriction
    {
        [Tooltip("The itemCollections affected by this restriction.")]
        [SerializeField] protected string[] m_ItemCollectionNames;
        [Tooltip("The max stack amount.")]
        [SerializeField] protected int m_MaxStackAmount;

        [System.NonSerialized] protected IInventory m_Inventory;
        [System.NonSerialized] protected ResizableArray<ItemCollection> m_ItemCollections;
        [System.NonSerialized] protected bool m_Initialized;

        public string[] ItemCollectionNames
        {
            get { return m_ItemCollectionNames; }
        }

        public int MaxStackAmount
        {
            get { return m_MaxStackAmount; }
            set { m_MaxStackAmount = value; }
        }

        /// <summary>
        /// Initialize with the Inventory.
        /// </summary>
        /// <param name="inventory">The inventory.</param>
        /// <param name="force">Force Initialization.</param>
        public void Initialize(IInventory inventory, bool force)
        {
            if (m_Initialized && !force && inventory == m_Inventory) { return; }
            
            if (m_Inventory != inventory) {
                m_Inventory = inventory;
                if (m_ItemCollections == null) { m_ItemCollections = new ResizableArray<ItemCollection>(); }
                m_ItemCollections.Clear();
                for (int i = 0; i < m_ItemCollectionNames.Length; i++) {
                    var match = m_Inventory.GetItemCollection(m_ItemCollectionNames[i]);
                    if (match == null) { continue; }
                    m_ItemCollections.Add(match);
                }
            }

            m_Initialized = true;
        }

        /// <summary>
        /// Get current Item Stack Amount.
        /// </summary>
        /// <returns>The number of item stacks.</returns>
        public int GetCurrentStackAmount()
        {
            var count = 0;
            for (int i = 0; i < m_ItemCollections.Count; i++) {
                count += m_ItemCollections[i].ItemStacks.Count;
            }

            return count;
        }

        /// <summary>
        /// Can the Item be added to the item collection?
        /// </summary>
        /// <param name="itemInfo">The item to add.</param>
        /// <param name="receivingCollection">The item collection the item is added to.</param>
        /// <returns>The item that can be added.</returns>
        public ItemInfo? CanAddItem(ItemInfo itemInfo, ItemCollection receivingCollection)
        {
            if (m_ItemCollections.Contains(receivingCollection) == false) { return itemInfo; }

            var count = GetCurrentStackAmount();

            var availableAdditionalStacks = m_MaxStackAmount - count;

            var itemAmountsThatFit = receivingCollection.GetItemAmountFittingInLimitedAdditionalStacks(itemInfo,
                availableAdditionalStacks);

            if (itemAmountsThatFit == 0) {
                return null;
            }

            return (itemAmountsThatFit, itemInfo);
        }

        /// <summary>
        /// Can the Item be removed.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <returns>The item Info.</returns>
        public ItemInfo? CanRemoveItem(ItemInfo itemInfo)
        {
            return itemInfo;
        }

        /// <summary>
        /// Easy to read string.
        /// </summary>
        /// <returns>String representation.</returns>
        public override string ToString()
        {
            var stackAmount = $" Max Stack Amount {m_MaxStackAmount}";

            if (m_ItemCollectionNames == null || m_ItemCollectionNames.Length == 0) {
                return "EMPTY"+stackAmount;
            }

            var collectionNames = m_ItemCollectionNames[0];
            for (int i = 1; i < m_ItemCollectionNames.Length; i++) {
                collectionNames =  ", "+m_ItemCollectionNames[i];
            }

            return collectionNames +" "+ stackAmount;
        }
    }
}