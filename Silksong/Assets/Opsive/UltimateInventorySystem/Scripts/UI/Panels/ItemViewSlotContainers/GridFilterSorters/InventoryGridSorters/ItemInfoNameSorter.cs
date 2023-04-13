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
    using UnityEngine;

    /// <summary>
    /// Sort the items by name.
    /// </summary>
    public class ItemInfoNameSorter : ItemInfoSorterBase
    {
        [Tooltip("Sort by name ascending or descending?")]
        [SerializeField] protected bool m_Ascending = false;

        protected Comparer<ItemInfo> m_ItemNameComparer;

        public override Comparer<ItemInfo> Comparer => m_ItemNameComparer;

        /// <summary>
        /// Initialize.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            m_ItemNameComparer = Comparer<ItemInfo>.Create((i1, i2) =>
            {
                if (i1.Item == i2.Item) { return 0; }

                if (i1.Item == null) { return 1; }
                if (i2.Item == null) { return -1; }

                if (m_Ascending) {
                    return i2.Item.name.CompareTo(i1.Item.name);
                } else {
                    return i1.Item.name.CompareTo(i2.Item.name);
                }
            });
        }
    }
}