/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.Crafting
{
    using Opsive.UltimateInventorySystem.Crafting;
    using Opsive.UltimateInventorySystem.UI.DataContainers;
    using Opsive.UltimateInventorySystem.UI.Views;
    using UnityEngine;
    using UnityEngine.Serialization;

    /// <summary>
    /// The crafting recipe box drawer.
    /// </summary>
    public class CraftingRecipeViewDrawer : ViewDrawer<CraftingRecipe>
    {
        [FormerlySerializedAs("m_CategoryRecipeBoxSet")]
        [FormerlySerializedAs("m_CategoriesRecipeBoxes")]
        [Tooltip("The categories recipe boxes.")]
        [SerializeField] protected CategoryRecipeViewSet m_CategoryRecipeViewSet;

        public CategoryRecipeViewSet CategoryRecipeViewSet {
            get { return m_CategoryRecipeViewSet; }
            set { m_CategoryRecipeViewSet = value; }
        }

        /// <summary>
        /// Get the recipe box prefab for the crafting recipe.
        /// </summary>
        /// <param name="craftingRecipe">The crafting recipe.</param>
        /// <returns>The prefab game object.</returns>
        public override GameObject GetViewPrefabFor(CraftingRecipe craftingRecipe)
        {
            return m_CategoryRecipeViewSet.FindRecipeBoxPrefabForItem(craftingRecipe);
        }
    }
}