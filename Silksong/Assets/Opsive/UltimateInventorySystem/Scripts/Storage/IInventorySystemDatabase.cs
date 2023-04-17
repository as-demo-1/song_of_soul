/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Storage
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Crafting;
    using Opsive.UltimateInventorySystem.Exchange;

    /// <summary>
    /// The inventory system database interface.
    /// </summary>
    public interface IInventorySystemDatabase
    {
        /// <summary>
        /// Get the item categories.
        /// </summary>
        ItemCategory[] ItemCategories { get; }

        /// <summary>
        /// Get the item definitions.
        /// </summary>
        ItemDefinition[] ItemDefinitions { get; }

        /// <summary>
        /// Get the currencies.
        /// </summary>
        Currency[] Currencies { get; }

        /// <summary>
        /// Get the crafting recipes.
        /// </summary>
        CraftingRecipe[] CraftingRecipes { get; }

        /// <summary>
        /// Get the crafting categories.
        /// </summary>
        CraftingCategory[] CraftingCategories { get; }
    }
}