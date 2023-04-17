/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Menus.Crafting
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.Crafting;
    using Opsive.UltimateInventorySystem.Crafting.IngredientsTypes;
    using Opsive.UltimateInventorySystem.Exchange;
    using Opsive.UltimateInventorySystem.UI.Currency;
    using Opsive.UltimateInventorySystem.UI.Item;
    using Opsive.UltimateInventorySystem.UI.Item.ItemViewModules;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// A struct to combine an item view with a description
    /// </summary>
    [Serializable]
    public struct ItemViewWithDescription
    {
        public ItemView ItemView;
        public ItemDescriptionBase ItemDescription;
    }

    /// <summary>
    /// The recipe panel.
    /// </summary>
    public class RecipePanel : DisplayPanelBinding
    {
        [Tooltip("The currency UI.")]
        [SerializeField] protected MultiCurrencyView m_TotalCraftCost;
        [Tooltip("The result Item View.")]
        [SerializeField] protected ItemViewWithDescription m_ResultItem;
        [Tooltip("The ingredients Item Viewes.")]
        [SerializeField] internal List<ItemViewWithDescription> m_IngredientItems;

        [Tooltip("The ingredient view parent.")]
        [SerializeField] internal RectTransform m_IngredientViewParent;
        [Tooltip("The ingredient description parent.")]
        [SerializeField] internal RectTransform m_IngredientDescriptionParent;

        protected CraftingRecipe m_Recipe;
        protected int m_Quantity = 1;
        protected CurrencyCollection m_TemporaryCurrencyCollection;
        protected Inventory m_Inventory;
        protected Color m_CurrencyTextBaseColor;

        protected bool m_InventoryRegistered = false;

        public CraftingRecipe Recipe => m_Recipe;
        public int Quantity => m_Quantity;
        public Inventory Inventory => m_Inventory;

        /// <summary>
        /// Initialize the Recipe panel.
        /// </summary>
        /// <param name="display">The display panel.</param>
        /// <param name="force">Force the initialize.</param>
        public override void Initialize(DisplayPanel display, bool force)
        {
            var wasInitialized = m_IsInitialized;
            if (wasInitialized && !force) { return; }
            base.Initialize(display, force);

            m_TemporaryCurrencyCollection = new CurrencyCollection();
            m_CurrencyTextBaseColor = m_TotalCraftCost.GetTextColor();

            InitializeDescriptions();
        }

        private void InitializeDescriptions()
        {
            if (m_ResultItem.ItemDescription != null) {
                m_ResultItem.ItemDescription.Initialize();
            }

            for (int i = 0; i < m_IngredientItems.Count; i++) {
                if (m_IngredientItems[i].ItemDescription != null) {
                    m_IngredientItems[i].ItemDescription.Initialize();
                }
            }
        }

        /// <summary>
        /// Set the inventory.
        /// </summary>
        /// <param name="inventory">The inventory.</param>
        public void SetInventory(Inventory inventory)
        {
            if (!m_InventoryRegistered || m_Inventory != inventory) {
                if (m_Inventory != null && m_InventoryRegistered) {
                    EventHandler.UnregisterEvent(m_Inventory, EventNames.c_Inventory_OnUpdate, Refresh);
                }
                m_Inventory = inventory;
                if (m_Inventory != null) {
                    EventHandler.RegisterEvent(m_Inventory, EventNames.c_Inventory_OnUpdate, Refresh);
                }

                m_InventoryRegistered = true;
            }

            //Initialized might not have been called yet so we must initialize the descriptions just in case.
            InitializeDescriptions();

            AssignInventoryToItemView(m_ResultItem.ItemDescription);

            for (int i = 0; i < m_IngredientItems.Count; i++) {
                var itemView = m_IngredientItems[i].ItemView;
                AssignInventoryToItemView(itemView);
            }
        }

        /// <summary>
        /// Assign an inventory to the item view.
        /// </summary>
        /// <param name="itemView">The item view to assign the inventory to.</param>
        private void AssignInventoryToItemView(ItemView itemView)
        {
            if(itemView == null){ return; }
            for (int j = 0; j < itemView.Modules.Count; j++) {
                var component = itemView.Modules[j];
                if (component is IInventoryDependent inventoryDependentComponent) {
                    inventoryDependentComponent.Inventory = m_Inventory;
                }
            }
        }

        /// <summary>
        /// Set the recipe to craft.
        /// </summary>
        /// <param name="recipe">The recipe.</param>
        public void SetRecipe(CraftingRecipe recipe)
        {
            m_Recipe = recipe;
            Refresh();
        }

        /// <summary>
        /// Set the Quantity to craft.
        /// </summary>
        /// <param name="amount">The amount.</param>
        public void SetQuantity(int amount)
        {
            m_Quantity = amount;
        }

        /// <summary>
        /// Refresh the view.
        /// </summary>
        public void Refresh()
        {
            //Only refresh if enabled otherwise the refresh can happen everytime the inventory updates.
            if(isActiveAndEnabled == false){ return; }
            DrawRecipeOutput();
            DrawRecipeIngredients();
        }

        /// <summary>
        /// Draw the recipe output.
        /// </summary>
        protected virtual void DrawRecipeOutput()
        {
            if (m_Recipe == null) {
                ClearMainOutput();
                return;
            }
            var itemAmount = m_Recipe.MainItemAmountOutput;
            if (itemAmount == null) {
                ClearMainOutput();
                return;
            }

            var itemAmountWithQuantity = new ItemAmount(itemAmount.Value.Item, itemAmount.Value.Amount * m_Quantity);
            SetMainOutput(itemAmountWithQuantity);
        }

        /// <summary>
        /// Clear the main output.
        /// </summary>
        protected virtual void ClearMainOutput()
        {
            if (m_ResultItem.ItemView != null) {
                m_ResultItem.ItemView.Clear();
            }

            if (m_ResultItem.ItemDescription != null) {
                m_ResultItem.ItemDescription.Clear();
            }
            
        }

        /// <summary>
        /// Set the main output.
        /// </summary>
        /// <param name="itemAmount">The main output.</param>
        protected virtual void SetMainOutput(ItemAmount itemAmount)
        {
            if (m_ResultItem.ItemView != null) {
                m_ResultItem.ItemView.SetValue((itemAmount, null));
            }

            if (m_ResultItem.ItemDescription != null) {
                m_ResultItem.ItemDescription.SetValue((ItemInfo)itemAmount);
            }
        }

        /// <summary>
        /// Draw the recipes ingredients.
        /// </summary>
        protected virtual void DrawRecipeIngredients()
        {
            DrawItemIngredients();
            DrawCurrencyIngredients();
        }

        /// <summary>
        /// Draw the ingredients.
        /// </summary>
        protected void DrawItemIngredients()
        {
            if (m_Recipe == null) {
                for (int i = 0; i < m_IngredientItems.Count; i++) { ClearItemIngredient(i); }
                return;
            }

            var ingredientCount = 0;
            for (int i = 0; i < m_Recipe.Ingredients.ItemAmounts.Count; i++) {
                var itemIngredient = m_Recipe.Ingredients.ItemAmounts[i];
                if (itemIngredient.Item == null) { continue; }

                var itemAmount = new ItemAmount(itemIngredient.Item, itemIngredient.Amount * m_Quantity);
                SetItemIngredient(itemAmount, ingredientCount);
                ingredientCount++;
            }

            for (int i = 0; i < m_Recipe.Ingredients.ItemDefinitionAmounts.Count; i++) {
                var itemDefinitionIngredient = m_Recipe.Ingredients.ItemDefinitionAmounts[i];
                if (itemDefinitionIngredient.ItemDefinition == null) { continue; }

                var itemAmount = new ItemAmount(itemDefinitionIngredient.ItemDefinition.DefaultItem,
                    itemDefinitionIngredient.Amount * m_Quantity);
                SetItemIngredient(itemAmount, ingredientCount);
                ingredientCount++;
            }

            for (int i = ingredientCount; i < m_IngredientItems.Count; i++) { ClearItemIngredient(i); }
        }

        /// <summary>
        /// Clear the item ingredients.
        /// </summary>
        /// <param name="i">The index.</param>
        private void ClearItemIngredient(int i)
        {
            if (m_IngredientItems[i].ItemView != null) {
                m_IngredientItems[i].ItemView.Clear();
            }

            if (m_IngredientItems[i].ItemDescription != null) {
                m_IngredientItems[i].ItemDescription.Clear();
                m_IngredientItems[i].ItemDescription.Hide(true);
            }
        }

        /// <summary>
        /// Set the item ingredients.
        /// </summary>
        /// <param name="itemAmount">The item amount.</param>
        /// <param name="i">The index.</param>
        private void SetItemIngredient(ItemAmount itemAmount, int i)
        {
            if (m_IngredientItems[i].ItemView != null) {
                m_IngredientItems[i].ItemView.SetValue((itemAmount, null));
            }

            if (m_IngredientItems[i].ItemDescription != null) {
                m_IngredientItems[i].ItemDescription.Hide(false);
                m_IngredientItems[i].ItemDescription.SetValue((ItemInfo)itemAmount);
            }
        }

        /// <summary>
        /// Draw the currency ingredients.
        /// </summary>
        protected void DrawCurrencyIngredients()
        {
            m_TotalCraftCost.SetTextColor(m_CurrencyTextBaseColor);

            if (m_Recipe != null && m_Recipe.Ingredients is CraftingIngredientsWithCurrency ingredientsWithCurrency) {
                m_TemporaryCurrencyCollection.SetCurrency(ingredientsWithCurrency.CurrencyAmounts, m_Quantity);
                if (!m_Inventory.GetCurrencyComponent<CurrencyCollection>()?.
                        CurrencyAmount.HasCurrency(m_TemporaryCurrencyCollection) ?? false) {
                    m_TotalCraftCost.SetTextColor(Color.red);
                }
            } else {
                m_TemporaryCurrencyCollection.RemoveAll();
            }

            m_TotalCraftCost.DrawCurrency(m_TemporaryCurrencyCollection);
        }
    }
}
