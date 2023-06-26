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
    /// The item view name.
    /// </summary>
    public class ItemViewName : ColoredBoxViewName<Item>
    {
        public Item Item => m_Object;

        /// <summary>
        /// Constructor to setup the object.
        /// </summary>
        public ItemViewName() : base()
        {
        }

        /// <summary>
        /// Refresh the view.
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            if (Item == null || Item.ItemDefinition == null) {
                m_ColoredBox.visible = false;
                m_Label.text = "None";
                return;
            }

            if (Item.IsInitialized == false) {
                Item.Initialize(false);
            }

            var isequivalentToDefault = true;
            for (int i = 0; i < Item.ItemAttributeCollection.Count; i++) {
                if (Item.ItemAttributeCollection[i].VariantType != VariantType.Inherit) {
                    isequivalentToDefault = false;
                    break;
                }
            }

            var suffix = isequivalentToDefault ? " (default)" : " (custom)";
            var itemName = Item.name + suffix;

            m_ColoredBox.visible = true;
            SetText(itemName);

            var boxTooltip = $"{itemName} <{Item.GetType().Name}> (ID: {Item.ID})";

            var iconAttribute = Item.GetAttribute<Attribute<Sprite>>("Icon");
            var icon = Item.ItemDefinition.m_EditorIcon != null ? Item.ItemDefinition.m_EditorIcon : iconAttribute?.GetValue();

            if (icon == null) {
                SetColoredBox(m_ColoredBox, Item.Category, "", boxTooltip);
            } else {
                SetColoredBox(m_ColoredBox, icon, Item.Category.m_Color, boxTooltip);
            }
            m_ColoredBox.SetClickAction(() => Managers.InventoryMainWindow.NavigateTo(Item?.ItemDefinition, true));
        }
    }
}