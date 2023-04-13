/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers.GridFilterSorters.InventoryGridSorters
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.Grid;
    using System.Collections.Generic;

    /// <summary>
    /// Sort items by amount.
    /// </summary>
    public class ItemInfoAmountSorter : ItemInfoSorterBase
    {
        /// <summary>
        /// Compare the items to sort them.
        /// </summary>
        protected Comparer<ItemInfo> m_ItemNameComparer = Comparer<ItemInfo>.Create((i1, i2) =>
        {
            return i2.Amount.CompareTo(i1.Amount);
        });

        public override Comparer<ItemInfo> Comparer => m_ItemNameComparer;
    }
}