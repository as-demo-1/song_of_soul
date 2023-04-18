/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Crafting
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using UnityEngine;

    /// <summary>
    /// Ingredients data in a crafting recipe.
    /// </summary>
    [System.Serializable]
    public class CraftingIngredients
    {
        [Tooltip("Amounts of item category.")]
        [SerializeField] [HideInInspector] protected ItemCategoryAmounts m_ItemCategoryAmounts;
        [Tooltip("Amounts of item definition.")]
        [SerializeField] [HideInInspector] protected ItemDefinitionAmounts m_ItemDefinitionAmounts;
        [Tooltip("Amounts of item.")]
        [SerializeField] [HideInInspector] protected ItemAmounts m_ItemAmounts;

        public ItemCategoryAmounts ItemCategoryAmounts => m_ItemCategoryAmounts;
        public ItemDefinitionAmounts ItemDefinitionAmounts => m_ItemDefinitionAmounts;
        public ItemAmounts ItemAmounts => m_ItemAmounts;

        /// <summary>
        /// Default Constructor used to define the crafting ingredients.
        /// </summary>
        public CraftingIngredients()
        {
            m_ItemCategoryAmounts = new ItemCategoryAmounts();
            m_ItemDefinitionAmounts = new ItemDefinitionAmounts();
            m_ItemAmounts = new ItemAmounts();
        }

        /// <summary>
        /// Constructor used to define the crafting ingredients.
        /// </summary>
        /// <param name="itemCategoryAmounts">The item category amounts.</param>
        /// <param name="itemDefinitionAmounts">The item definition amounts.</param>
        /// <param name="ItemAmounts">The item amounts.</param>
        /// <param name="currencyAmounts">The currency amounts.</param>
        public CraftingIngredients(ItemCategoryAmounts itemCategoryAmounts = null,
            ItemDefinitionAmounts itemDefinitionAmounts = null,
            ItemAmounts ItemAmounts = null)
        {
            m_ItemCategoryAmounts = itemCategoryAmounts ?? new ItemCategoryAmounts();
            m_ItemDefinitionAmounts = itemDefinitionAmounts ?? new ItemDefinitionAmounts();
            m_ItemAmounts = ItemAmounts ?? new ItemAmounts();
        }
    }
}