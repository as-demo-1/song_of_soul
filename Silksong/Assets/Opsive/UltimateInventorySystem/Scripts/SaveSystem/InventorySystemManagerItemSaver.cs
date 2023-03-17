/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

//#define DEBUG_ITEM_SAVER

namespace Opsive.UltimateInventorySystem.SaveSystem
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The Inventory System Manager Saver 
    /// </summary>
    public class InventorySystemManagerItemSaver : SaverBase
    {
        //Save Last
        public override int SavePriority => 10000;

        //Load First
        public override int LoadPriority => -10000;

        /// <summary>
        /// The item save data.
        /// </summary>
        [System.Serializable]
        public struct SaverItemsIDs
        {
            public string SaverFullKey;
            public uint[] ItemIDs;
        }

        /// <summary>
        /// The item save data.
        /// </summary>
        [System.Serializable]
        public struct ItemsSaveData
        {
            public Item[] Items;
            public SaverItemsIDs[] SaversItemIDs;
        }

        [Tooltip("You MUST use this option if you are nesting items within item attributes (only works ONE level).")]
        [SerializeField] protected bool m_UsingNestedItems = true;
        [Tooltip("Remember the names of the items saved, this saves the names of ALL the items saved.")]
        [SerializeField] protected bool m_SaveItemNames = true;

        protected List<SaverItemsIDs> m_SaversItemIDs;
        protected ItemsSaveData m_CurrentSaveData;

        public ItemsSaveData CurrentSaveData => m_CurrentSaveData;

        /// <summary>
        /// Set the Item IDs of the items that should be saved by each saver.
        /// </summary>
        /// <param name="saverFullKey">The saver full key.</param>
        /// <param name="listOfItemIDsToSave">The list of items to save for that saver.</param>
        public virtual void SetItemsToSave(string saverFullKey, ListSlice<uint> listOfItemIDsToSave)
        {
            if (m_SaversItemIDs == null) {
                m_SaversItemIDs = new List<SaverItemsIDs>();
            }

            var newSaverItemID = new SaverItemsIDs() {
                SaverFullKey = saverFullKey,
                ItemIDs = listOfItemIDsToSave.ToArray()
            };

            var foundMatch = false;
            for (int i = 0; i < m_SaversItemIDs.Count; i++) {
                if (m_SaversItemIDs[i].SaverFullKey == saverFullKey) {
                    foundMatch = true;
                    m_SaversItemIDs[i] = newSaverItemID;
                }
            }

            if (foundMatch == false) {
                m_SaversItemIDs.Add(newSaverItemID);
            }
        }

        /// <summary>
        /// Remove a key from the save data.
        /// </summary>
        /// <param name="saverFullKey">The saver full key to remove.</param>
        public void RemoveKey(string saverFullKey)
        {
            if (m_SaversItemIDs == null) { return; }

            int index = -1;
            for (int i = 0; i < m_SaversItemIDs.Count; i++) {
                if (m_SaversItemIDs[i].SaverFullKey != saverFullKey) { continue; }

                index = i;
                break;
            }

            if (index == -1) { return; }

            m_SaversItemIDs.RemoveAt(index);
        }

        /// <summary>
        /// Serialize the save data.
        /// </summary>
        /// <returns>The serialize data.</returns>
        public override Serialization SerializeSaveData()
        {
#if DEBUG_ITEM_SAVER
            Debug.Log("Save Item Saver");
#endif

            if (InventorySystemManager.IsNull) { return null; }

            if (m_SaversItemIDs == null || m_SaversItemIDs.Count == 0) {
                //Nothing to save.
                return null;
            }

            var itemsToSave = new Stack<Item>();

            for (int i = 0; i < m_SaversItemIDs.Count; i++) {
                var itemIDs = m_SaversItemIDs[i].ItemIDs;

                for (int j = 0; j < itemIDs.Length; j++) {
                    var itemID = itemIDs[j];

                    var item = InventorySystemManager.GetItem(itemID);

                    //save the item if it not already saved
                    if (itemsToSave.Contains(item)) {
                        continue;
                    }

                    //The item must be serialized such that its attributes can be saved.
                    item.Serialize();
                    itemsToSave.Push(item);

#if DEBUG_ITEM_SAVER
                    Debug.Log("Save "+item);
#endif

                    if (m_UsingNestedItems) { AddNestedItemsToSave(item, itemsToSave); }
                }
            }

            m_CurrentSaveData = new ItemsSaveData {
                Items = itemsToSave.ToArray(),
                SaversItemIDs = m_SaversItemIDs.ToArray()
            };

            return Serialization.Serialize(m_CurrentSaveData);
        }

        /// <summary>
        /// Add nested items to the save data.
        /// </summary>
        /// <param name="item">The item containing nested items.</param>
        /// <param name="itemsToSave">The item list.</param>
        protected virtual void AddNestedItemsToSave(Item item, Stack<Item> itemsToSave)
        {
            for (int i = 0; i < item.ItemAttributeCollection.Count; i++) {
                var attribute = item.ItemAttributeCollection[i];

                if (attribute.GetValueType() != typeof(ItemAmounts)) { continue; }

                // Don't save if it inherits.
                if (attribute.VariantType == VariantType.Inherit) { continue; }

                var itemAmounts = (ItemAmounts)attribute.GetValueAsObject();

                for (int j = 0; j < itemAmounts.Count; j++) {
                    var nestedItem = itemAmounts[j].Item;

                    if (nestedItem == null || itemsToSave.Contains(nestedItem)) { continue; }

                    nestedItem.Serialize();
                    itemsToSave.Push(nestedItem);
                }
            }
        }

        /// <summary>
        /// Deserialize and load the save data.
        /// </summary>
        /// <param name="serializedSaveData">The serialized save data.</param>
        public override void DeserializeAndLoadSaveData(Serialization serializedSaveData)
        {
#if DEBUG_ITEM_SAVER
            Debug.Log("Load Item Saver");
#endif

            if (InventorySystemManager.IsNull) { return; }

            var savedData = serializedSaveData.DeserializeFields(MemberVisibility.All) as ItemsSaveData?;

            if (savedData.HasValue == false) {
                m_CurrentSaveData = new ItemsSaveData();
                m_SaversItemIDs = new List<SaverItemsIDs>();
                return;
            }

            m_CurrentSaveData = savedData.Value;

            m_SaversItemIDs = new List<SaverItemsIDs>();
            if (m_CurrentSaveData.SaversItemIDs != null) {
                m_SaversItemIDs.AddRange(m_CurrentSaveData.SaversItemIDs);
            }

            for (int i = 0; i < m_CurrentSaveData.Items.Length; i++) {
                var item = m_CurrentSaveData.Items[i];

                var itemNameSaved = item.name;
                item.Initialize(false);

                if (m_SaveItemNames) {
                    item.name = itemNameSaved;
                }

                if (InventorySystemManager.ItemRegister.TryGetValue(item.ID, out var registeredItem)) {
                    var valueEquivalent = Item.AreValueEquivalent(item, registeredItem);

                    if (m_SaveItemNames) {
                        registeredItem.name = item.name;
                    }

                    //An Item with the same ID is already loaded but has different values, replace attribute values
                    if (valueEquivalent == false) {

                        for (int j = 0; j < item.ItemAttributeCollection.Count; j++) {
                            var attribute = item.ItemAttributeCollection[j];
                            //Don't copy inherited values.
                            if (attribute.VariantType == VariantType.Inherit) { continue; }

                            registeredItem.OverrideAttribute(attribute);
                        }

                    }
                } else {
                    InventorySystemManager.ItemRegister.Register(ref item);
                }

                // Assign the item back to keep the current save data info relevant.
                m_CurrentSaveData.Items[i] = item;

#if DEBUG_ITEM_SAVER
                Debug.Log("Loaded "+item);
#endif
            }

            if (m_UsingNestedItems) {
                LoadNestedItems(m_CurrentSaveData.Items);
            }
        }

        /// <summary>
        /// Load Nested items.
        /// </summary>
        /// <param name="items">The list of items which may contain nested items.</param>
        protected virtual void LoadNestedItems(Item[] items)
        {

            for (int i = 0; i < items.Length; i++) {
                var item = items[i];

                for (int j = 0; j < item.ItemAttributeCollection.Count; j++) {
                    var attribute = item.ItemAttributeCollection[j];

                    if (attribute.GetValueType() != typeof(ItemAmounts)) { continue; }

                    // Don't load if it inherits.
                    if (attribute.VariantType == VariantType.Inherit) { continue; }

                    var itemAmounts = (ItemAmounts)attribute.GetValueAsObject();

                    for (int k = 0; k < itemAmounts.Count; k++) {
                        var nestedItem = itemAmounts[k].Item;

                        if (nestedItem == null) { continue; }

                        itemAmounts[k] = new ItemAmount(
                            InventorySystemManager.GetItem(nestedItem.ID),
                            itemAmounts[k].Amount);
                    }
                }
            }


        }
    }
}