/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Utility
{
    using Opsive.UltimateInventorySystem.Crafting;
    using Opsive.UltimateInventorySystem.Editor.Managers;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Static class that has useful function for craftin recipe.
    /// </summary>
    public static class CraftingRecipeEditorUtility
    {
        /// <summary>
        /// Add an itemDefinition to the database.
        /// </summary>
        /// <param name="newRecipeName">The itemDefinition name.</param>
        /// <param name="craftingCategory">The itemCategory.</param>
        /// <param name="database">The database.</param>
        /// <param name="assetDirectory">The asset directory.</param>
        public static CraftingRecipe AddCraftingRecipe(string newRecipeName, CraftingCategory craftingCategory,
            InventorySystemDatabase database, string assetDirectory)
        {
            Undo.RegisterCompleteObjectUndo(database, "Add Recipe");
            Undo.RegisterCompleteObjectUndo(craftingCategory, "Add Recipe");
            // Create the ScriptableObject representing the category and add the category to the database.
            var recipe = CraftingRecipe.Create(newRecipeName, craftingCategory);
            if (recipe == null) {
                Debug.LogError("Error: The recipe cannot be created.");
                return null;
            }

            Undo.RegisterCreatedObjectUndo(recipe, "Add Recipe");

            database.AddRecipe(recipe);

            AssetDatabaseUtility.CreateAsset(recipe,
                $"{assetDirectory}\\CraftingRecipes\\{recipe.name}",
                new string[] { database.name, recipe.Category.name });

            SetCraftingRecipeDirty(recipe, true);
            CraftingCategoryEditorUtility.SetCraftingCategoryDirty(craftingCategory, true);
            Shared.Editor.Utility.EditorUtility.SetDirty(database);
            return recipe;
        }

        /// <summary>
        /// Removes the ItemDefinition from any of its connections with the database
        /// </summary>
        /// <param name="recipe">The recipe to remove.</param>
        /// <param name="database">The database to remove from.</param>
        public static void RemoveCraftingRecipe(CraftingRecipe recipe, InventorySystemDatabase database)
        {
            if (recipe == null) { return; }

            CraftingCategoryEditorUtility.RemoveCraftingRecipeFromCategory(recipe.Category, recipe);

            database.RemoveRecipe(recipe);
            AssetDatabaseUtility.DeleteAsset(recipe);
            Shared.Editor.Utility.EditorUtility.SetDirty(database);
        }

        /// <summary>
        /// Add an itemDefinition to the database.
        /// </summary>
        /// <param name="originalRecipe">The itemCategory.</param>
        /// <param name="database">The database.</param>
        /// <param name="assetDirectory">The asset directory.</param>
        public static CraftingRecipe DuplicateCraftingRecipe(CraftingRecipe originalRecipe,
            InventorySystemDatabase database, string assetDirectory)
        {
            Undo.RegisterCompleteObjectUndo(database, "Add Recipe");
            Undo.RegisterCompleteObjectUndo(originalRecipe, "Add Recipe");

            var name = AssetDatabaseUtility.FindValidName(originalRecipe.name, database.CraftingRecipes);

            // Create the ScriptableObject representing the category and add the category to the database.
            var recipe = CraftingRecipe.CreateFrom(name, originalRecipe.Category, originalRecipe);
            if (recipe == null) {
                Debug.LogError("Error: The recipe cannot be created.");
                return null;
            }

            Undo.RegisterCreatedObjectUndo(recipe, "Add Recipe");

            database.AddRecipe(recipe);

            AssetDatabaseUtility.CreateAsset(recipe,
                $"{assetDirectory}\\CraftingRecipes\\{recipe.name}",
                new string[] { database.name, recipe.Category.name });

            CraftingCategoryEditorUtility.SetCraftingCategoryDirty(recipe.Category, true);
            SetCraftingRecipeDirty(recipe, true);
            Shared.Editor.Utility.EditorUtility.SetDirty(database);
            return recipe;
        }

        /// <summary>
        /// Change the type of the crafting Recipe
        /// </summary>
        /// <param name="recipe">The recipe.</param>
        /// <param name="category">The crafting category.</param>
        /// <param name="database">The database.</param>
        /// <param name="assetDirectory">The asset Directory.</param>
        /// <returns>The new crafting recipe.</returns>
        public static CraftingRecipe ChangeCraftingRecipeType(CraftingRecipe recipe, CraftingCategory category, InventorySystemDatabase database, string assetDirectory)
        {
            Undo.RegisterCompleteObjectUndo(database, "Change Recipe");
            Undo.RegisterCompleteObjectUndo(category, "Change Recipe");

            recipe.Category.Manager?.Register.CraftingRecipeRegister.Unregister(recipe);

            var newRecipe = CraftingRecipe.CreateFrom(recipe.name, category, recipe);
            if (newRecipe == null) {
                Debug.LogError("Error: The recipe cannot be created.");
                return null;
            }

            Undo.RegisterCreatedObjectUndo(newRecipe, "Add Recipe");

            RemoveCraftingRecipe(recipe, database);

            database.AddRecipe(newRecipe);

            AssetDatabaseUtility.CreateAsset(newRecipe,
                $"{assetDirectory}\\CraftingRecipes\\{newRecipe.name}",
                new string[] { database.name, newRecipe.Category.name });

            CraftingCategoryEditorUtility.SetCraftingCategoryDirty(newRecipe.Category, true);
            SetCraftingRecipeDirty(recipe, true);
            Shared.Editor.Utility.EditorUtility.SetDirty(database);
            return newRecipe;
        }

        /// <summary>
        /// Set Recipe Dirty & sets the children array if necessary
        /// </summary>
        /// <param name="element">The recipe to serialize.</param>
        /// <param name="force">Force the dirtying.</param>
        public static void SetCraftingRecipeDirty(CraftingRecipe element, bool force)
        {
            if (element.Dirty == false && !force) { return; }
            element.Serialize();

            Shared.Editor.Utility.EditorUtility.SetDirty(element);
            element.Dirty = false;
        }

        /// <summary>
        /// Set the ItemDefinition Category and serialize.
        /// </summary>
        /// <param name="recipe">The ItemDefinition.</param>
        /// <param name="newCategory">The new ItemCategory.</param>
        public static CraftingRecipe SetCraftingRecipeCategory(CraftingRecipe recipe, CraftingCategory newCategory)
        {
            if (newCategory == null) { newCategory = DatabaseValidator.UncategorizedCraftingCategory; }

            var previousCategory = recipe.Category;

            if (previousCategory != null) { Undo.RegisterCompleteObjectUndo(previousCategory, "Category Change"); }
            if (recipe != null) { Undo.RegisterCompleteObjectUndo(recipe, "Category Change"); }
            if (newCategory != null) { Undo.RegisterCompleteObjectUndo(newCategory, "Category Change"); }

            if (recipe.GetType() != newCategory.RecipeType) {
                recipe = ChangeCraftingRecipeType(recipe, newCategory, DatabaseValidator.Database,
                    DatabaseValidator.GetDatabaseDirectory(DatabaseValidator.Database));
            } else {
                recipe.SetCategory(newCategory);
            }

            if (previousCategory != null) { CraftingCategoryEditorUtility.SetCraftingCategoryDirty(previousCategory, true); }
            if (recipe != null) { SetCraftingRecipeDirty(recipe, true); }
            if (newCategory != null) { CraftingCategoryEditorUtility.SetCraftingCategoryDirty(newCategory, true); }

            return recipe;
        }

        /// <summary>
        /// Set the Icon.
        /// </summary>
        /// <param name="recipe">The recipe.</param>
        /// <param name="newIcon">The new icon.</param>
        public static void SetIcon(CraftingRecipe recipe, Sprite newIcon)
        {
            Undo.RegisterCompleteObjectUndo(recipe, "Icon Change");
            recipe.m_EditorIcon = newIcon;
            SetCraftingRecipeDirty(recipe, true);
        }

        /// <summary>
        /// Returns a list of options for sorting.
        /// </summary>
        /// <returns>The list of sort options.</returns>
        public static IList<SortOption> SortOptions()
        {
            return new SortOption[]
            {
                new SortOption("A-Z", list => (list as List<CraftingRecipe>).Sort(
                    (x, y) =>
                    {
                        if (x == null) { return -1;}
                        if (y == null) { return 1; }
                        return x?.name.CompareTo(y?.name ?? "") ?? 1;
                    })),
                new SortOption("Z-A", list => (list as List<CraftingRecipe>).Sort(
                    (x, y) =>
                    {
                        if (x == null) { return -1;}
                        if (y == null) { return 1; }
                        return y?.name.CompareTo(x?.name ?? "") ?? 1;
                    })),
                new SortOption("Category A-Z",list => (list as List<CraftingRecipe>).Sort(
                    (x, y) =>
                    {
                        if (x?.Category == DatabaseValidator.UncategorizedCraftingCategory) { return -1; }
                        if (y?.Category == DatabaseValidator.UncategorizedCraftingCategory) { return 1; }
                        return x?.Category.name.CompareTo(y?.Category.name ?? "") ?? 1;
                    })),
                new SortOption("Category Z-A",list => (list as List<CraftingRecipe>).Sort(
                    (x, y) =>
                    {
                        if (x?.Category == DatabaseValidator.UncategorizedCraftingCategory) { return -1; }
                        if (y?.Category == DatabaseValidator.UncategorizedCraftingCategory) { return 1; }
                        return y?.Category.name.CompareTo(x?.Category.name ?? "") ?? 1;
                    })),
            };
        }

        /// <summary>
        /// Search filter for the ItemDefinition list
        /// </summary>
        /// <param name="list">The list to search.</param>
        /// <param name="searchValue">The search string.</param>
        /// <returns>A new filtered list.</returns>
        public static IList<CraftingRecipe> SearchFilter(IList<CraftingRecipe> list, string searchValue)
        {
            var compareInfo = CultureInfo.CurrentCulture.CompareInfo;

            return ManagerUtility.SearchFilter(list, searchValue,
                new (string prefix, Func<string, CraftingRecipe, bool>)[]
                {
                    //search by attribute
                    ("c:",(searchWord, itemDefinition) =>{
                        var categorySearchWord = searchWord.Remove(0, 2);
                        // Case insensitive Contains(string).
                        if (compareInfo.IndexOf(itemDefinition.Category.name, categorySearchWord,
                                CompareOptions.IgnoreCase) >= 0) { return true; }

                        return false;
                    })
                });
        }
    }
}
