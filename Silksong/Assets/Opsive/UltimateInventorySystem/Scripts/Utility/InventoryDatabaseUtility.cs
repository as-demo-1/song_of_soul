/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Utility
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Crafting;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Some useful functions for the Inventory System database.
    /// </summary>
    public static class InventoryDatabaseUtility
    {
        /// <summary>
        /// Filter a list using a condition delegate.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="listArg">The comparision argument.</param>
        /// <param name="arg">The filter parameter.</param>
        /// <param name="condition">The condition.</param>
        /// <typeparam name="T1">Source element Type.</typeparam>
        /// <typeparam name="T2">The comparison argument Type.</typeparam>
        /// <returns>The filtered list.</returns>
        public static List<T1> FilterList<T1, T2>(IList<T1> source, T2 arg, Func<T1, T2, bool> condition)
        {
            var filteredList = new List<T1>();

            if (source == null) { return filteredList; }
            for (int i = 0; i < source.Count; i++) {
                if (condition.Invoke(source[i], arg)) {
                    filteredList.Add(source[i]);
                }
            }

            return filteredList;
        }

        /// <summary>
        /// Get the Crafting Recipes that directly contain an ItemCategory as ingredient.
        /// </summary>
        /// <param name="source">The recipes.</param>
        /// <param name="itemCategory">The itemCategory.</param>
        /// <returns>The filtered list of recipes.</returns>
        public static List<CraftingRecipe> DirectItemCategoryRecipes(IList<CraftingRecipe> source,
            ItemCategory itemCategory)
        {
            return FilterList(source, itemCategory, (recipe, itemCat) =>
             {
                 if (recipe == null || recipe.Ingredients == null) { return false; }
                 for (int i = 0; i < recipe.Ingredients.ItemCategoryAmounts.Count; i++) {
                     var otherItemCat = recipe.Ingredients.ItemCategoryAmounts[i];
                     if (itemCat == otherItemCat.ItemCategory) { return true; }
                 }

                 return false;
             });
        }

        /// <summary>
        /// Get the Crafting Recipes that inherently contain an ItemCategory as ingredient.
        /// </summary>
        /// <param name="source">The recipes.</param>
        /// <param name="itemCategory">The itemCategory.</param>
        /// <returns>The filtered list of recipes.</returns>
        public static List<CraftingRecipe> InheritedItemCategoryRecipes(IList<CraftingRecipe> source,
            ItemCategory itemCategory)
        {
            return FilterList(source, itemCategory, (recipe, itemCat) =>
             {
                 if (recipe == null || recipe.Ingredients == null) { return false; }
                 for (int i = 0; i < recipe.Ingredients.ItemCategoryAmounts.Count; i++) {
                     var otherItemCat = recipe.Ingredients.ItemCategoryAmounts[i];
                     if (itemCat.InherentlyContains(otherItemCat.ItemCategory)) { return true; }
                 }

                 return false;
             });
        }

        /// <summary>
        /// Get the Crafting Recipes that directly contain an itemDefinition as ingredient or output.
        /// </summary>
        /// <param name="source">The recipes.</param>
        /// <param name="itemDefinition">The itemDefinition.</param>
        /// <returns>The filtered list of recipes.</returns>
        public static List<CraftingRecipe> DirectItemDefinitionRecipes(CraftingRecipe[] source, ItemDefinition itemDefinition)
        {
            return FilterList(source, itemDefinition, (recipe, itemDef) =>
             {
                 if (recipe == null || recipe.Ingredients == null) { return false; }
                 for (int i = 0; i < recipe.Ingredients.ItemDefinitionAmounts.Count; i++) {
                     var otherItemDef = recipe.Ingredients.ItemDefinitionAmounts[i];
                     if (itemDef == otherItemDef.ItemDefinition) { return true; }
                 }
                 for (int i = 0; i < recipe.Ingredients.ItemAmounts.Count; i++) {
                     var otherItem = recipe.Ingredients.ItemAmounts[i];
                     if (itemDef == otherItem.Item?.ItemDefinition) { return true; }
                 }

                 for (int i = 0; i < recipe.DefaultOutput.ItemAmounts.Count; i++) {
                     var otherItem = recipe.DefaultOutput.ItemAmounts[i];
                     if (itemDef == otherItem.Item?.ItemDefinition) { return true; }
                 }

                 return false;
             });
        }

        /// <summary>
        /// Get the Crafting Recipes that inherently contain an itemDefinition as ingredient or output.
        /// </summary>
        /// <param name="source">The recipes.</param>
        /// <param name="itemDefinition">The itemDefinition.</param>
        /// <returns>The filtered list of recipes.</returns>
        public static List<CraftingRecipe> InheritedItemDefinitionRecipes(CraftingRecipe[] source, ItemDefinition itemDefinition)
        {
            return FilterList(source, itemDefinition, (recipe, itemDef) =>
             {
                 if (recipe == null || recipe.Ingredients == null) { return false; }
                 for (int i = 0; i < recipe.Ingredients.ItemDefinitionAmounts.Count; i++) {
                     var otherItemDef = recipe.Ingredients.ItemDefinitionAmounts[i];
                     if (itemDef.InherentlyContains(otherItemDef.ItemDefinition)) { return true; }
                 }
                 for (int i = 0; i < recipe.Ingredients.ItemAmounts.Count; i++) {
                     var otherItem = recipe.Ingredients.ItemAmounts[i];
                     if (itemDef.InherentlyContains(otherItem.Item?.ItemDefinition)) { return true; }
                 }

                 for (int i = 0; i < recipe.DefaultOutput.ItemAmounts.Count; i++) {
                     var otherItem = recipe.DefaultOutput.ItemAmounts[i];
                     if (itemDef.InherentlyContains(otherItem.Item?.ItemDefinition)) { return true; }
                 }

                 return false;
             });
        }
    }
}
