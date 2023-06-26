/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewSlotRestrictions
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using UnityEngine;

    /// <summary>
    /// Use the Item View Slot Category Restriction component to limit the items which can be set in the slot by category.
    /// </summary>
    public class ItemViewSlotCategoryRestriction : ItemViewSlotRestriction
    {
        [Tooltip("The item category the the item should have.")]
        [SerializeField] protected DynamicItemCategory m_ItemCategory;
        [Tooltip("Compare the category inherently or by exact match.")]
        [SerializeField] protected bool m_Inherently = true;

        public ItemCategory ItemCategory {
            get => m_ItemCategory;
            set => m_ItemCategory = value;
        }

        public bool Inherently {
            get => m_Inherently;
            set => m_Inherently = value;
        }

        /// <summary>
        /// Can the slot container this item.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <returns>True if the slot can contain the item.</returns>
        public override bool CanContain(ItemInfo itemInfo)
        {
            if (itemInfo.Item == null) { return true; }
            if (ItemCategory == null) { return true; }

            if (m_Inherently && ItemCategory.InherentlyContains(itemInfo.Item.Category)) { return true; }

            return ItemCategory == itemInfo.Item.Category;
        }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            var categoryName = ItemCategory != null ? ItemCategory.name : "NULL";
            return GetType().Name + " With Category: " + categoryName;
        }
    }
}