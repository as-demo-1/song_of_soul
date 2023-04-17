/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Crafting.IngredientsTypes
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Exchange;
    using UnityEngine;

    /// <summary>
    /// The crafting ingredients with currency included.
    /// </summary>
    [System.Serializable]
    public class CraftingIngredientsWithCurrency : CraftingIngredients
    {
        [Tooltip("Amounts of currency.")]
        [SerializeField] [HideInInspector] protected CurrencyAmounts m_CurrencyAmounts;

        public CurrencyAmounts CurrencyAmounts => m_CurrencyAmounts;

        /// <summary>
        /// Default Constructor used to define the crafting ingredients.
        /// </summary>
        public CraftingIngredientsWithCurrency() : base()
        {
            m_CurrencyAmounts = new CurrencyAmounts();
        }

        /// <summary>
        /// Constructor used to define the crafting ingredients.
        /// </summary>
        /// <param name="itemCategoryAmounts">The item category amounts.</param>
        /// <param name="itemDefinitionAmounts">The item definition amounts.</param>
        /// <param name="ItemAmounts">The item amounts.</param>
        /// <param name="currencyAmounts">The currency amounts.</param>
        public CraftingIngredientsWithCurrency(ItemCategoryAmounts itemCategoryAmounts = null,
            ItemDefinitionAmounts itemDefinitionAmounts = null,
            ItemAmounts ItemAmounts = null,
            CurrencyAmounts currencyAmounts = null) : base(itemCategoryAmounts, itemDefinitionAmounts,
            ItemAmounts)
        {
            m_CurrencyAmounts = currencyAmounts ?? new CurrencyAmounts();
        }
    }
}
