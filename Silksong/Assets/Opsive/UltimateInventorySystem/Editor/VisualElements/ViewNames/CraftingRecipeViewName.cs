/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames
{
    using Opsive.UltimateInventorySystem.Crafting;

    /// <summary>
    /// The crafting recipe view name.
    /// </summary>
    public class CraftingRecipeViewName : ColoredBoxViewName<CraftingRecipe>
    {
        public CraftingRecipe CraftingRecipe => m_Object;

        /// <summary>
        /// Constructor to setup the object.
        /// </summary>
        public CraftingRecipeViewName() : base()
        {
        }

        /// <summary>
        /// Refresh the view.
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            if (CraftingRecipe == null) {
                m_ColoredBox.visible = false;
                m_Label.text = "None";
                return;
            }

            m_ColoredBox.visible = true;
            SetText(CraftingRecipe.name);

            var boxTooltip = $"{m_Object.name} <{m_Object.GetType().Name}> (ID: {m_Object.ID})";

            var icon = m_Object.m_EditorIcon;

            if (icon == null) {
                SetColoredBox(m_ColoredBox, CraftingRecipe.Category, "", boxTooltip);
            } else {
                SetColoredBox(m_ColoredBox, icon, CraftingRecipe.Category.m_Color, boxTooltip);
            }

            m_ColoredBox.SetClickAction(() => Managers.InventoryMainWindow.NavigateTo(m_Object));
        }
    }
}