/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.InventoryCollections
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Utility;
    using System.Collections.Generic;
    using UnityEngine;

    public class ItemCollectionGroup
    {
        protected List<ItemCollection> m_ItemCollections;
        protected List<ItemCollection> m_IgnoreCollections;

        protected List<ItemStack> m_AllItemStacks;
        protected List<ItemInfo> m_AllItemInfos;
        
        public List<ItemCollection> ItemCollections
        {
            get => m_ItemCollections;
            set => m_ItemCollections = value;
        }
        
        public List<ItemCollection> IgnoreCollections
        {
            get => m_IgnoreCollections;
            set => m_IgnoreCollections = value;
        }
        
        /// <summary>
        /// The Item Collection, it includes all the items in the inventory.
        /// </summary>
        public virtual ItemCollection MainItemCollection {
            get {
                if (m_ItemCollections.Count == 0) {
                    Debug.LogError("ItemCollection should always have a size of at least 1.");
                    return null;
                }
                return GetItemCollection(ItemCollectionPurpose.Main);
            }
        }
        
        /// <summary>
        /// Get the number of item collections in the inventory.
        /// </summary>
        /// <returns>The item collection count.</returns>
        public int GetItemCollectionCount()
        {
            return m_ItemCollections.Count;
        }

        /// <summary>
        /// Get an itemCollection by name.
        /// </summary>
        /// <param name="itemCollectionName">The name.</param>
        /// <returns>The item collection.</returns>
        public ItemCollection GetItemCollection(string itemCollectionName)
        {
            for (int i = 0; i < m_ItemCollections.Count; i++) {
                if (m_ItemCollections[i].Name == itemCollectionName) { return m_ItemCollections[i]; }
            }

            return null;
        }

        /// <summary>
        /// Get the item collection by purpose.
        /// </summary>
        /// <param name="purpose">The purpose.</param>
        /// <returns>The item collection.</returns>
        public ItemCollection GetItemCollection(ItemCollectionPurpose purpose)
        {
            if (purpose == ItemCollectionPurpose.None) { return null; }

            for (int i = 0; i < m_ItemCollections.Count; i++) {
                if (m_ItemCollections[i].Purpose == purpose) { return m_ItemCollections[i]; }
            }

            if (purpose == ItemCollectionPurpose.Main) { return m_ItemCollections[0]; }

            return null;
        }

        /// <summary>
        /// Get the item collection index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The item collection.</returns>
        public ItemCollection GetItemCollection(int index)
        {
            if (index < 0 || index >= m_ItemCollections.Count) { return null; }

            return m_ItemCollections[index];
        }

        /// <summary>
        /// Get the item collection by ItemCollectionId.
        /// </summary>
        /// <param name="collectionID">The itemCollection identifier.</param>
        /// <returns>The item collection.</returns>
        public ItemCollection GetItemCollection(ItemCollectionID collectionID)
        {
            var bestMatchWeight = 0;
            ItemCollection bestMatch = null;

            for (int i = 0; i < m_ItemCollections.Count; i++) {
                var match = collectionID.CompareWeighted(m_ItemCollections[i]);
                if (match <= bestMatchWeight) { continue; }

                bestMatchWeight = match;
                bestMatch = m_ItemCollections[i];
            }

            if (bestMatch != null) { return bestMatch; }

            if (collectionID.Purpose == ItemCollectionPurpose.Main) { return m_ItemCollections[0]; }

            return null;
        }

        public bool Contains(ItemCollection itemCollection)
        {
            if (itemCollection == null) { return false;}

            return m_ItemCollections.Contains(itemCollection);
        }

        /// <summary>
        /// Add an item to the an item collection in the inventory.
        /// </summary>
        /// <param name="itemInfo">The amount of item being added.</param>
        /// <param name="stackTarget">The item stack where the item should be added.</param>
        /// <returns>Returns the number of items added, 0 if no item was added.</returns>
        public virtual ItemInfo AddItem(ItemInfo itemInfo, ItemStack stackTarget = null)
        {
            if (stackTarget != null && stackTarget.ItemCollection != null && Contains(stackTarget.ItemCollection)) {
                return stackTarget.ItemCollection.AddItem(itemInfo, stackTarget);
            }

            return MainItemCollection.AddItem(itemInfo);
        }

        /// <summary>
        /// Add an item to the an item collection in the inventory.
        /// </summary>
        /// <param name="itemDefinition">The itemDefinition of item being added.</param>
        /// <param name="amount">The amount to add.</param>
        /// <returns>Returns the number of items added, 0 if no item was added.</returns>
        public virtual ItemInfo AddItem(ItemDefinition itemDefinition, int amount)
        {
            if (itemDefinition == null) { return ItemInfo.None; }

            var itemInfo = (ItemInfo)(InventorySystemManager.CreateItem(itemDefinition), amount);

            return AddItem(itemInfo);
        }

        /// <summary>
        /// Add an item to the an item collection in the inventory.
        /// </summary>
        /// <param name="itemName">The item name being added.</param>
        /// <param name="amount">The amount to add.</param>
        /// <returns>Returns the number of items added, 0 if no item was added.</returns>
        public virtual ItemInfo AddItem(string itemName, int amount)
        {
            var itemInfo = (ItemInfo)(InventorySystemManager.CreateItem(itemName), amount);

            return AddItem(itemInfo);
        }

        /// <summary>
        /// Remove an item from the item collection in the inventory.
        /// </summary>
        /// <param name="itemInfo">The amount of item to remove.</param>
        /// <returns>Returns true if the item was removed correctly.</returns>
        public virtual ItemInfo RemoveItem(ItemInfo itemInfo)
        {
            if (Contains(itemInfo.ItemCollection)) {
                return itemInfo.ItemCollection.RemoveItem(itemInfo);
            }

            return MainItemCollection.RemoveItem(itemInfo);
        }

        /// <summary>
        /// Remove an item from the item collection in the inventory.
        /// </summary>
        /// <param name="itemDefinition">The itemDefinition of item being removed.</param>
        /// <param name="amount">The amount to remove.</param>
        /// <returns>Returns the number of items added, 0 if no item was removed.</returns>
        public virtual ItemInfo RemoveItem(ItemDefinition itemDefinition, int amount)
        {
            if (itemDefinition == null) { return ItemInfo.None; }

            var itemInfo = GetItemInfo(itemDefinition, false);

            if (itemInfo.HasValue == false) { return ItemInfo.None; }

            return RemoveItem((amount, itemInfo.Value));
        }

        /// <summary>
        /// Remove an item from the item collection in the inventory.
        /// </summary>
        /// <param name="itemName">The item name of the item being removed.</param>
        /// <param name="amount">The amount to remove.</param>
        /// <returns>Returns the number of items added, 0 if no item was removed.</returns>
        public virtual ItemInfo RemoveItem(string itemName, int amount)
        {
            var itemDefinition = InventorySystemManager.GetItemDefinition(itemName);

            return RemoveItem(itemDefinition, amount);
        }

        /// <summary>
        /// Determines if the Item Collection contains the item.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <param name="similarItem">Find an itemInfo with that value equivalent item or just one similar?</param>
        /// <returns>Returns true if the amount of items in the collection is equal or bigger than the amount specified.</returns>
        public virtual bool HasItem(Item item, bool similarItem = true)
        {
            if (item == null) { return false; }

            return GetItemAmount(item, similarItem) >= 1;
        }

        /// <summary>
        /// Determines if the Item Collection contains the item.
        /// </summary>
        /// <param name="itemAmount">The item to check.</param>
        /// <param name="similarItem">Find an itemInfo with that value equivalent item or just one similar?</param>
        /// <returns>Returns true if the amount of items in the collection is equal or bigger than the amount specified.</returns>
        public virtual bool HasItem(ItemAmount itemAmount, bool similarItem = true)
        {
            if (itemAmount.Item == null) { return false; }

            return GetItemAmount(itemAmount.Item, similarItem) >= itemAmount.Amount;
        }

        /// <summary>
        /// Check if the inventory has at least the item amount specified.
        /// </summary>
        /// <param name="itemInfo">The item info to check.</param>
        /// <param name="similarItem">Find an itemInfo with that value equivalent item or just one similar?</param>
        /// <returns>True if the inventory has at least that amount.</returns>
        public virtual bool HasItem(ItemInfo itemInfo, bool similarItem = true)
        {
            if (itemInfo.Item == null) { return true; }

            return GetItemAmount(itemInfo.Item, similarItem) >= itemInfo.Amount;
        }

        /// <summary>
        /// Check if the inventory has at least the item amount specified.
        /// </summary>
        /// <param name="itemDefinitionAmount">The item info to check.</param>
        /// <returns>True if the inventory has at least that amount.</returns>
        public virtual bool HasItem(ItemDefinitionAmount itemDefinitionAmount)
        {
            if (itemDefinitionAmount.ItemDefinition == null) { return true; }

            return GetItemAmount(itemDefinitionAmount.ItemDefinition) >= itemDefinitionAmount.Amount;
        }

        /// <summary>
        /// Determines if the Item Collection contains an item with the Item Definition.
        /// </summary>
        /// <param name="itemDefinition">The itemDefinition of the item to check.</param>
        /// <param name="checkInherently">Take into account the children of the Item Definition.</param>
        /// <param name="countStacks">If true count the stacks number not the stacks amounts.</param>
        /// <returns>Returns true if the amount of items with the Item Definition in the collection is equal or bigger than the amount specified.</returns>
        public virtual bool HasItem(ItemDefinitionAmount itemDefinitionAmount, bool checkInherently, bool countStacks = false)
        {
            if (itemDefinitionAmount.ItemDefinition == null) { return false; }

            var count = GetItemAmount(itemDefinitionAmount.ItemDefinition, checkInherently, countStacks);

            return count >= itemDefinitionAmount.Amount;
        }

        /// <summary>
        /// Determines if the Item Collection contains an item with the exact same category provided (does NOT check the category children).
        /// </summary>
        /// <param name="categoryAmount">The category amount of the items being checked.</param>
        /// <param name="checkInherently">Take into account the children of the item category.</param>
        /// <param name="countStacks">If true count the stacks number not the stacks amounts.</param>
        /// <returns>Returns true if the amount of items with the Item Category in the collection is equal or bigger than the amount specified.</returns>
        public virtual bool HasItem(ItemCategoryAmount categoryAmount, bool checkInherently, bool countStacks = false)
        {
            if (categoryAmount.ItemCategory == null) { return false; }

            var count = GetItemAmount(categoryAmount.ItemCategory, checkInherently, countStacks);

            return count >= categoryAmount.Amount;
        }

        /// <summary>
        /// Checks if the inventory contains a list of Item Amounts.
        /// </summary>
        /// <param name="itemAmounts">The Item amounts list to check.</param>
        /// <returns>Returns true if the collection has ALL the item amounts in the list.</returns>
        public virtual bool HasItemList(ListSlice<ItemAmount> itemAmounts)
        {
            for (var i = 0; i < itemAmounts.Count; i++) {
                var itemAmount = itemAmounts[i];
                if (HasItem(itemAmount) == false) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if the inventory contains a list of Item Infos.
        /// </summary>
        /// <param name="itemInfos">The Item info list to check.</param>
        /// <returns>Returns true if the collection has ALL the item amounts in the list.</returns>
        public virtual bool HasItemList(ListSlice<ItemInfo> itemInfos)
        {
            for (var i = 0; i < itemInfos.Count; i++) {
                var itemInfo = itemInfos[i];
                if (HasItem(itemInfo) == false) {
                    return false;
                }
            }

            return true;
        }
        
        /// <summary>
        /// Check if the collection can be ignored when searching for items.
        /// </summary>
        /// <param name="itemCollection">The item collection to check.</param>
        /// <returns>It can be ignored if true.</returns>
        protected virtual bool IgnoreCollection(ItemCollection itemCollection)
        {
            if (itemCollection == null) { return true; }
            if (m_IgnoreCollections == null) { return false; }

            return m_IgnoreCollections.Contains(itemCollection);
        }

        /// <summary>
        /// Return the amount of item in the inventory.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <param name="similarItem">Find an itemInfo with that value equivalent item or just one similar?</param>
        /// <returns>The amount of that item in the inventory.</returns>
        public virtual int GetItemAmount(Item item, bool similarItem=true)
        {
            var amount = 0;
            for (int i = 0; i < m_ItemCollections.Count; i++) {
                if (IgnoreCollection(m_ItemCollections[i])) { continue; }

                amount += m_ItemCollections[i].GetItemAmount(item, similarItem);
            }

            return amount;
        }

        /// <summary>
        /// Returns the number of items in the collection with the specified Item Definition.
        /// </summary>
        /// <param name="itemDefinition">The Item Definition to retrieve the amount of.</param>
        /// <param name="checkInherently">Take into account the children of the Item Definition.</param>
        /// <param name="unique">Should count unique items or amounts of items.</param>
        /// <returns>The number of items in the collection with the specified Item Definition.</returns>
        public virtual int GetItemAmount(ItemDefinition itemDefinition, bool checkInherently = false, bool unique = false)
        {
            var amount = 0;
            for (int i = 0; i < m_ItemCollections.Count; i++) {
                if (IgnoreCollection(m_ItemCollections[i])) { continue; }

                amount += m_ItemCollections[i].GetItemAmount(itemDefinition, checkInherently, unique);
            }

            return amount;
        }

        /// <summary>
        /// Returns the number of items in the collection with the specified Item Definition.
        /// </summary>
        /// <param name="itemCategory">The Item Category to retrieve the amount of.</param>
        /// <param name="checkInherently">Take into account the children of the item category.</param>
        /// <param name="unique">Should count unique items or amounts of items.</param>
        /// <returns>The number of items in the collection with the specified Item Category.</returns>
        public virtual int GetItemAmount(ItemCategory itemCategory, bool checkInherently = false, bool unique = false)
        {
            var amount = 0;
            for (int i = 0; i < m_ItemCollections.Count; i++) {
                if (IgnoreCollection(m_ItemCollections[i])) { continue; }

                amount += m_ItemCollections[i].GetItemAmount(itemCategory, checkInherently, unique);
            }

            return amount;
        }

        /// <summary>
        /// Returns the first itemInfo that matches the description.
        /// </summary>
        /// <param name="item">The item to search for.</param>
        /// <returns>The itemInfo.</returns>
        public virtual ItemInfo? GetItemInfo(Item item)
        {
            for (int i = 0; i < m_ItemCollections.Count; i++) {
                if (IgnoreCollection(m_ItemCollections[i])) { continue; }

                var itemInfo = m_ItemCollections[i].GetItemInfo(item);

                if (itemInfo.HasValue) { return itemInfo; }
            }

            return null;
        }

        /// <summary>
        /// Returns the first itemInfo that matches the description.
        /// </summary>
        /// <param name="itemDefinition">The Item Definition to retrieve the amount of.</param>
        /// <param name="checkInherently">Take into account the children of the item definition.</param>
        /// <returns>The first item info found in the inventory.</returns>
        public virtual ItemInfo? GetItemInfo(ItemDefinition itemDefinition, bool checkInherently = false)
        {
            for (int i = 0; i < m_ItemCollections.Count; i++) {
                if (IgnoreCollection(m_ItemCollections[i])) { continue; }

                var itemInfo = m_ItemCollections[i].GetItemInfo(itemDefinition, checkInherently);

                if (itemInfo.HasValue) { return itemInfo; }
            }

            return null;
        }

        /// <summary>
        /// Returns the first itemInfo that matches the description.
        /// </summary>
        /// <param name="itemCategory">The Item Category to retrieve the amount of.</param>
        /// <param name="checkInherently">Take into account the children of the item category.</param>
        /// <returns>The first item info found in the inventory.</returns>
        public virtual ItemInfo? GetItemInfo(ItemCategory itemCategory, bool checkInherently = false)
        {
            for (int i = 0; i < m_ItemCollections.Count; i++) {
                if (IgnoreCollection(m_ItemCollections[i])) { continue; }

                var itemInfo = m_ItemCollections[i].GetItemInfo(itemCategory, checkInherently);

                if (itemInfo.HasValue) { return itemInfo; }
            }

            return null;
        }

        public ListSlice<ItemStack> GetAllItemStacks()
        {
            if (m_AllItemStacks == null) {
                m_AllItemStacks = new List<ItemStack>();
            }
            
            m_AllItemStacks.Clear();

            for (int i = 0; i < m_ItemCollections.Count; i++) {
                if (IgnoreCollection(m_ItemCollections[i])) { continue; }
                
                var allItemStacks = m_ItemCollections[i].GetAllItemStacks();

                m_AllItemStacks.AddRange(allItemStacks);
            }

            return m_AllItemStacks;
        }
        
        public ListSlice<ItemInfo> GetAllItemInfos()
        {
            if (m_AllItemInfos == null) {
                m_AllItemInfos = new List<ItemInfo>();
            }
            
            m_AllItemInfos.Clear();

            for (int i = 0; i < m_ItemCollections.Count; i++) {
                if (IgnoreCollection(m_ItemCollections[i])) { continue; }
                
                var allItemStacks = m_ItemCollections[i].GetAllItemStacks();

                for (int j = 0; j < allItemStacks.Count; j++) {
                    m_AllItemInfos.Add((ItemInfo) allItemStacks[j]);
                }
            }

            return m_AllItemInfos;
        }
    }
}