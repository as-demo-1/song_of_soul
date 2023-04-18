/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Grid
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.SaveSystem;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// A saver component that saves the item shape grid Data.
    /// </summary>
    public class ItemShapeGridDataSaver : SaverBase
    {
        /// <summary>
        /// The inventory save data.
        /// </summary>
        [System.Serializable]
        public struct GridElementSaveData
        {
            public ItemStackSaveData ItemStackData;
            public bool IsAnchor;

            public GridElementSaveData(ItemStackSaveData itemStackData, bool isAnchor)
            {
                ItemStackData = itemStackData;
                IsAnchor = isAnchor;
            }

            public GridElementSaveData(ItemShapeGridData.GridElementData gridElementData)
            {
                IsAnchor = gridElementData.IsAnchor;
                ItemStackData = new ItemStackSaveData(gridElementData.ItemStack);
            }
        }
        
        /// <summary>
        /// The inventory save data.
        /// </summary>
        [System.Serializable]
        public struct SaveData
        {
            public GridElementSaveData[] GridSaveData;
        }
        
        [Tooltip("The Item Shape Grid Data to save.")]
        [SerializeField] protected ItemShapeGridData m_ItemShapeGridData;

        /// <summary>
        /// Initialize the component.
        /// </summary>
        protected override void Awake()
        {
            if (m_ItemShapeGridData == null) { m_ItemShapeGridData = GetComponent<ItemShapeGridData>(); }
            base.Awake();
        }
        
        /// <summary>
        /// Serialize the grid data.
        /// </summary>
        /// <returns>The serialized data.</returns>
        public override Serialization SerializeSaveData()
        {
            if (m_ItemShapeGridData == null) { return null; }
            
            var allGridElementSaveData = new GridElementSaveData[m_ItemShapeGridData.GridSizeCount];

            for (int i = 0; i < allGridElementSaveData.Length; i++) {

                var gridElementData = m_ItemShapeGridData.GetElementAt(i);
                
                allGridElementSaveData[i] = new GridElementSaveData(gridElementData);
            }
            
            var saveData = new SaveData {
                GridSaveData = allGridElementSaveData
            };

            return Serialization.Serialize(saveData);
        }

        /// <summary>
        /// Deserialize the saved data.
        /// </summary>
        /// <param name="serializedSaveData">The serialized save data.</param>
        public override void DeserializeAndLoadSaveData(Serialization serializedSaveData)
        {
            if (m_ItemShapeGridData == null) { return; }
            
            var savedData = serializedSaveData.DeserializeFields(MemberVisibility.All) as SaveData?;

            if (savedData.HasValue == false) { return; }

            var gridSaveData = savedData.Value.GridSaveData;

            if (gridSaveData == null || gridSaveData.Length != m_ItemShapeGridData.GridSizeCount) { return; }

            var inventory = m_ItemShapeGridData.Inventory;
            
            var gridData = new ItemShapeGridData.GridElementData[m_ItemShapeGridData.GridSizeCount];

            var allItems = inventory.AllItemInfos;
            for (int i = 0; i < allItems.Count; i++) {
                var itemStack = allItems[i].ItemStack;
                for (int j = 0; j < gridSaveData.Length; j++) {
                    var gridElementSaveData = gridSaveData[j];
                    
                    if(gridElementSaveData.ItemStackData.Match(itemStack) == false){continue; }
                    
                    var gridElement = new ItemShapeGridData.GridElementData(itemStack,gridElementSaveData.IsAnchor);
                    gridData[j] = gridElement;
                }
            }
            
            m_ItemShapeGridData.SetNewGridData(gridData);
        }
    }
}