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
    public class ItemCollectionCategoryRestriction : IItemRestriction
    {
        [Tooltip("The itemCollections affected by this restriction.")]
        [SerializeField] protected string[] m_ItemCollectionNames;
        [Tooltip("The Item Categories allowed.")]
        [SerializeField] protected DynamicItemCategoryArray m_AllowedItemCategories;
        [Tooltip("The Item Categories that are not allowed.")]
        [SerializeField] protected DynamicItemCategoryArray m_NotAllowedItemCategories;

        [System.NonSerialized] protected IInventory m_Inventory;
        [System.NonSerialized] protected ResizableArray<ItemCollection> m_ItemCollections;
        [System.NonSerialized] protected bool m_Initialized;
        
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
        /// Can the Item be added to the item collection?
        /// </summary>
        /// <param name="itemInfo">The item to add.</param>
        /// <param name="receivingCollection">The item collection the item is added to.</param>
        /// <returns>The item that can be added.</returns>
        public ItemInfo? CanAddItem(ItemInfo itemInfo, ItemCollection receivingCollection)
        {
            if (m_ItemCollections.Contains(receivingCollection) == false) { return itemInfo; }
            
            var item = itemInfo.Item;

            if (m_AllowedItemCategories.Count != 0) {
                var foundMatch = false;
                for (int i = 0; i < m_AllowedItemCategories.Count; i++) {
                    var allowedItemCategory = m_AllowedItemCategories.Value[i];
                    if (!allowedItemCategory.InherentlyContains(itemInfo.Item)) { continue; }

                    foundMatch = true;
                    break;
                }

                if (foundMatch == false) {
                    return null;
                }
            }

            if (m_NotAllowedItemCategories.Count == 0) {
                return itemInfo;
            }

            for (int i = 0; i < m_NotAllowedItemCategories.Count; i++) {
                if (m_NotAllowedItemCategories.Value[i].InherentlyContains(itemInfo.Item)) {
                    return null;
                }
            }

            return itemInfo;
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
            var allowedItemCategory = "";
            if (m_AllowedItemCategories.Count == 0) {
                allowedItemCategory = "All";
            } else {
                allowedItemCategory = m_AllowedItemCategories.Value[0]?.name;
                for (int i = 1; i < m_AllowedItemCategories.Count; i++) {
                    allowedItemCategory +=  ", "+ m_AllowedItemCategories.Value[i]?.name;
                }
            }

            var notAllowedItemCaegories = "";
            if (m_NotAllowedItemCategories.Count == 0) {
                notAllowedItemCaegories = "";
            } else {
                notAllowedItemCaegories = "Except "+m_NotAllowedItemCategories.Value[0]?.name;
                for (int i = 1; i < m_NotAllowedItemCategories.Count; i++) {
                    notAllowedItemCaegories +=  ", "+ m_NotAllowedItemCategories.Value[i]?.name;
                }
            }
            
            var s = $" Allows Categories {allowedItemCategory} {notAllowedItemCaegories}";

            if (m_ItemCollectionNames == null || m_ItemCollectionNames.Length == 0) {
                return "EMPTY"+s;
            }

            var collectionNames = m_ItemCollectionNames[0];
            for (int i = 1; i < m_ItemCollectionNames.Length; i++) {
                collectionNames +=  ", "+m_ItemCollectionNames[i];
            }

            return collectionNames +" "+ s;
        }
    }
}