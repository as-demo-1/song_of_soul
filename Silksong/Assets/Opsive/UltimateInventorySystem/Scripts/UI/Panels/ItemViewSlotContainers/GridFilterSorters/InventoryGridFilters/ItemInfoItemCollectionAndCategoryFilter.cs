/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers.GridFilterSorters.InventoryGridFilters
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using UnityEngine;

    /// <summary>
    /// The item info item collection filter, filters items by item collection.
    /// </summary>
    public class ItemInfoItemCollectionAndCategoryFilter : ItemInfoCategoryFilter
    {
        [Tooltip("The item collections to take into account.")]
        [SerializeField] protected ItemCollectionID[] m_ItemCollections;

        /// <summary>
        /// The filter.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <returns>True if the item is valid.</returns>
        public override bool Filter(ItemInfo itemInfo)
        {
            var show = m_ItemCollections.Length == 0;

            for (int i = 0; i < m_ItemCollections.Length; i++) {
                if (!m_ItemCollections[i].Compare(itemInfo.ItemCollection)) { continue; }

                show = true;
                break;
            }

            if (show == false) { return true; }

            return base.Filter(itemInfo);
        }
    }
}