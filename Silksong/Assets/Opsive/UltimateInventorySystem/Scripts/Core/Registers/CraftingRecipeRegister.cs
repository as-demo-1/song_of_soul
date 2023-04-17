/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.Registers
{
    using Opsive.UltimateInventorySystem.Crafting;

    /// <summary>
    /// The register of crafting recipes.
    /// </summary>
    public class CraftingRecipeRegister : InventoryObjectRegister<CraftingRecipe>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="register">The register.</param>
        public CraftingRecipeRegister(InventorySystemRegister register) : base(register)
        {
        }

        /// <summary>
        /// Add object to Dictionary.
        /// </summary>
        /// <param name="obj">The object.</param>
        protected override void AddInternal(CraftingRecipe obj)
        {
            for (int i = 0; i < obj.DefaultOutput.ItemAmounts.Count; i++) {
                obj.DefaultOutput.ItemAmounts[i].Item.Initialize(false);
            }
            for (int i = 0; i < obj.Ingredients.ItemAmounts.Count; i++) {
                obj.Ingredients.ItemAmounts[i].Item.Initialize(false);
            }
            base.AddInternal(obj);
        }
    }
}