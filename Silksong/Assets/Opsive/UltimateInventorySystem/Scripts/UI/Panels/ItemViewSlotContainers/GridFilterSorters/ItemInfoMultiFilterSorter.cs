/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers.GridFilterSorters
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.Grid;
    using System.Collections.Generic;
    using UnityEngine;

    public class ItemInfoMultiFilterSorter : ItemInfoFilterSorterBase
    {
        [Tooltip("A list of frig filters and sorters.")]
        [SerializeField] internal List<ItemInfoFilterSorterBase> m_GridFilters;

        public List<ItemInfoFilterSorterBase> GridFilters => m_GridFilters;

        /// <summary>
        /// Filter the list of item infos.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="outputPooledArray">Reference to the output.</param>
        /// <returns>The list slice of filter.</returns>
        public override ListSlice<ItemInfo> Filter(ListSlice<ItemInfo> input, ref ItemInfo[] outputPooledArray)
        {
            var list = input;
            for (int i = 0; i < m_GridFilters.Count; i++) {
                list = m_GridFilters[i].Filter(list, ref outputPooledArray);
            }

            return list;
        }

        /// <summary>
        /// Can the input be contained.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>True if can be contained.</returns>
        public override bool CanContain(ItemInfo input)
        {
            for (int i = 0; i < m_GridFilters.Count; i++) {
                if (m_GridFilters[i].CanContain(input)) { continue; }

                return false;
            }

            return true;
        }
    }
}