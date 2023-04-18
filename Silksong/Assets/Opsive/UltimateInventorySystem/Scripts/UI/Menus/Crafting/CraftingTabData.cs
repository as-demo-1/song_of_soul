/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Menus.Crafting
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Crafting;
    using Opsive.UltimateInventorySystem.UI.Grid;
    using UnityEngine;

    /// <summary>
    /// Crafting tab data used by the Crafting Menu.
    /// </summary>
    public class CraftingTabData : MonoBehaviour, IFilterSorter<CraftingRecipe>
    {
        [Tooltip("The item category of the result of the recipe.")]
        [SerializeField] protected DynamicItemCategory m_ItemCategory;
        [Tooltip("The crafting category of the recipe.")]
        [SerializeField] protected DynamicCraftingCategory m_CraftingCategory;

        public ItemCategory ItemCategory => m_ItemCategory;
        public CraftingCategory CraftingCategory => m_CraftingCategory;

        protected bool m_IsInitialized;
        protected IFilterSorter<CraftingRecipe> m_CraftingRecipeFilter;

        public IFilterSorter<CraftingRecipe> CraftingFilter => m_CraftingRecipeFilter;

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="force">Force initialize.</param>
        public virtual void Initialize(bool force)
        {
            if (m_IsInitialized && !force) { return; }

            m_CraftingRecipeFilter = this;
        }

        /// <summary>
        /// Filter the recipes by category.
        /// </summary>
        /// <param name="input">The recipes input.</param>
        /// <param name="outputPooledArray">The recipes after the filter.</param>
        /// <returns>The filtered recipes.</returns>
        public ListSlice<CraftingRecipe> Filter(ListSlice<CraftingRecipe> input, ref CraftingRecipe[] outputPooledArray)
        {
            var itemCategory = m_ItemCategory.Value;
            var craftingCategory = m_CraftingCategory.Value;

            var count = 0;
            for (int i = 0; i < input.Count; i++) {

                var recipe = input[i];

                if (FilterItemCategory(recipe, itemCategory) == false) {
                    continue;
                }

                if (FilterCraftingCategory(craftingCategory, recipe) == false) {
                    continue;
                }

                count++;
                outputPooledArray.ResizeIfNecessary(ref outputPooledArray, count);
                outputPooledArray[count - 1] = recipe;
            }

            return (outputPooledArray, 0, count);
        }

        /// <summary>
        /// Filter by crafting category.
        /// </summary>
        /// <param name="craftingCategory">The crafting category.</param>
        /// <param name="recipe">The recipe.</param>
        /// <returns>True if they match.</returns>
        private bool FilterCraftingCategory(CraftingCategory craftingCategory, CraftingRecipe recipe)
        {
            if (m_CraftingCategory.HasValue == false) { return true; }

            var inherentlyContains = craftingCategory.InherentlyContains(recipe);
            if (inherentlyContains == false) { return false; }

            return true;
        }

        /// <summary>
        /// Filter by item category.
        /// </summary>
        /// <param name="recipe">The recipe.</param>
        /// <param name="itemCategory">The item category.</param>
        /// <returns>True if they match.</returns>
        private bool FilterItemCategory(CraftingRecipe recipe, ItemCategory itemCategory)
        {
            if (m_ItemCategory.HasValue == false) { return true; }
            if (recipe == null) { return false;}

            var itemResult = recipe.DefaultOutput.MainItemAmount;
            if (itemResult.HasValue == false) { return true; }

            var inherentlyContains = itemCategory.InherentlyContains(itemResult.Value.Item);
            if (inherentlyContains == false) { return false; }

            return true;
        }
    }
}