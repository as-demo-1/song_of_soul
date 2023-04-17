/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Crafting.Processors
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.Crafting.IngredientsTypes;
    using Opsive.UltimateInventorySystem.Exchange;
    using System;

    /// <summary>
    /// Simple crafting solution using the recipe itemDefinitions.
    /// </summary>
    [Serializable]
    public class SimpleCraftingProcessorWithCurrency : SimpleCraftingProcessor
    {
        protected readonly CurrencyCollection m_CurrencyCollection = new CurrencyCollection();

        /// <summary>
        /// Constructor.
        /// </summary>
        public SimpleCraftingProcessorWithCurrency()
        {
            m_ExternallyRemoveIngredients = false;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="externallyRemoveIngredients">Choose whether to remove the items externally or from the main collection.</param>
        public SimpleCraftingProcessorWithCurrency(bool externallyRemoveIngredients)
        {
            m_ExternallyRemoveIngredients = externallyRemoveIngredients;
        }

        /// <summary>
        /// Check if the parameters are valid to craft an item.
        /// </summary>
        /// <param name="recipe">The recipe.</param>
        /// <param name="inventory">The inventory containing the items.</param>
        /// <param name="selectedIngredients">The item infos selected.</param>
        /// <param name="quantity">The quantity to craft.</param>
        /// <returns>True if you can craft.</returns>
        protected override bool CanCraftInternal(CraftingRecipe recipe, IInventory inventory,
            ListSlice<ItemInfo> selectedIngredients, int quantity)
        {
            var result = base.CanCraftInternal(recipe, inventory, selectedIngredients, quantity);
            if (result == false) { return false; }

            //Currency Ingredients
            RetrieveCurrencyFromIngredients(recipe.Ingredients, quantity);

            var currencyOwner = inventory.GetCurrencyComponent<CurrencyCollection>();

            if (currencyOwner != null) {
                if (currencyOwner.HasCurrency(m_CurrencyCollection) == false) { return false; }
            } else {
                if (m_CurrencyCollection.GetCurrencyAmounts().Count != 0) { return false; }
            }

            return true;
        }

        /// <summary>
        /// Craft the items.
        /// </summary>
        /// <param name="recipe">The recipe.</param>
        /// <param name="inventory">The inventory containing the items.</param>
        /// <param name="selectedIngredients">The item infos selected.</param>
        /// <param name="quantity">The quantity to craft.</param>
        /// <returns>True if you can craft.</returns>
        protected override CraftingResult CraftInternal(CraftingRecipe recipe, IInventory inventory,
            ListSlice<ItemInfo> selectedIngredients, int quantity)
        {
            if (CanCraftInternal(recipe, inventory, selectedIngredients, quantity) == false) {
                return new CraftingResult(null, false);
            }

            if (RemoveIngredients(inventory, selectedIngredients) == false) {
                return new CraftingResult(null, false);
            }

            //Remove Currency
            if (recipe.Ingredients is CraftingIngredientsWithCurrency ingredientsWithCurrency) {
                var currencyOwner = inventory.GetCurrencyComponent<CurrencyCollection>();
                if (currencyOwner != null) {
                    m_CurrencyCollection.SetCurrency(ingredientsWithCurrency.CurrencyAmounts, quantity);
                    currencyOwner.RemoveCurrency(m_CurrencyCollection);
                }
            }

            var output = CreateCraftingOutput(recipe, inventory, quantity);

            return new CraftingResult(output, true);
        }

        /// <summary>
        /// Retrieve currency from the ingredients.
        /// </summary>
        /// <param name="ingredients">The ingredients.</param>
        /// <param name="quantity">The quantity.</param>
        protected void RetrieveCurrencyFromIngredients(CraftingIngredients ingredients, int quantity)
        {
            if (ingredients is CraftingIngredientsWithCurrency ingredientsWithCurrency) {
                m_CurrencyCollection.SetCurrency(ingredientsWithCurrency.CurrencyAmounts, quantity);
                return;
            }
            m_CurrencyCollection.RemoveAll();

        }
    }
}
