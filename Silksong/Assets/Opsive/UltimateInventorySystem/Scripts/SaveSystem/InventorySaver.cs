/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.SaveSystem
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using System.Collections.Generic;
    using Opsive.Shared.Events;
    using UnityEngine;

    /// <summary>
    /// A saver component that saves the content of an inventory.
    /// </summary>
    public class InventorySaver : SaverBase
    {
        //A higher priority (-1 > 1) will be saved first.
        public override int SavePriority => -100;
        //A higher priority (-1 > 1) will be loaded first.
        public override int LoadPriority => -100;
        
        /// <summary>
        /// The inventory save data.
        /// </summary>
        [System.Serializable]
        public struct InventorySaveData
        {
            public IDAmountSaveData[][] ItemIDAmountsPerCollection;
        }

        [Tooltip("The inventory.")]
        [SerializeField] protected Inventory m_Inventory;
        [Tooltip("Is the save data added to the loadout of does it overwrite it.")]
        [SerializeField] protected bool m_Additive;

        /// <summary>
        /// Initialize the component.
        /// </summary>
        protected override void Awake()
        {
            if (m_Inventory == null) { m_Inventory = GetComponent<Inventory>(); }
            base.Awake();
        }

        /// <summary>
        /// Serialize the save data.
        /// </summary>
        /// <returns>The serialized data.</returns>
        public override Serialization SerializeSaveData()
        {
            if (m_Inventory == null) { return null; }

            var itemCollectionCount = m_Inventory.GetItemCollectionCount();
            var newItemAmountsArray = new IDAmountSaveData[itemCollectionCount][];
            var listItemIDs = new List<uint>();

            for (int i = 0; i < itemCollectionCount; i++) {

                var itemCollection = m_Inventory.GetItemCollection(i);

                var allItemAmounts = itemCollection.GetAllItemStacks();

                var itemAmounts = new IDAmountSaveData[allItemAmounts.Count];
                for (int j = 0; j < itemAmounts.Length; j++) {

                    var itemAmount = allItemAmounts[j];

                    itemAmounts[j] = new IDAmountSaveData() {
                        ID = itemAmount.Item.ID,
                        Amount = itemAmount.Amount
                    };

                    listItemIDs.Add(itemAmount.Item.ID);
                }

                newItemAmountsArray[i] = itemAmounts;
            }

            SaveSystemManager.InventorySystemManagerItemSaver.SetItemsToSave(FullKey, listItemIDs);

            var saveData = new InventorySaveData {
                ItemIDAmountsPerCollection = newItemAmountsArray
            };

            return Serialization.Serialize(saveData);
        }

        /// <summary>
        /// Deserialize and load the save data.
        /// </summary>
        /// <param name="serializedSaveData">The serialized save data.</param>
        public override void DeserializeAndLoadSaveData(Serialization serializedSaveData)
        {
            if (m_Inventory == null) { return; }
            
            if (!m_Additive) {
                var itemCollectionCount = m_Inventory.GetItemCollectionCount();

                for (int i = 0; i < itemCollectionCount; i++) {
                    m_Inventory.GetItemCollection(i).RemoveAll();
                }
            }

            var savedData = serializedSaveData.DeserializeFields(MemberVisibility.All) as InventorySaveData?;

            if (savedData.HasValue == false) {
                return;
            }

            var inventorySaveData = savedData.Value;

            if (inventorySaveData.ItemIDAmountsPerCollection == null) { return; }
            
            EventHandler.ExecuteEvent(m_Inventory.gameObject, EventNames.c_InventoryGameObject_InventoryMonitorListen_Bool, false);


            for (int i = 0; i < inventorySaveData.ItemIDAmountsPerCollection.Length; i++) {
                var itemIDAmounts = inventorySaveData.ItemIDAmountsPerCollection[i];
                var itemAmounts = new ItemAmount[itemIDAmounts.Length];
                for (int j = 0; j < itemIDAmounts.Length; j++) {
                    if (InventorySystemManager.ItemRegister.TryGetValue(itemIDAmounts[j].ID, out var item) == false) {
                        Debug.LogWarning($"Saved Item ID {itemIDAmounts[j].ID} could not be retrieved from the Inventory System Manager.");
                        continue;
                    }
                    itemAmounts[j] = new ItemAmount(item, itemIDAmounts[j].Amount);
                }

                var itemCollection = m_Inventory.GetItemCollection(i);
                if (itemCollection == null) {
                    Debug.LogWarning("Item Collection from save data is missing in the scene.");
                } else {
                    m_Inventory.GetItemCollection(i).AddItems(itemAmounts);
                }
            }
            
            EventHandler.ExecuteEvent(m_Inventory.gameObject, EventNames.c_InventoryGameObject_InventoryMonitorListen_Bool, true);

        }
    }
}