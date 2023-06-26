/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Menus.Crafting
{
    using Opsive.UltimateInventorySystem.Crafting;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// The Crafting menu.
    /// </summary>
    public class CraftingMenu : CraftingMenuBase
    {
        [Tooltip("The quantity picker panel.")]
        [SerializeField] internal QuantityPickerPanel m_QuantityPickerPanel;
        [Tooltip("The Exit button.")]
        [SerializeField] protected Button m_ExitButton;

        /// <summary>
        /// Initialize the Crafting Menu.
        /// </summary>
        /// <param name="display">The display.</param>
        /// <param name="force">Force the initialize.</param>
        public override void Initialize(DisplayPanel display, bool force)
        {
            var wasInitialized = m_IsInitialized;
            if (wasInitialized && !force) { return; }
            base.Initialize(display, force);

            if (wasInitialized == false) {
                //Only do it once even if forced.
                m_QuantityPickerPanel.OnAmountChanged += CraftingAmountChanged;
                m_QuantityPickerPanel.ConfirmCancelPanel.OnConfirm += CraftSelectedQuantity;

                if (m_ExitButton != null) {
                    m_ExitButton.onClick.AddListener(() => m_DisplayPanel.Close(true));
                }
            }
        }

        /// <summary>
        /// update when the crafting amount changes.
        /// </summary>
        /// <param name="amount">The new amount.</param>
        public override void CraftingAmountChanged(int amount)
        {
            var canCraft = m_Crafter.Processor.CanCraft(m_SelectedRecipe, m_Inventory, amount);
            if (canCraft == false) {
                m_QuantityPickerPanel.QuantityPicker.MaxQuantity = amount;
                m_QuantityPickerPanel.ConfirmCancelPanel.EnableConfirm(false);
            } else {
                m_QuantityPickerPanel.QuantityPicker.MaxQuantity = amount + 1;
                m_QuantityPickerPanel.ConfirmCancelPanel.EnableConfirm(true);
            }
            
            base.CraftingAmountChanged(amount);
        }

        /// <summary>
        /// A recipe is selected.
        /// </summary>
        /// <param name="recipe">The recipe.</param>
        /// <param name="index">The index.</param>
        public override void CraftingRecipeSelected(CraftingRecipe recipe, int index)
        {
            m_RecipePanel.SetRecipe(recipe);

            if (m_QuantityPickerPanel.IsOpen == false) { return; }
            if (m_SelectedRecipe == recipe) { return; }

            m_SelectedRecipe = recipe;
            m_QuantityPickerPanel.SetPreviousSelectable(m_CraftingRecipeGrid.GetButton(index));
            m_QuantityPickerPanel.QuantityPicker.MinQuantity = 1;
            m_QuantityPickerPanel.QuantityPicker.MaxQuantity = 2;

            m_QuantityPickerPanel.ConfirmCancelPanel.SetConfirmText("Craft");
            m_QuantityPickerPanel.QuantityPicker.SetQuantity(1);
            CraftingAmountChanged(1);
        }

        /// <summary>
        /// Recipe is clicked.
        /// </summary>
        /// <param name="recipe">The recipe.</param>
        /// <param name="index">The index.</param>
        public override void CraftingRecipeClicked(CraftingRecipe recipe, int index)
        {
            m_SelectedRecipe = recipe;

            m_QuantityPickerPanel.Open(m_DisplayPanel, m_CraftingRecipeGrid.GetButton(index));

            m_QuantityPickerPanel.QuantityPicker.MinQuantity = 1;
            m_QuantityPickerPanel.QuantityPicker.MaxQuantity = 2;

            m_QuantityPickerPanel.ConfirmCancelPanel.SetConfirmText("Craft");
            m_QuantityPickerPanel.QuantityPicker.SetQuantity(1);
            m_QuantityPickerPanel.QuantityPicker.SelectMainButton();
            CraftingAmountChanged(1);
        }

        /// <summary>
        /// Close the QuantityPickerPanel when the menu is closed. 
        /// </summary>
        public override void OnClose()
        {
            base.OnClose();
            if (m_QuantityPickerPanel.IsOpen) {
                m_QuantityPickerPanel.Close(false);
            }
        }
    }
}
