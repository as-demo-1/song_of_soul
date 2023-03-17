/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.Item.ItemViewSlotRestrictions;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// A Item View Module component that show the Icon of the definition that restricts the Item View Slot.
    /// </summary>
    public class DefinitionRestrictionIconItemView : ItemViewModule
    {
        [Tooltip("The icon image.")]
        [SerializeField] protected Image m_Icon;
        [Tooltip("The Category Icon Attribute.")]
        [SerializeField] protected string m_IconAttributeName = "Icon";
        [Tooltip("The Category Icon Attribute.")]
        [SerializeField] protected Sprite m_DefaultSprite;

        /// <summary>
        /// Set the item info.
        /// </summary>
        /// <param name="info">The item info.</param>
        public override void SetValue(ItemInfo info)
        {
            Clear();
        }

        /// <summary>
        /// Clear the Item View Module.
        /// </summary>
        public override void Clear()
        {
            var definitionRestriction = View.ViewSlot.gameObject.GetCachedComponent<ItemViewSlotDefinitionRestriction>();
            if (definitionRestriction == null) {
                m_Icon.sprite = m_DefaultSprite;
                return;
            }
            
            if (definitionRestriction.ItemDefinition.TryGetAttributeValue<Sprite>(m_IconAttributeName, out var icon)) {
                m_Icon.sprite = icon;
            } else {
                m_Icon.sprite = m_DefaultSprite;
            }
        }
    }
}