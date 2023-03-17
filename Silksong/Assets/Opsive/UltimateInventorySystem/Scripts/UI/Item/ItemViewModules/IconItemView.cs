/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// A Item View UI component that lets you bind an icon to the item icon attribute.
    /// </summary>
    public class IconItemView : ItemViewModule
    {
        [Tooltip("The icon image.")]
        [SerializeField] protected Image m_Icon;
        [Tooltip("The missing item icon sprite.")]
        [SerializeField] protected Sprite m_MissingIcon;
        [Tooltip("Disable the image component if item is null.")]
        [SerializeField] protected bool m_DisableOnClear;

        /// <summary>
        /// Set the item info.
        /// </summary>
        /// <param name="info">The item info.</param>
        public override void SetValue(ItemInfo info)
        {
            if (info.Item == null || info.Item.IsInitialized == false) {
                Clear();
                return;
            }

            m_Icon.enabled = true;
            var iconToSet = m_MissingIcon;
            if (info.Item.TryGetAttributeValue<Sprite>("Icon", out var icon)) {
                iconToSet = icon == null ? m_MissingIcon : icon;
            }

            m_Icon.sprite = iconToSet;
        }

        /// <summary>
        /// Clear the component.
        /// </summary>
        public override void Clear()
        {
            m_Icon.sprite = m_MissingIcon;
            if (m_DisableOnClear) { m_Icon.enabled = false; }
        }
    }
}