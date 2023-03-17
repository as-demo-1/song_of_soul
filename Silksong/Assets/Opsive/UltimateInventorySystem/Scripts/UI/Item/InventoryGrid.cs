/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.UI.Grid;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The hot item bar component allows you to use an item action for an item that was added to the hot bar.
    /// </summary>
    public class InventoryGrid : ItemViewSlotsContainerBase
    {
        [Tooltip("The inventory Grid.")]
        [SerializeField] protected ItemInfoGrid m_Grid;
        [Tooltip("If true the inventory grid will keep items on the slot they were last set, if false there won't be empty spaces between items.")]
        [SerializeField] internal bool m_UseGridIndex = true;

        [SerializeField] protected bool m_ResetIndexOnResetDraw;

        protected InventoryGridIndexer m_InventoryGridIndexer;

        public ItemInfoGrid Grid => m_Grid;
        public InventoryGridIndexer InventoryGridIndexer {
            get { return m_InventoryGridIndexer; }
            set { m_InventoryGridIndexer = value; }
        }

        public int GridID => Grid.GridID;
        public int TabID { get; set; }
        public IFilterSorter<ItemInfo> FilterSorter => m_Grid.FilterSorter;

        public bool UseGridIndex {
            get => m_UseGridIndex;
            set => m_UseGridIndex = value;
        }

        /// <summary>
        /// This method is called before the Inventory is set to the Item View Slots Container.
        /// </summary>
        protected override void OnInitializeBeforeSettingInventory()
        { }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="force">For Initialize.</param>
        public override void Initialize(bool force)
        {
            if (m_IsInitialized && !force) { return; }

            if (m_Grid == null) {
                m_Grid = GetComponent<ItemInfoGrid>();
                if (m_Grid == null) {
                    Debug.LogError("Inventory grid field should not be null.", gameObject);
                    return;
                }
            }

            m_Grid.Initialize(force);

            var itemViewSlotAmount = m_Grid.ViewDrawer.ViewSlots.Length;
            m_ItemViewSlots = new ItemViewSlot[itemViewSlotAmount];
            for (int i = 0; i < itemViewSlotAmount; i++) {
                var boxSlot = m_Grid.ViewDrawer.ViewSlots[i] as ItemViewSlot;
                if (boxSlot == null) {
                    Debug.LogWarning("The item view slot container must use ItemViewSlots and not IViewSlots");
                }
                m_ItemViewSlots[i] = boxSlot;
            }

            if (m_InventoryGridIndexer == null) { m_InventoryGridIndexer = new InventoryGridIndexer(); }

            base.Initialize(force);
        }

        #region Item View Slot Container Overrides

        /// <summary>
        /// Remove an item from the index.
        /// </summary>
        /// <param name="itemInfo">The item info to remove.</param>
        /// <param name="index">The index to remove the item from.</param>
        /// <returns>The item info removed.</returns>
        public override ItemInfo RemoveItem(ItemInfo itemInfo, int index)
        {
            return Inventory.RemoveItem(itemInfo);
        }

        /// <summary>
        /// Can the item be added at index?
        /// </summary>
        /// <param name="itemInfo">The item info to add.</param>
        /// <param name="index">The index to add to item to.</param>
        /// <returns>True if the item can be added.</returns>
        public override bool CanAddItem(ItemInfo itemInfo, int index)
        {
            var canAddBase = base.CanAddItem(itemInfo, index);
            if (canAddBase == false) { return false; }

            if (Inventory.CanAddItem(itemInfo, null) == null) { return false; }

            return true;
        }

        /// <summary>
        /// Add the item at the index.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="index">The index of the item.</param>
        /// <returns>The added item.</returns>
        public override ItemInfo AddItem(ItemInfo itemInfo, int index)
        {
            if (CanAddItem(itemInfo, index) == false) {
                return (0, null, null);
            }

            var addedItem = Inventory.AddItem(itemInfo);
            m_InventoryGridIndexer.SetStackIndex(addedItem.ItemStack, index);

            //The grid needs to be refreshed
            Draw();

            return itemInfo;
        }

        /// <summary>
        /// Can the item be moved from one index to another.
        /// </summary>
        /// <param name="sourceIndex">The source index.</param>
        /// <param name="destinationIndex">The destination index.</param>
        /// <returns>True if the item can be moved.</returns>
        public override bool CanMoveItem(int sourceIndex, int destinationIndex)
        {
            if (base.CanMoveItem(sourceIndex, destinationIndex) == false) { return false; }
            return m_InventoryGridIndexer.CanMoveItem(sourceIndex, destinationIndex);
        }

        /// <summary>
        /// Move the item from one index to another.
        /// </summary>
        /// <param name="sourceIndex">The source index.</param>
        /// <param name="destinationIndex">The destination index.</param>
        public override void MoveItem(int sourceIndex, int destinationIndex)
        {
            if (CanMoveItem(sourceIndex, destinationIndex) == false) { return; }

            var sourceItemIndex = m_Grid.StartIndex + sourceIndex;
            var destinationItemIndex = m_Grid.StartIndex + destinationIndex;
            m_InventoryGridIndexer.MoveItemStackIndex(sourceItemIndex, destinationItemIndex);

            //The grid needs to be refreshed
            Draw();
        }

        protected override void ResetDrawInternal()
        {
            if (m_ResetIndexOnResetDraw) {
                m_Grid.SetIndex(0);
            }
            
            base.ResetDrawInternal();
        }

        /// <summary>
        /// Draw the item view slots.
        /// </summary>
        protected override void DrawInternal()
        {
            if (m_Inventory == null) {
                m_Grid.SetElements((null, 0, 0));
                m_Grid.Draw();
                return;
            }

            var listSlice = new ListSlice<ItemInfo>(m_Inventory.AllItemInfos);
            var filterSorter = m_Grid.FilterSorter;

            if (filterSorter != null) {

                var pooledArray = GenericObjectPool.Get<ItemInfo[]>();
                listSlice = filterSorter.Filter(listSlice, ref pooledArray);
                if (m_UseGridIndex) {
                    listSlice = m_InventoryGridIndexer.GetOrderedItems(listSlice);
                }
                GenericObjectPool.Return(pooledArray);
            } else {
                if (m_UseGridIndex) {
                    listSlice = m_InventoryGridIndexer.GetOrderedItems(listSlice);
                }
            }

            m_Grid.SetElements(listSlice, true);

            m_Grid.Draw();
        }

        /// <summary>
        /// Cleanup the Inventory Grid Indexer every frame.
        /// </summary>
        protected override void LateUpdate()
        {
            base.LateUpdate();
            if (m_UseGridIndex) {
                m_InventoryGridIndexer.Cleanup();
            }
        }

        /// <summary>
        /// Set the display panel.
        /// </summary>
        /// <param name="display">The display panel.</param>
        public override void SetDisplayPanel(DisplayPanel display)
        {
            base.SetDisplayPanel(display);
            if (m_Grid == null) {
                Debug.LogError("The Inventory Grid MUST have a Item Info Grid", gameObject);
                return;
            }
            m_Grid.SetParentPanel(display);
        }

        /// <summary>
        /// Get the Box prefab for the item specified.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <returns>The view prefab game object.</returns>
        public override GameObject GetViewPrefabFor(ItemInfo itemInfo)
        {
            return m_Grid.ViewDrawer.GetViewPrefabFor(itemInfo);
        }
        #endregion


        /// <summary>
        /// A new Inventory was bound to the container.
        /// </summary>
        protected override void OnInventoryBound()
        {
            var inventoryGridIndexData = m_Inventory.GetComponent<InventoryGridIndexData>();
            if (inventoryGridIndexData != null) {
                m_InventoryGridIndexer.Copy(inventoryGridIndexData.GetGridIndexer(this));
                inventoryGridIndexData.SetGridIndexData(this);
            }
        }

        /// <summary>
        /// The inventory was unbound from the container.
        /// </summary>
        protected override void OnInventoryUnbound()
        {
            var inventoryGridIndexData = m_Inventory.GetComponent<InventoryGridIndexData>();
            if (inventoryGridIndexData != null) {
                inventoryGridIndexData.SetGridIndexData(this);
            }
        }

        /// <summary>
        /// Sort the items within the indexer.
        /// </summary>
        /// <param name="comparer">The comparer object.</param>
        public void SortItemIndexes(Comparer<ItemInfo> comparer)
        {
            m_InventoryGridIndexer.SortItemIndexes(comparer);
        }

        /// <summary>
        /// Bind a filter to the grid.
        /// </summary>
        /// <param name="sorterFilter">The filter to bind.</param>
        /// <returns>The previous filter.</returns>
        public IFilterSorter<ItemInfo> BindGridFilterSorter(IFilterSorter<ItemInfo> sorterFilter)
        {
            return Grid.BindGridFilterSorter(sorterFilter);
        }

    }
}