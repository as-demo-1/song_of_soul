/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.Hotbar
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.ItemActions;
    using Opsive.UltimateInventorySystem.Storage;
    using Opsive.UltimateInventorySystem.UI.Item;
    using Opsive.UltimateInventorySystem.UI.Item.ItemViewSlotRestrictions;
    using System;
    using UnityEngine;

    /// <summary>
    /// The component used to view an item slot collection, for example an equipment window.
    /// </summary>
    public class ItemSlotCollectionView : ItemViewSlotsContainer, IDatabaseSwitcher
    {
        [Tooltip("Update UI when inventory updates.")]
        [SerializeField] protected ItemCollectionID m_ItemCollectionID = new ItemCollectionID(null, ItemCollectionPurpose.Equipped);
        [Tooltip("Automatically set the item view slot category restriction to match the ItemSlot category.")]
        [SerializeField] protected bool m_SetItemViewSlotRestrictions = true;
        [Tooltip("The slots that belong to the collection.")]
        [SerializeField] protected ItemSlotSet m_ItemSlotSet;
        [Tooltip("Add Category Restrictions if missing.")]
        [SerializeField] protected bool m_AddViewSlotCategoryRestrictions = true;
        [Tooltip("The item view slots that map to item slots by index.")]
        [SerializeField] internal ItemViewSlot[] m_ItemSlotItemViewSlots;

        protected ItemSlotCollection m_ItemSlotCollection;

        public ItemSlotCollection ItemSlotCollection => m_ItemSlotCollection;
        public ItemUser ItemUser => Inventory.ItemUser;
        public ItemSlotSet ItemSlotSet {
            get { return m_ItemSlotSet; }
            internal set { m_ItemSlotSet = value; }
        }
        
        protected override ItemViewSlot[] RetrieveItemViewSlots()
        {
            ResizeItemViewSlotsToItemSlotsSetCount();
            
            if (m_ItemSlotItemViewSlots == null) {
                return base.RetrieveItemViewSlots();
            }

            return m_ItemSlotItemViewSlots;
        }

        /// <summary>
        /// This method is called before the Inventory is set to the Item View Slots Container.
        /// </summary>
        protected override void OnInitializeBeforeSettingInventory()
        {
            if (m_SetItemViewSlotRestrictions) { SetItemViewSlotRestrictions(); }
        }

        /// <summary>
        /// Resize the length of the Item View Slots.
        /// </summary>
        /// <returns>Return true if the length changed.</returns>
        public bool ResizeItemViewSlotsToItemSlotsSetCount()
        {
            if (m_ItemSlotSet == null || m_ItemSlotSet.ItemSlots == null) {
                if (Application.isPlaying) { Debug.LogError("The item slot set cannot be null", gameObject); }

                return false;
            }

            if (m_ItemSlotItemViewSlots == null) {
                m_ItemSlotItemViewSlots = new ItemViewSlot[m_ItemSlotSet.ItemSlots.Count];
                return true;
            }

            if (m_ItemSlotItemViewSlots.Length != m_ItemSlotSet.ItemSlots.Count) {
                Array.Resize(ref m_ItemSlotItemViewSlots, m_ItemSlotSet.ItemSlots.Count);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Set the item view slot restrictions.
        /// </summary>
        public void SetItemViewSlotRestrictions()
        {
            if (m_ItemSlotSet == null) { return; }

            for (int i = 0; i < m_ItemSlotItemViewSlots.Length; i++) {
                var itemViewSlot = m_ItemSlotItemViewSlots[i];
                if (itemViewSlot == null) {
                    return;
                    //Debug.LogError($"The item view slot at index {i} is null, the item view slots cannot be null",gameObject);
                }

                var itemSlot = m_ItemSlotSet.GetSlot(i);
                if (itemSlot.HasValue == false) { return; }

                if (m_AddViewSlotCategoryRestrictions) {
                    var itemViewSlotRestriction = itemViewSlot.GetComponent<ItemViewSlotCategoryRestriction>();
                    if (itemViewSlotRestriction == null) {
                        itemViewSlotRestriction = itemViewSlot.gameObject.AddComponent<ItemViewSlotCategoryRestriction>();
                    }
                    itemViewSlotRestriction.ItemCategory = itemSlot.Value.Category;
                }
            }
        }

        /// <summary>
        /// Get the item view slot from the item slot.
        /// </summary>
        /// <param name="itemSlot">The item slot.</param>
        /// <returns>The item view slot.</returns>
        public ItemViewSlot GetItemViewSlot(ItemSlot itemSlot)
        {
            return GetItemViewSlot(itemSlot.Name);
        }

        /// <summary>
        /// Get the item view slot from the item slot name.
        /// </summary>
        /// <param name="slotName">The item slot name.</param>
        /// <returns>The item view slot.</returns>
        public ItemViewSlot GetItemViewSlot(string slotName)
        {
            for (int i = 0; i < m_ItemSlotSet.ItemSlots.Count; i++) {
                if (slotName == m_ItemSlotSet.ItemSlots[i].Name) {
                    return m_ItemSlotItemViewSlots[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Get the item slot from the item view slot.
        /// </summary>
        /// <param name="itemViewSlot">The item view slot.</param>
        /// <returns>The item slot.</returns>
        public ItemSlot? GetItemSlot(ItemViewSlot itemViewSlot)
        {
            for (int i = 0; i < m_ItemSlotItemViewSlots.Length; i++) {
                if (itemViewSlot == m_ItemSlotItemViewSlots[i]) {
                    return m_ItemSlotSet.GetSlot(i);
                }
            }

            return null;
        }

        /// <summary>
        /// A new Inventory was set.
        /// </summary>
        protected override void OnInventoryChanged(Inventory previousInventory, Inventory newInventory)
        {
            if (m_Inventory == null) {
                m_ItemSlotCollection = null;
                return;
            }

            m_ItemSlotCollection = m_Inventory.GetItemCollection(m_ItemCollectionID) as ItemSlotCollection;
            if (m_ItemSlotCollection == null) {
                Debug.LogError($"The inventory '{m_Inventory}' does not contain an item slot collection for the ID provided in the ItemSlotCollectionView", gameObject);
                m_Inventory = null;
                return;
            }

            if (m_ItemSlotSet != m_ItemSlotCollection.ItemSlotSet) {
                Debug.LogError($"The item slot collection '{m_ItemSlotCollection}' has an Item Slot Set '{m_ItemSlotCollection.ItemSlotSet}' " +
                               $"which does not match with the Item Slot Set '{m_ItemSlotSet}' provided in the ItemSlotCollectionView", gameObject);
                m_Inventory = null;
                m_ItemSlotCollection = null;
                return;
            }
        }

        #region Item View Slot Container Overrides

        /// <summary>
        /// Remove the item.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="index">The index.</param>
        /// <returns>The item info added.</returns>
        public override ItemInfo RemoveItem(ItemInfo itemInfo, int index)
        {
            return m_ItemSlotCollection.RemoveItem(itemInfo);
        }

        /// <summary>
        /// Can the item be added to the item view slot.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="index">The index.</param>
        /// <returns>True if the item can be added.</returns>
        public override bool CanAddItem(ItemInfo itemInfo, int index)
        {
            var canAddBase = base.CanAddItem(itemInfo, index);
            if (canAddBase == false) { return false; }

            if (Inventory.CanAddItem(itemInfo, m_ItemSlotCollection) == null) { return false; }

            return true;
        }

        /// <summary>
        /// Add the item to the index.
        /// </summary>
        /// <param name="itemInfo">The item info to add.</param>
        /// <param name="index">The index to add the item to.</param>
        /// <returns>The item info actually added.</returns>
        public override ItemInfo AddItem(ItemInfo itemInfo, int index)
        {
            if (CanAddItem(itemInfo, index) == false) {
                return (0, null, null);
            }

            var addedItem = m_ItemSlotCollection.AddItem(itemInfo, index);
            AssignItemToSlot(addedItem, index);
            return itemInfo;
        }

        /// <summary>
        /// Draw the item view slot.
        /// </summary>
        protected override void DrawInternal()
        {
            if (m_Inventory == null || m_ItemSlotCollection == null) {
                Debug.LogWarning("The inventory or the item slot collection for the item slot collection view is null", gameObject);
                for (int i = 0; i < m_ItemViewSlots.Length; i++) {
                    AssignItemToSlot(ItemInfo.None, i);
                }
                return;
            }

            for (int i = 0; i < m_ItemViewSlots.Length; i++) {
                var itemInSlot = m_ItemSlotCollection.GetItemStackAtSlot(i);
                AssignItemToSlot(new ItemInfo(itemInSlot), i);
            }
        }
        
        /// <summary>
        /// Can the item be moved from one index to another.
        /// </summary>
        /// <param name="sourceIndex">The source index.</param>
        /// <param name="destinationIndex">The destination index.</param>
        /// <returns>True if the item can be moved.</returns>
        public override bool CanMoveItem(int sourceIndex, int destinationIndex)
        {
            if (sourceIndex < 0 || sourceIndex >= m_ItemViewSlots.Length) { return false; }
            if (destinationIndex < 0 || destinationIndex >= m_ItemViewSlots.Length) { return false; }
            
            return m_ItemViewSlots[sourceIndex].CanContain(m_ItemViewSlots[destinationIndex].ItemInfo)
                   && m_ItemViewSlots[destinationIndex].CanContain(m_ItemViewSlots[sourceIndex].ItemInfo);
        }

        /// <summary>
        /// Move the item from one index to another.
        /// </summary>
        /// <param name="sourceIndex">The source index.</param>
        /// <param name="destinationIndex">The destination index.</param>
        public override void MoveItem(int sourceIndex, int destinationIndex)
        {
            if (CanMoveItem(sourceIndex, destinationIndex) == false) { return; }

            var sourceItem = m_ItemSlotCollection.RemoveItem(sourceIndex);
            var destination = m_ItemSlotCollection.RemoveItem(destinationIndex);
            
            m_ItemSlotCollection.AddItem(sourceItem, destinationIndex);
            m_ItemSlotCollection.AddItem(destination, sourceIndex);
            
            AssignItemToSlot(destination, sourceIndex);
            AssignItemToSlot(sourceItem, destinationIndex);
        }

        #endregion

        /// <summary>
        /// Check if the object contained by this component are part of the database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>True if all the objects in the component are part of that database.</returns>
        bool IDatabaseSwitcher.IsComponentValidForDatabase(InventorySystemDatabase database)
        {
            if (database == null) { return false; }

            return true;
        }

        /// <summary>
        /// Replace any object that is not in the database by an equivalent object in the specified database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>The objects that have been changed.</returns>
        ModifiedObjectWithDatabaseObjects IDatabaseSwitcher.ReplaceInventoryObjectsBySelectedDatabaseEquivalents(InventorySystemDatabase database)
        {
            if (database == null) { return null; }

            return null;
        }
    }
}