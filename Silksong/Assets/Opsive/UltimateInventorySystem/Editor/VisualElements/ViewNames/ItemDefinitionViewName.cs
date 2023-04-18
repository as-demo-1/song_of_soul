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
    /// The item definition view name.
    /// </summary>
    public class ItemDefinitionViewName : ColoredBoxViewName<ItemDefinition>
    {
        public ItemDefinition ItemDefinition => m_Object;

        /// <summary>
        /// Constructor to setup the object.
        /// </summary>
        public ItemDefinitionViewName() : base()
        {
        }

        /// <summary>
        /// Refresh the view.
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            if (ItemDefinition == null) {
                m_ColoredBox.visible = false;
                m_Label.text = "None";
                return;
            }

            m_ColoredBox.visible = true;
            SetText(ItemDefinition.name);

            var boxTooltip = $"{ItemDefinition.name} <{ItemDefinition.GetType().Name}> (ID: {ItemDefinition.ID})";

            var iconAttribute = ItemDefinition.GetAttribute<Attribute<Sprite>>("Icon");
            var icon = m_Object.m_EditorIcon != null ? m_Object.m_EditorIcon : iconAttribute?.GetValue();

            if (icon == null) {
                SetColoredBox(m_ColoredBox, ItemDefinition.Category, "", boxTooltip);
            } else {
                SetColoredBox(m_ColoredBox, icon, ItemDefinition.Category.m_Color, boxTooltip);
            }
            m_ColoredBox.SetClickAction(() => Managers.InventoryMainWindow.NavigateTo(m_Object));
        }
    }
}