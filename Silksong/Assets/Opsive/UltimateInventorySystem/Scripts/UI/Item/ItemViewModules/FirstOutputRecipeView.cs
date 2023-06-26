/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.UltimateInventorySystem.Crafting;
    using Opsive.UltimateInventorySystem.UI.Views;
    using UnityEngine;
    using UnityEngine.Serialization;

    /// <summary>
    /// Recipe box UI which references a ItemBoxUI and set the main recipe output to it.
    /// </summary>
    public class FirstOutputRecipeView : ViewModule<CraftingRecipe>
    {
        [FormerlySerializedAs("m_ItemBoxObject")]
        [Tooltip("The Item View UI for the recipe output item.")]
        [SerializeField] protected ItemView m_ItemView;

        /// <summary>
        /// Set the recipe.
        /// </summary>
        /// <param name="recipe">The new recipe value.</param>
        public override void SetValue(CraftingRecipe recipe)
        {
            if (recipe == null) {
                Clear();
                return;
            }

            var itemAmounts = recipe.DefaultOutput?.ItemAmounts;
            if (itemAmounts == null || itemAmounts.Count == 0) {
                Debug.LogWarning($"Recipe '{recipe}' is missing a main output item.");
                return;
            }

            m_ItemView.SetValue((itemAmounts[0], null));
        }

        /// <summary>
        /// Clear the box.
        /// </summary>
        public override void Clear()
        {
            m_ItemView.Clear();
        }
    }
}