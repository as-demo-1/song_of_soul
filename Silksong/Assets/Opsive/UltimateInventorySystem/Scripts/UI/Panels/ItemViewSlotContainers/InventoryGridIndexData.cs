/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers
{
    using Opsive.UltimateInventorySystem.UI.Item;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Inventory grid index data is used to keep track of the position of items withing grids. 
    /// </summary>
    public class InventoryGridIndexData : MonoBehaviour
    {
        protected Dictionary<(int gridID, int tabID), InventoryGridIndexer> m_GridIDTabIndexedItems;
        protected Dictionary<(InventoryGrid grid, int tabID), InventoryGridIndexer> m_GridTabIndexedItems;

        /// <summary>
        /// Set the grid index data from an inventory grid.
        /// </summary>
        /// <param name="inventoryGrid">The inventory grid.</param>
        public void SetGridIndexData(InventoryGrid inventoryGrid)
        {
            if (inventoryGrid == null) { return; }

            InventoryGridIndexer value = null;

            if (inventoryGrid.GridID == -1) {
                if (m_GridTabIndexedItems == null) {
                    m_GridTabIndexedItems = new Dictionary<(InventoryGrid, int), InventoryGridIndexer>();
                }

                if (m_GridTabIndexedItems.TryGetValue((inventoryGrid, inventoryGrid.TabID), out value)) {
                    value.Copy(inventoryGrid.InventoryGridIndexer);
                } else {
                    var newValue = new InventoryGridIndexer();
                    newValue.Copy(inventoryGrid.InventoryGridIndexer);
                    m_GridTabIndexedItems[(inventoryGrid, inventoryGrid.TabID)] = newValue;
                }

                return;
            }

            if (m_GridIDTabIndexedItems == null) {
                m_GridIDTabIndexedItems = new Dictionary<(int, int), InventoryGridIndexer>();
            }

            if (m_GridIDTabIndexedItems.TryGetValue((inventoryGrid.GridID, inventoryGrid.TabID), out value)) {
                value.Copy(inventoryGrid.InventoryGridIndexer);
            } else {
                var newValue = new InventoryGridIndexer();
                newValue.Copy(inventoryGrid.InventoryGridIndexer);
                m_GridIDTabIndexedItems[(inventoryGrid.GridID, inventoryGrid.TabID)] = newValue;
            }
        }

        /// <summary>
        /// Get the grid indexer for the inventory grid.
        /// </summary>
        /// <param name="inventoryGrid">The inventory grid.</param>
        /// <returns>The grid indexer.</returns>
        public InventoryGridIndexer GetGridIndexer(InventoryGrid inventoryGrid)
        {
            if (inventoryGrid == null) { return null; }

            InventoryGridIndexer value = null;

            if (inventoryGrid.GridID == -1) {
                if (m_GridTabIndexedItems == null) { return null; }
                if (m_GridTabIndexedItems.TryGetValue((inventoryGrid, inventoryGrid.TabID), out value)) {
                    return value;
                }
                return null;
            }

            if (m_GridIDTabIndexedItems == null) { return null; }
            if (m_GridIDTabIndexedItems.TryGetValue((inventoryGrid.GridID, inventoryGrid.TabID), out value)) {
                return value;
            }

            return null;
        }
    }
}