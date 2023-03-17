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
    /// Use the Item View Slot Definition Restriction component to limit the items which can be set in the slot by definition.
    /// </summary>
    public class ItemViewSlotDefinitionRestriction : ItemViewSlotRestriction
    {
        [Tooltip("The item definition the the item should have.")]
        [SerializeField] protected DynamicItemDefinition m_ItemDefinition;
        [Tooltip("Compare the definition inherently or by exact match.")]
        [SerializeField] protected bool m_Inherently = true;

        public ItemDefinition ItemDefinition {
            get => m_ItemDefinition;
            set => m_ItemDefinition = value;
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
            if (ItemDefinition == null) { return true; }

            if (m_Inherently && ItemDefinition.InherentlyContains(itemInfo.Item.ItemDefinition)) { return true; }

            return ItemDefinition == itemInfo.Item.ItemDefinition;
        }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            var definitionName = ItemDefinition != null ? ItemDefinition.name : "NULL";
            return GetType().Name + " With Definition: " + definitionName;
        }
    }
}