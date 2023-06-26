/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Crafting
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Storage;
    using System.Collections.Generic;
    using Opsive.Shared.UI;
    using UnityEngine;

    /// <summary>
    /// The crafter component is used to hold the recipes which the player will use to craft items.
    /// </summary>
    public class Crafter : MonoBehaviour, IDatabaseSwitcher
    {
        [Tooltip("The item collections serialized data.")]
        [SerializeField] protected Serialization m_CraftingProcessorData;
        [Tooltip("The recipes to display in the menu.")]
        [SerializeField] protected DynamicCraftingRecipeArray m_MiscellaneousRecipes;
        [Tooltip("The recipes with the categories specified will be visible in the menu.")]
        [SerializeField] protected DynamicCraftingCategoryArray m_CraftingCategories;

        protected bool m_IsInitialized;
        protected List<CraftingRecipe> m_CraftingRecipes;
        protected CraftingProcessor m_Processor;

        public CraftingRecipe[] MiscellaneousRecipes {
            get => m_MiscellaneousRecipes;
            set => m_MiscellaneousRecipes = value;
        }

        public CraftingCategory[] CraftingCategories {
            get => m_CraftingCategories;
            set => m_CraftingCategories = value;
        }

        public virtual CraftingProcessor Processor {
            get => m_Processor;
            set => m_Processor = value;
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        private void Awake()
        {
            Initialize(false);
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        public virtual void Initialize(bool force)
        {
            if (m_IsInitialized && force) { return; }

            m_IsInitialized = true;

            if ((this as IDatabaseSwitcher).IsComponentValidForDatabase(InventorySystemManager.Instance.Database) == false) {
                Debug.LogError("The Crafting interactable behavior has recipes and crafting categories from the wrong database, please fix it.");
                return;
            }

            m_CraftingRecipes = new List<CraftingRecipe>(m_MiscellaneousRecipes.Value);
            Deserialize();

            for (int i = 0; i < CraftingCategories.Length; i++) {
                var pooledArray = GenericObjectPool.Get<CraftingRecipe[]>();
                var recipesCount = CraftingCategories[i].GetAllChildrenElements(ref pooledArray);
                for (int j = 0; j < recipesCount; j++) {
                    m_CraftingRecipes.Add(pooledArray[j]);
                }
                GenericObjectPool.Return(pooledArray);
            }
        }

        /// <summary>
        /// Deserialize the Crafting Processor.
        /// </summary>
        public void Deserialize()
        {
            if (m_CraftingProcessorData != null) {
                m_Processor = m_CraftingProcessorData.DeserializeFields(MemberVisibility.Public) as CraftingProcessor;
            }
        }

        /// <summary>
        /// Serialize the Crafting processor.
        /// </summary>
        public void Serialize()
        {
            m_CraftingProcessorData = Serialization.Serialize(m_Processor);
        }

        /// <summary>
        /// Get the recipes.
        /// </summary>
        /// <returns>A list of the recipes.</returns>
        public virtual ListSlice<CraftingRecipe> GetRecipes()
        {
            return m_CraftingRecipes;
        }

        /// <summary>
        /// Add a recipe to the list of recipes.
        /// </summary>
        /// <param name="recipe">The recipe to add.</param>
        public virtual void AddRecipe(CraftingRecipe recipe)
        {
            if (m_CraftingRecipes.Contains(recipe)) { return; }
            m_CraftingRecipes.Add(recipe);
        }

        /// <summary>
        /// Remove a recipe.
        /// </summary>
        /// <param name="recipe">The recipe to remove.</param>
        public virtual void RemoveRecipe(CraftingRecipe recipe)
        {
            if (m_CraftingRecipes.Contains(recipe)) { return; }
            m_CraftingRecipes.Remove(recipe);
        }

        /// <summary>
        /// Set all the recipes.
        /// </summary>
        /// <param name="recipes">The recipes to set.</param>
        public virtual void SetRecipes(ListSlice<CraftingRecipe> recipes)
        {
            m_CraftingRecipes.Clear();
            for (int i = 0; i < recipes.Count; i++) {
                m_CraftingRecipes.Add(recipes[i]);
            }
        }

        /// <summary>
        /// Check if the object contained by this component are part of the database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>True if all the objects in the component are part of that database.</returns>
        bool IDatabaseSwitcher.IsComponentValidForDatabase(InventorySystemDatabase database)
        {
            if (database == null) { return false; }

            var recipes = MiscellaneousRecipes;
            if (recipes != null) {
                for (int i = 0; i < recipes.Length; i++) {
                    if (database.Contains(recipes[i])) { continue; }

                    return false;
                }
            }

            var categories = CraftingCategories;
            if (categories != null) {
                for (int i = 0; i < categories.Length; i++) {
                    if (database.Contains(categories[i])) { continue; }

                    return false;
                }
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

            var recipes = MiscellaneousRecipes;
            if (recipes != null) {
                for (int i = 0; i < recipes.Length; i++) {
                    if (database.Contains(recipes[i])) { continue; }

                    recipes[i] = database.FindSimilar(recipes[i]);
                }

                m_MiscellaneousRecipes = new DynamicCraftingRecipeArray(recipes);
            }


            var categories = CraftingCategories;
            if (categories != null) {
                for (int i = 0; i < categories.Length; i++) {
                    if (database.Contains(categories[i])) { continue; }

                    categories[i] = database.FindSimilar(categories[i]);
                }

                m_CraftingCategories = new DynamicCraftingCategoryArray(categories);
            }

            return null;
        }
    }
}