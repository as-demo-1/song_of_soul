/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.SaveSystem
{
    using System;
    using System.Collections.Generic;
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.UI.Item;
    using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// A component used to save the index of items inside an inventory.
    /// </summary>
    public class InventoryGridSaver : SaverBase
    {
        [Serializable]
        public struct ItemStackIndexSaveData
        {
            public ItemStackSaveData ItemStackSaveData;
            public int Index;
        }
        
        /// <summary>
        /// The inventory save data.
        /// </summary>
        [System.Serializable]
        public struct InventoryGridIndexerSaveData
        {
            public ItemStackIndexSaveData[] ItemStackIndexSaveDatas;
        }
        
        /// <summary>
        /// The inventory save data.
        /// </summary>
        [System.Serializable]
        public struct InventoryGridSaveData
        {
            public InventoryGridIndexerSaveData InventoryGridIndexerSaveData;
            public InventoryGridIndexerSaveData[] TabsIndexerSaveData;
        }

        [Tooltip("The inventory Grid.")]
        [SerializeField] protected InventoryGrid m_InventoryGrid;

        [Tooltip("The Tab Control Binding.")] 
        [SerializeField] protected InventoryGridTabControlBinding m_TabControlBinding;

        /// <summary>
        /// Initialize the component.
        /// </summary>
        protected override void Awake()
        {
            if (m_InventoryGrid == null) { m_InventoryGrid = GetComponent<InventoryGrid>(); }

            if (m_TabControlBinding == null) { m_TabControlBinding = GetComponent<InventoryGridTabControlBinding>(); }

            base.Awake();
        }

        /// <summary>
        /// Serialize the save data.
        /// </summary>
        /// <returns>The serialized data.</returns>
        public override Serialization SerializeSaveData()
        {
            if (m_InventoryGrid == null) { return null; }

            var inventoryGridIndexerSaveData = GetIndexerSaveData(m_InventoryGrid.InventoryGridIndexer);

            InventoryGridIndexerSaveData[] tabsIndexerSaveData = null;
            if (m_TabControlBinding != null) {
                var tabs = m_TabControlBinding.m_TabControl.TabToggles;

                tabsIndexerSaveData = new InventoryGridIndexerSaveData[tabs.Count];
                for (int i = 0; i < tabs.Count; i++) {
                    var inventoryTabData = tabs[i].gameObject.GetCachedComponent<InventoryTabData>();
                    if(inventoryTabData == null){continue;}

                    if (m_TabControlBinding.m_TabControl.TabIndex == i) {
                        //Save the current inventory grid indexer because the tab indexer wasn't cached yet.
                        tabsIndexerSaveData[i] = GetIndexerSaveData(m_InventoryGrid.InventoryGridIndexer);
                    } else {
                        tabsIndexerSaveData[i] = GetIndexerSaveData(inventoryTabData.Indexer);
                    }
                    
                }
            }

            var saveData = new InventoryGridSaveData()
            {
                InventoryGridIndexerSaveData = inventoryGridIndexerSaveData,
                TabsIndexerSaveData = tabsIndexerSaveData
            };

            return Serialization.Serialize(saveData);
        }

        /// <summary>
        /// Get the Indexer save data.
        /// </summary>
        /// <param name="inventoryGridIndexer">The inventory grid indexer.</param>
        /// <returns>The save data for the inventory grid indexer.</returns>
        private InventoryGridIndexerSaveData GetIndexerSaveData(InventoryGridIndexer inventoryGridIndexer)
        {
            var indexedItems = inventoryGridIndexer.IndexedItems;
            var itemStackIndexSaveDatas = new ItemStackIndexSaveData[indexedItems.Count];

            int count = 0;
            foreach (var keyValuePair in indexedItems) {
                var itemStack = keyValuePair.Key;
                var index = keyValuePair.Value.index;

                itemStackIndexSaveDatas[count] = new ItemStackIndexSaveData()
                {
                    ItemStackSaveData = new ItemStackSaveData(itemStack),
                    Index = index
                };
                count++;
            }

            var saveData = new InventoryGridIndexerSaveData
            {
                ItemStackIndexSaveDatas = itemStackIndexSaveDatas
            };
            return saveData;
        }

        /// <summary>
        /// Deserialize and load the save data.
        /// </summary>
        /// <param name="serializedSaveData">The serialized save data.</param>
        public override void DeserializeAndLoadSaveData(Serialization serializedSaveData)
        {
            if (m_InventoryGrid == null) { return; }

            var savedData = serializedSaveData.DeserializeFields(MemberVisibility.All) as InventoryGridSaveData?;

            if (savedData.HasValue == false) {
                return;
            }
            
            var inventory = m_InventoryGrid.Inventory;

            if(inventory == null){ return; }

            var inventoryGridSaveData = savedData.Value;

            if (m_TabControlBinding == null) {
                var indexedItems = GetIndexedItems(inventory, inventoryGridSaveData.InventoryGridIndexerSaveData);

                m_InventoryGrid.InventoryGridIndexer.SetIndexItems(indexedItems);
                m_InventoryGrid.Draw();
                return;
            }

            if (inventoryGridSaveData.TabsIndexerSaveData == null) {
                return;
            }
            
            var tabs = m_TabControlBinding.m_TabControl.TabToggles;

            for (int i = 0; i < tabs.Count; i++) {
                var inventoryTabData = tabs[i].gameObject.GetCachedComponent<InventoryTabData>();
                if(inventoryTabData == null){continue;}

                if (inventoryGridSaveData.TabsIndexerSaveData.Length <= i) {
                    break;
                }
                
                var indexedItems = GetIndexedItems(inventory, inventoryGridSaveData.TabsIndexerSaveData[i]);
                
                inventoryTabData.Indexer.SetIndexItems(indexedItems);

                //The current tab indexed item must be set on the inventory directly.
                if (m_TabControlBinding.m_TabControl.TabIndex == i) {
                    m_InventoryGrid.InventoryGridIndexer.SetIndexItems(indexedItems);
                }
            }
            m_InventoryGrid.Draw();
        }

        /// <summary>
        /// Get the indexer items.
        /// </summary>
        /// <param name="inventory">The inventory.</param>
        /// <param name="inventoryGridIndexerSaveData">The inventory grid indexer save data.</param>
        /// <returns></returns>
        private Dictionary<ItemStack, (int index, ItemInfo itemInfo)> GetIndexedItems(Inventory inventory, InventoryGridIndexerSaveData inventoryGridIndexerSaveData)
        {
            var itemStackIndexSaveDatas = inventoryGridIndexerSaveData.ItemStackIndexSaveDatas;
            if (itemStackIndexSaveDatas == null) {
                return null;
            }
            var indexedItems = new Dictionary<ItemStack, (int index, ItemInfo itemInfo)>();

            var allItems = inventory.AllItemInfos;
            for (int i = 0; i < allItems.Count; i++) {
                var itemStack = allItems[i].ItemStack;
                for (int j = 0; j < itemStackIndexSaveDatas.Length; j++) {
                    var itemStackIndexedSaveData = itemStackIndexSaveDatas[j];

                    if (itemStackIndexedSaveData.ItemStackSaveData.Match(itemStack) == false) {
                        continue;
                    }

                    if (indexedItems.ContainsKey(itemStack)) {
                        continue;
                    }

                    indexedItems[itemStack] = (itemStackIndexedSaveData.Index, (ItemInfo)itemStack);
                }
            }

            return indexedItems;
        }
    }
}