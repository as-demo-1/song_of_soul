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
    /// A Item View Module component that show the Icon of the category that restricts the Item View Slot.
    /// </summary>
    public class CategoryRestrictionIconItemView : ItemViewModule
    {
        [Tooltip("The icon image.")]
        [SerializeField] protected Image m_Icon;
        [Tooltip("The Category Icon Attribute.")]
        [SerializeField] protected string m_CategoryIconAttributeName = "CategoryIcon";
        [Tooltip("The Category Icon Attribute.")]
        [SerializeField] protected Sprite m_DefaultSprite;
        [Tooltip("Disable the Icon when an item is assigned.")]
        [SerializeField] protected bool m_DisableWhenItemAssigned;

        /// <summary>
        /// Set the item info.
        /// </summary>
        /// <param name="info">The item info.</param>
        public override void SetValue(ItemInfo info)
        {
            if (info.Item == null) {
                Clear();
                return;
            }
            
            if (m_DisableWhenItemAssigned) {
                HideIcon();
            } else {
                ShowIcon();
            }
            
        }

        private void ShowIcon()
        {
            var categoryRestriction = View.ViewSlot.gameObject.GetComponent<ItemViewSlotCategoryRestriction>();
            if (categoryRestriction == null) {
                HideIcon();
                return;
            }

            m_Icon.enabled = true;
            if (categoryRestriction.ItemCategory.TryGetCategoryAttributeValue<Sprite>(m_CategoryIconAttributeName,
                out var icon)) {
                m_Icon.sprite = icon;
            } else {
                m_Icon.sprite = m_DefaultSprite;
            }
        }

        /// <summary>
        /// Clear the Item View Module.
        /// </summary>
        public override void Clear()
        {
            ShowIcon();
        }

        private void HideIcon()
        {
            m_Icon.enabled = false;
        }
    }
}