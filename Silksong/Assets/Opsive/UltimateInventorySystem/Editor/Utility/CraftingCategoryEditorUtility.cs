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
    /// Static class that has useful function for crafting category.
    /// </summary>
    public class CraftingCategoryEditorUtility : CategoryEditorUtility<CraftingCategory, CraftingRecipe>
    {

        private static CraftingCategoryEditorUtility s_Instance;

        public static CraftingCategoryEditorUtility Instance => s_Instance ?? (s_Instance = new CraftingCategoryEditorUtility());

        /// <summary>
        /// Adds the Category to the database
        /// </summary>
        /// <param name="newCategoryName">The new category name.</param>
        /// <param name="database">The database to add to.</param>
        /// <param name="databaseDirectory">The database directory.</param>
        public static CraftingCategory AddCraftingCategory(string newCategoryName, InventorySystemDatabase database, string databaseDirectory)
        {
            var craftingCategory = CraftingCategory.Create(newCategoryName);
            Instance.AddCategoryInternal(
                craftingCategory, database,
                databaseDirectory + "\\CraftingCategory\\",
                "Add CraftingCategory");
            return craftingCategory;
        }

        /// <summary>
        /// Removes the CraftingCategory from any of its connections with the database
        /// </summary>
        /// <param name="category">The category to remove.</param>
        /// <param name="database">The database to remove from.</param>
        public static void RemoveCraftingCategory(CraftingCategory category, InventorySystemDatabase database)
        {
            Instance.RemoveCategoryInternal(category, database);
        }

        /// <summary>
        /// Duplicate the ItemCategory from any of its connections with the database
        /// </summary>
        /// <param name="originalCategory">The category to duplicate.</param>
        /// <param name="database">The database.</param>
        public static CraftingCategory DuplicateCraftingCategory(CraftingCategory originalCategory, InventorySystemDatabase database, string databaseDirectory)
        {

            var name = AssetDatabaseUtility.FindValidName(originalCategory.name, database.CraftingCategories);

            var newCategory = CraftingCategory.Create(name, originalCategory.IsAbstract, originalCategory.RecipeType);

            newCategory.AddParents(originalCategory.Parents);

            Instance.AddCategoryInternal(
                newCategory, database,
                databaseDirectory + "\\CraftingCategory\\",
                "Add CraftingCategory");

            for (int i = 0; i < newCategory.Parents.Count; i++) {
                SetCraftingCategoryDirty(newCategory.Parents[i], true);
            }

            SetCraftingCategoryDirty(newCategory, true);

            return newCategory;
        }

        /// <summary>
        /// Set the category IsAbstract, returns the new value of isAbstract, which can be different then the value provided 
        /// if something is preventing from letting you set the abstract field.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="isAbstract">The new is abstract value.</param>
        /// <returns>The new abstract value.</returns>
        public static bool SetIsAbstract(CraftingCategory category, bool isAbstract)
        {
            return Instance.SetIsAbstractInternal(category, isAbstract);
        }

        /// <summary>
        /// Set the category Color.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="newColor">The new color.</param>
        public static void SetColor(CraftingCategory category, Color newColor)
        {
            Instance.SetColorInternal(category, newColor);
        }

        /// <summary>
        /// Set the category Icon.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="newIcon">The new sprite.</param>
        public static void SetIcon(CraftingCategory category, Sprite newIcon)
        {
            Instance.SetIconInternal(category, newIcon);
        }

        /// <summary>
        /// Set the category RecipeType.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="recipeType">The recipe type.</param>
        public static void SetRecipeType(CraftingCategory category, Type recipeType)
        {
            Undo.RegisterCompleteObjectUndo(category, "Recipe Type Change");
            category.RecipeType = recipeType;
            Instance.SetCategoryDirtyInternal(category, false);
            var database = DatabaseValidator.Database;

            for (int i = category.Elements.Count - 1; i >= 0; i--) {
                var recipe = category.Elements[i];
                CraftingRecipeEditorUtility.ChangeCraftingRecipeType(recipe, category, database, DatabaseValidator.GetDatabaseDirectory(database));
            }

            Shared.Editor.Utility.EditorUtility.SetDirty(database);
        }

        /// <summary>
        /// Add an parent from its category.
        /// </summary>
        /// <param name="category">The CraftingCategory.</param>
        /// <param name="parent">The parent.</param>
        public static void AddCraftingCategoryParent(CraftingCategory category, CraftingCategory parent)
        {
            Instance.AddCategoryParentInternal(category, parent);
        }

        /// <summary>
        /// Remove an parent from its category.
        /// </summary>
        /// <param name="category">The CraftingCategory.</param>
        /// <param name="parent">The parent.</param>
        public static void RemoveCraftingCategoryParent(CraftingCategory category, CraftingCategory parent)
        {
            Instance.RemoveCategoryParentInternal(category, parent);
        }

        /// <summary>
        /// Remove an CraftingDefinition from its category, use when the CraftingDefinition changes category or is deleted.
        /// </summary>
        /// <param name="category">The category that the element belongs to.</param>
        /// <param name="element">The element that should be changed/removed.</param>
        /// <returns>True if the item definition was removed.</returns>
        public static bool RemoveCraftingRecipeFromCategory(CraftingCategory category, CraftingRecipe element)
        {
            return Instance.RemoveElementFromCategoryInternal(category, element);
        }

        /// <summary>
        /// Set category Dirty.
        /// </summary>
        /// <param name="category">The category to dirty.</param>
        /// <param name="updateRelationshipArrays">If true it will set the children array correctly.</param>
        public static void SetCraftingCategoryDirty(CraftingCategory category, bool force)
        {
            Instance.SetCategoryDirtyInternal(category, force);
        }

        /// <summary>
        /// Serialize the entire family of category
        /// </summary>
        /// <param name="category">The category.</param>
        public static void SerializeCategoryFamilyConnections(CraftingCategory category)
        {
            Instance.SerializeCategoryFamilyConnectionsInternal(category);
        }

        /// <summary>
        /// Serialize the entire family of category
        /// </summary>
        /// <param name="category">The category.</param>
        public static void SerializeCategoryChildrenConnections(CraftingCategory category)
        {
            Instance.SerializeCategoryChildrenConnectionsInternal(category);
        }

        /// <summary>
        /// Register all connections of Category for undo.
        /// </summary>
        /// <param name="category">The Category.</param>
        /// <param name="undoKey">The undo key.</param>
        public static void RegisterUndoCategoryFamilyConnections(CraftingCategory category, string undoKey)
        {
            Instance.RegisterUndoCategoryFamilyConnectionsInternal(category, undoKey);
        }

        /// <summary>
        /// Register all children connections of Category for undo.
        /// </summary>
        /// <param name="category">The Category.</param>
        /// <param name="undoKey">The undo key.</param>
        public static void RegisterUndoCategoryChildrenConnections(CraftingCategory category, string undoKey)
        {
            Instance.RegisterUndoCategoryChildrenConnectionsInternal(category, undoKey);
        }

        /// <summary>
        /// Add the category to the database.
        /// </summary>
        /// <param name="category">The category to add.</param>
        /// <param name="database">The database to add to.</param>
        protected override void AddToDatabaseInternal(CraftingCategory category, InventorySystemDatabase database)
        {
            database.AddCraftingCategory(category);
        }

        /// <summary>
        /// Remove the category from the database.
        /// </summary>
        /// <param name="category">The category to remove.</param>
        /// <param name="database">The database to remove from.</param>
        protected override void RemoveFromDatabaseInternal(CraftingCategory category, InventorySystemDatabase database)
        {
            database.RemoveRecipeCategory(category);
        }

        /// <summary>
        /// Set the element category.
        /// </summary>
        /// <param name="element">The element to set.</param>
        /// <param name="newCategory">The elements new category.</param>
        protected override void SetElementCategoryInternal(CraftingRecipe element, CraftingCategory newCategory)
        {
            CraftingRecipeEditorUtility.SetCraftingRecipeCategory(element, newCategory);
        }

        /// <summary>
        /// Set the element to dirty.
        /// </summary>
        /// <param name="element">The element to serialize.</param>
        protected override void SetElementDirtyInternal(CraftingRecipe element)
        {
            CraftingRecipeEditorUtility.SetCraftingRecipeDirty(element, false);
        }

        /// <summary>
        /// Returns a list of options for sorting.
        /// </summary>
        /// <returns>The list of sort options.</returns>
        public static IList<SortOption> SortOptions()
        {
            return new SortOption[]
            {
                new SortOption("A-Z", list => (list as List<CraftingCategory>).Sort(
                    (x, y) =>
                    {
                        if (x == null) { return -1; }
                        if (y == null) { return 1; }
                        if (x == DatabaseValidator.UncategorizedCraftingCategory) { return -1; }
                        if (y == DatabaseValidator.UncategorizedCraftingCategory) { return 1; }
                        return x?.name.CompareTo(y?.name) ?? 1;
                    })),
                new SortOption("Z-A", list => (list as List<CraftingCategory>).Sort(
                    (x, y) =>
                    {
                        if (x == null) { return -1; }
                        if (y == null) { return 1; }
                        if (x == DatabaseValidator.UncategorizedCraftingCategory) { return -1; }
                        if (y == DatabaseValidator.UncategorizedCraftingCategory) { return 1; }
                        return y?.name.CompareTo(x?.name) ?? 1;
                    })),
                new SortOption("Hierarchy",list => (list as List<CraftingCategory>).Sort(
                    (x, y) =>
                    {
                        if (x == null) { return -1; }
                        if (y == null) { return 1; }
                        if (x == DatabaseValidator.UncategorizedCraftingCategory) { return -1; }
                        if (y == DatabaseValidator.UncategorizedCraftingCategory) { return 1; }
                        return 0;
                    }))
            };
        }

        /// <summary>
        /// Search filter for the category list
        /// </summary>
        /// <param name="list">The list to search.</param>
        /// <param name="searchValue">The search string.</param>
        /// <returns>A new filtered list.</returns>
        public static IList<CraftingCategory> SearchFilter(IList<CraftingCategory> list, string searchValue)
        {
            var compareInfo = CultureInfo.CurrentCulture.CompareInfo;

            return ManagerUtility.SearchFilter(list, searchValue,
                new (string prefix, Func<string, CraftingCategory, bool>)[]
                {
                });
        }
    }
}