/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames
{
    using Opsive.UltimateInventorySystem.Crafting;

    /// <summary>
    /// The crafting category view with a name.
    /// </summary>
    public class CraftingCategoryViewName : ColoredBoxViewName<CraftingCategory>
    {
        public CraftingCategory CraftingCategory => m_Object;

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

            var icon = m_Object.m_EditorIcon;

            if (icon == null) {
                SetColoredBox(m_ColoredBox, m_Object, "", boxTooltip);
            } else {
                SetColoredBox(m_ColoredBox, icon, m_Object.m_Color, boxTooltip);
            }

            m_ColoredBox.SetClickAction(() => Managers.InventoryMainWindow.NavigateTo(m_Object));
        }
    }
}