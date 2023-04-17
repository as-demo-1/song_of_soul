/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.DataContainers
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Crafting;
    using Opsive.UltimateInventorySystem.Storage;
    using Opsive.UltimateInventorySystem.UI.Views;
    using System;
    using UnityEngine;
    using UnityEngine.Serialization;

    /// <summary>
    /// A scriptable object to map Crafting categories to Recipe boxes.
    /// </summary>
    [CreateAssetMenu(fileName = "CategoryRecipeBoxSet", menuName = "Ultimate Inventory System/UI/Category Recipe Box Set", order = 1)]
    public class CategoryRecipeViewSet : ScriptableObject, IDatabaseSwitcher
    {

        [FormerlySerializedAs("m_EmptyRecipeBox")]
        [Tooltip("The recipe box when the recipe is null.")]
        [SerializeField] protected GameObject m_EmptyRecipeView;

        [FormerlySerializedAs("m_CategoriesRecipeBoxes")]
        [Tooltip("The recipe box mapped to a recipe category.")]
        [SerializeField] protected CategoryRecipeViews[] m_CategoriesRecipeViews;

        public CategoryRecipeViews[] CategoriesRecipeBoxUIArray {
            get => m_CategoriesRecipeViews;
            set => m_CategoriesRecipeViews = value;
        }

        /// <summary>
        /// Find the recipe box UI prefab for the recipe specified.
        /// </summary>
        /// <param name="recipe">The recipe.</param>
        /// <returns>The prefab with the recipe box UI.</returns>
        public GameObject FindRecipeBoxPrefabForItem(CraftingRecipe recipe)
        {
            if (recipe == null) {
                if (m_EmptyRecipeView == null) {
                    Debug.LogWarning($"The Empty Recipe Box must be specified.");
                }

                return m_EmptyRecipeView;
            }

            var selectedCategoryRecipeBoxUI = new CategoryRecipeViews();
            for (int i = 0; i < m_CategoriesRecipeViews.Length; i++) {
                var category = m_CategoriesRecipeViews[i].Category;

                if (category == null) {
                    if (selectedCategoryRecipeBoxUI.Category == null) {
                        selectedCategoryRecipeBoxUI = m_CategoriesRecipeViews[i];
                    }
                    continue;
                }

                if (category.InherentlyContains(recipe) == false) { continue; }

                if (selectedCategoryRecipeBoxUI.Category != null
                   && category.InherentlyContains(selectedCategoryRecipeBoxUI.Category)) { continue; }

                selectedCategoryRecipeBoxUI = m_CategoriesRecipeViews[i];
            }

            if (selectedCategoryRecipeBoxUI.RecipeViewPrefab == null
                || selectedCategoryRecipeBoxUI.RecipeViewPrefab.GetComponent<View<CraftingRecipe>>() == null) {
                Debug.LogWarning($"Missing an itemBoxUi type for {recipe?.Category?.name} category.");
            }

            return selectedCategoryRecipeBoxUI.RecipeViewPrefab;
        }

        /// <summary>
        /// Check if the object contained by this component are part of the database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>True if all the objects in the component are part of that database.</returns>
        bool IDatabaseSwitcher.IsComponentValidForDatabase(InventorySystemDatabase database)
        {
            if (database == null) { return false; }

            for (int i = 0; i < m_CategoriesRecipeViews.Length; i++) {
                var category = m_CategoriesRecipeViews[i].Category;
                if (database.Contains(category)) { continue; }

                return false;

            }

            return true;
        }

        /// <summary>
        /// Replace any object that is not in the database by an equivalent object in the specified database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>The objects that have been changed.</returns>
        ModifiedObjectWithDatabaseObjects IDatabaseSwitcher.ReplaceInventoryObjectsBySelectedDatabaseEquivalents(InventorySystemDatabase database)
        {
            if (database == null) { return null; }

            for (int i = 0; i < m_CategoriesRecipeViews.Length; i++) {
                var category = m_CategoriesRecipeViews[i].Category;
                if (database.Contains(category)) { continue; }

                category = database.FindSimilar(category);

                m_CategoriesRecipeViews[i] =
                    new CategoryRecipeViews(category,
                        m_CategoriesRecipeViews[i].RecipeViewPrefab);

            }

            return null;
        }
    }

    /// <summary>
    /// The struct with a category and a game object.
    /// </summary>
    [Serializable]
    public struct CategoryRecipeViews
    {
        [Tooltip("The crafting category.")]
        [SerializeField] private DynamicCraftingCategory m_Category;


        [FormerlySerializedAs("m_RecipeBoxPrefab")]
        [Tooltip("The recipe box prefab.")]
        [SerializeField] private GameObject m_RecipeViewPrefab;

        public CraftingCategory Category => m_Category;
        public GameObject RecipeViewPrefab => m_RecipeViewPrefab;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="recipeViewPrefab">The prefab.</param>
        public CategoryRecipeViews(CraftingCategory category, GameObject recipeViewPrefab)
        {
            m_Category = category;
            m_RecipeViewPrefab = recipeViewPrefab;
        }
    }
}