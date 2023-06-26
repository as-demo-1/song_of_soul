/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using UnityEngine;

    /// <summary>
    /// The item category view name.
    /// </summary>
    public class ItemCategoryViewName : ColoredBoxViewName<ItemCategory>
    {
        public ItemCategory ItemCategory => m_Object;

        /// <summary>
        /// Refresh the view.
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            if (m_Object == null) {
                m_ColoredBox.visible = false;
                m_Label.text = "None";
                return;
            }

            m_ColoredBox.visible = true;
            SetText(m_Object.name);

            var boxTooltip = $"{m_Object.name} <{m_Object.GetType().Name}> (ID: {m_Object.ID})";

            var iconAttribute = ItemCategory.GetCategoryAttribute<Attribute<Sprite>>("Icon");
            if (iconAttribute == null) {
                iconAttribute = ItemCategory.GetCategoryAttribute<Attribute<Sprite>>("CategoryIcon");
            }

            var icon = ItemCategory.m_EditorIcon != null ? ItemCategory.m_EditorIcon : iconAttribute?.OverrideValue;

            if (icon == null) {
                SetColoredBox(m_ColoredBox, ItemCategory, "", boxTooltip);
            } else {
                SetColoredBox(m_ColoredBox, icon, ItemCategory.m_Color, boxTooltip);
            }
            m_ColoredBox.SetClickAction(() =>
            {
                Managers.InventoryMainWindow.NavigateTo(m_Object, true);
            });
        }
    }
}