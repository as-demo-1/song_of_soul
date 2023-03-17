/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers.GridFilterSorters.InventoryGridFilters
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.UI.Grid;
    using UnityEngine;

    /// <summary>
    /// The item info item collection filter, filters items by item collection.
    /// </summary>
    public class ItemInfoItemCollectionFilter : ItemInfoFilterBase
    {
        [Tooltip("The item collections to show, Show all if null.")]
        [SerializeField]
        protected ItemCollectionID[] m_ShowItemCollections;
        [Tooltip("The item collections that should not be drawn.")]
        [SerializeField]
        protected ItemCollectionID[] m_HideItemCollections =
            { ItemCollectionPurpose.Loadout, ItemCollectionPurpose.Hide };

        public ItemCollectionID[] ShowItemCollections => m_ShowItemCollections;
        public ItemCollectionID[] HideItemCollections => m_HideItemCollections;

        /// <summary>
        /// The filter.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <returns>True if the item is valid.</returns>
        public override bool Filter(ItemInfo itemInfo)
        {
            var show = m_ShowItemCollections.Length == 0;

            for (int i = 0; i < m_ShowItemCollections.Length; i++) {
                if (!m_ShowItemCollections[i].Compare(itemInfo.ItemCollection)) { continue; }

                show = true;
                break;
            }

            if (show == false) { return false; }

            for (int i = 0; i < m_HideItemCollections.Length; i++) {
                if (m_HideItemCollections[i].Compare(itemInfo.ItemCollection)) { return false; }
            }

            return true;
        }
    }
}