/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Item Category Tab Control is used to organize an InventoryUI panel by ItemCategories.
    /// </summary>
    public class InventoryGridTabControlBinding : ItemViewSlotsContainerInventoryGridBinding
    {
        [Tooltip("The tab control.")]
        [SerializeField] protected internal TabControl m_TabControl;

        [SerializeField] protected bool m_ResetTabIndexOnResetDraw;

        /// <summary>
        /// Handle the Item View Slot Container being bound.
        /// </summary>
        protected override void OnBindItemViewSlotContainer()
        {
            if (m_TabControl == null) {
                Debug.LogError("The tab control is missing from the inventory grid tab control binding", gameObject);
                return;
            }

            m_ItemViewSlotsContainer.OnResetDraw += ResetDraw;
            m_TabControl.OnTabChange += HandleTabChange;
            m_TabControl.Initialize(false);
            m_TabControl.SetTabOn(m_TabControl.TabIndex);

        }

        /// <summary>
        /// Reset the state of the tabs.
        /// </summary>
        protected virtual void ResetDraw()
        {
            if (m_ResetTabIndexOnResetDraw) {
                m_TabControl.SetTabOn(0);
            }
        }

        /// <summary>
        /// Handle the Item view slot container being unbound.
        /// </summary>
        protected override void OnUnbindItemViewSlotContainer()
        {
            m_ItemViewSlotsContainer.OnResetDraw -= ResetDraw;
            m_TabControl.OnTabChange -= HandleTabChange;
        }

        /// <summary>
        /// Handle the tab changing.
        /// </summary>
        /// <param name="previousIndex">The previous tab index.</param>
        /// <param name="newIndex">The new tab index.</param>
        private void HandleTabChange(int previousIndex, int newIndex)
        {
            var inventoryTabData = m_TabControl.CurrentTab.GetComponent<InventoryTabData>();
            inventoryTabData.Initialize(false);

            if (inventoryTabData == null) {
                Debug.LogWarning("The selected tab is either null or does not have an InventoryTabData");
                return;
            }

            // Save the indexed items data.
            var previousTab = m_TabControl.TabToggles[previousIndex];
            var previousInventory = m_InventoryGrid.Inventory;

            var previousInventoryGridIndexData = previousInventory?.GetComponent<InventoryGridIndexData>();
            if (previousInventoryGridIndexData != null) {
                //m_InventoryGrid.InventoryGridIndexer.SetIndexItems(previousInventoryGridIndexData.GetGridIndexData(m_InventoryGrid));
                previousInventoryGridIndexData.SetGridIndexData(m_InventoryGrid);
            } else {
                var previousInventoryGridTab = previousTab.GetComponent<InventoryTabData>();

                if (previousInventoryGridTab == null) {
                    Debug.LogWarning("The selected tab is either null or not an InventoryTabData");
                } else {
                    previousInventoryGridTab.Initialize(false);
                    previousInventoryGridTab.Indexer.Copy(m_InventoryGrid.InventoryGridIndexer);
                }
            }

            // Change the tab bindings
            m_InventoryGrid.TabID = m_TabControl.TabIndex;

            if (inventoryTabData.Inventory != null) {
                m_InventoryGrid.SetInventory(inventoryTabData.Inventory);
            }

            if (inventoryTabData.ItemInfoFilter != null) {
                m_InventoryGrid.BindGridFilterSorter(inventoryTabData.ItemInfoFilter);
            }

            // Set the indexed items data.
            var inventoryGridIndexData = m_InventoryGrid.Inventory?.GetComponent<InventoryGridIndexData>();
            if (inventoryGridIndexData != null) {
                m_InventoryGrid.InventoryGridIndexer.Copy(inventoryGridIndexData.GetGridIndexer(m_InventoryGrid));
            } else if (inventoryTabData.Indexer != null) {
                m_InventoryGrid.InventoryGridIndexer.Copy(inventoryTabData.Indexer);
            } else {
                m_InventoryGrid.InventoryGridIndexer.Clear();
            }

            m_InventoryGrid.Draw();
        }

        /// <summary>
        /// Sort the item indexes.
        /// </summary>
        /// <param name="comparer">The comparer used to sort the item infos.</param>
        public void SortItemIndexes(Comparer<ItemInfo> comparer)
        {
            for (int i = 0; i < m_TabControl.TabCount; i++) {
                var tab = m_TabControl.TabToggles[i];
                var inventoryTab = tab.GetComponent<InventoryTabData>();
                if (inventoryTab != null) {
                    inventoryTab.Indexer.SortItemIndexes(comparer);
                }
            }
        }
    }
}