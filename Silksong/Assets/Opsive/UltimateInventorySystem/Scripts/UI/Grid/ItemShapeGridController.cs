/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Grid
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;
    using System.Collections.Generic;

    [HideFromItemRestrictionSetAttribute]
    public class ItemShapeGridController : MonoBehaviour, IItemRestriction
    {
        [Tooltip("Add the item even if it does not match the filters of any grids.")]
        [SerializeField] protected bool m_NoGridAddItem;
        [Tooltip("The item shape grid data list..")]
        [SerializeField] internal List<ItemShapeGridData> m_ItemShapeGridData;
        [Tooltip("The attribute name for the shape attribute.")]
        [SerializeField] protected string m_ShapeAttributeName = "Shape";

        protected Inventory m_Inventory;

        public Inventory Inventory => m_Inventory;
        public string ShapeAttributeName => m_ShapeAttributeName;

        /// <summary>
        /// Initialize the Grid Controller.
        /// </summary>
        /// <param name="inventory">The inventory.</param>
        /// <param name="force">Force the initialize.</param>
        public void Initialize(IInventory inventory, bool force)
        {
            m_Inventory = inventory as Inventory;

            RetrieveItemShapeGridData();

            for (int i = 0; i < m_ItemShapeGridData.Count; i++) {
                var gridShapeHandler = m_ItemShapeGridData[i];

                gridShapeHandler.Initialize(this);
            }

            EventHandler.RegisterEvent<ItemInfo, ItemStack>(m_Inventory, EventNames.c_Inventory_OnAdd_ItemInfo_ItemStack, OnItemAdded);
            EventHandler.RegisterEvent<ItemInfo>(m_Inventory, EventNames.c_Inventory_OnRemove_ItemInfo, OnItemRemoved);
        }

        /// <summary>
        /// Retrieve the Item Shape Grid Data that are on the component and add them to the list of grid datas.
        /// </summary>
        public void RetrieveItemShapeGridData()
        {
            var localItemShapeGridDatas = GetComponents<ItemShapeGridData>();
            for (int i = 0; i < localItemShapeGridDatas.Length; i++) {
                var localItemShapeGridData = localItemShapeGridDatas[i];
                var foundMatch = false;
                for (int j = 0; j < m_ItemShapeGridData.Count; j++) {
                    if (localItemShapeGridData != m_ItemShapeGridData[j]) { continue; }

                    foundMatch = true;
                    break;
                }
                if(foundMatch){continue;}
                m_ItemShapeGridData.Add(localItemShapeGridData);
            }
        }

        /// <summary>
        /// Get the Grid Data with the ID.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <returns>The Grid data for that ID.</returns>
        public ItemShapeGridData GetGridDataWithID(int id)
        {
            for (int i = 0; i < m_ItemShapeGridData.Count; i++) {
                if (m_ItemShapeGridData[i].ID == id) {
                    return m_ItemShapeGridData[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Handle an item being removed from the inventory.
        /// </summary>
        /// <param name="itemInfoRemoved">The item info that was removed.</param>
        private void OnItemRemoved(ItemInfo itemInfoRemoved)
        {
            var gridData = GetGridDataForItem((ItemInfo)itemInfoRemoved);

            if (gridData == null) { return; }

            gridData.OnItemRemoved(itemInfoRemoved);
        }

        /// <summary>
        /// Handle and item being added to the inventory.
        /// </summary>
        /// <param name="originItemInfo">The original Item Info.</param>
        /// <param name="itemInfoAdded">The Item Stack where the item was added.</param>
        private void OnItemAdded(ItemInfo originItemInfo, ItemStack itemInfoAdded)
        {
            var gridData = GetGridDataForItem((ItemInfo)itemInfoAdded);

            if (gridData == null) { return; }

            gridData.OnItemAdded(originItemInfo, itemInfoAdded);
        }

        /// <summary>
        /// Should the item be added and how?
        /// </summary>
        /// <param name="itemInfo">The item Info to add.</param>
        /// <param name="receivingCollection">The item Collection where the item should be added to.</param>
        /// <returns>The Item Info that can be added.</returns>
        public ItemInfo? CanAddItem(ItemInfo itemInfo, ItemCollection receivingCollection)
        {
            var itemInfoPreview = new ItemInfo(itemInfo.ItemAmount, receivingCollection, itemInfo.ItemStack);

            // set the item info as if it was already added in the receiving collection to pass the conditions.
            var previewItemInfo =
                new ItemInfo(itemInfoPreview.ItemAmount, receivingCollection, itemInfoPreview.ItemStack);
            var gridData = GetPotentialGridDataForItem(previewItemInfo, receivingCollection);

            if (gridData == null) {
                if (m_NoGridAddItem) { return itemInfo; }
                return null;
            }

            if (gridData.TryFindAvailablePosition(itemInfoPreview, out var position)) {
                return itemInfo;
            }

            return null;
        }

        /// <summary>
        /// The remove condition.
        /// </summary>
        /// <param name="itemInfo">The item Info to remove.</param>
        /// <returns>The item Info to remove.</returns>
        public ItemInfo? CanRemoveItem(ItemInfo itemInfo)
        {
            return itemInfo;
        }

        /// <summary>
        /// Predict the grid data in which the item will go if it was added to the inventory.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="receivingCollection">The receiving Item Collection.</param>
        /// <returns>The grid data where the item could be set.</returns>
        public ItemShapeGridData GetPotentialGridDataForItem(ItemInfo itemInfo, ItemCollection receivingCollection)
        {
            return GetGridDataForItemInternal(itemInfo, receivingCollection, true);
        }

        /// <summary>
        /// Get the Grid Data for the item.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <returns>The grid data.</returns>
        public ItemShapeGridData GetGridDataForItem(ItemInfo itemInfo)
        {
            return GetGridDataForItemInternal(itemInfo, null, false);
        }

        /// <summary>
        /// Get the Grid data for the Item.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="receivingCollection">The receiving item collection.</param>
        /// <param name="getPotential">Get where the item is or potentially get where the item would be if it was added?</param>
        /// <returns>The item shape grid data.</returns>
        protected virtual ItemShapeGridData GetGridDataForItemInternal(ItemInfo itemInfo, ItemCollection receivingCollection, bool getPotential)
        {
            if (itemInfo.Item == null) {
                return null;
            }
            
            var index = -1;
            for (int i = 0; i < m_ItemShapeGridData.Count; i++) {
                if (getPotential) {
                    if (!m_ItemShapeGridData[i].CanAddItem(itemInfo, receivingCollection)) { continue; }
                } else {
                    if (!m_ItemShapeGridData[i].IsItemValidForGridData(itemInfo)) { continue; }
                }

                if (index != -1) {
                    Debug.LogWarning(
                        $"The inventory has multiple grids which could contain the same item '{itemInfo}', this can cause many types of issues.");
                }

                index = i;
            }

            if (index == -1) {
                var ignoreCollection = IsCollectionIgnored(itemInfo.ItemCollection) || IsCollectionIgnored(receivingCollection);

                if (!m_NoGridAddItem && !ignoreCollection) { Debug.LogWarning($"No Grid Data was found for the item '{itemInfo}' with a receiving collection {receivingCollection}."); }

                return null;
            }

            return m_ItemShapeGridData[index];
        }

        /// <summary>
        /// Is the Item Collection Ignored.
        /// </summary>
        /// <param name="itemCollection">The Item Collection to check if it was ignored.</param>
        /// <returns>Should the ItemCollection be ignored?</returns>
        public virtual bool IsCollectionIgnored(ItemCollection itemCollection)
        {
            if (itemCollection == null) { return false; }

            return itemCollection.Purpose == ItemCollectionPurpose.Hide ||
                   itemCollection.Purpose == ItemCollectionPurpose.Loadout;
        }
        
        /// <summary>
        /// Cleanup the Inventory Grid Indexer every frame.
        /// </summary>
        protected virtual void LateUpdate()
        {
            for (int i = 0; i < m_ItemShapeGridData.Count; i++) {
                m_ItemShapeGridData[i].Cleanup();
            }
        }
    }
}