/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.Registers
{
    using Opsive.UltimateInventorySystem.Crafting;

    /// <summary>
    /// The register for the crafting categories.
    /// </summary>
    public class CraftingCategoryRegister : InventoryObjectRegister<CraftingCategory>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="register">The register.</param>
        public CraftingCategoryRegister(InventorySystemRegister register) : base(register)
        {
        }

        /// <summary>
        /// Delete an itemCategory.
        /// </summary>
        /// <param name="craftingCategory">The crafting category.</param>
        public override void Delete(CraftingCategory craftingCategory)
        {
            var recipesCount = craftingCategory.ElementsReadOnly.Count;
            for (int i = 0; i < recipesCount; i++) {
                m_Register.CraftingRecipeRegister.Delete(craftingCategory.ElementsReadOnly[i]);
            }

            while (craftingCategory.ChildrenReadOnly.Count != 0) {
                var child = craftingCategory.ChildrenReadOnly[0];
                child.RemoveParent(craftingCategory);

                var parentCount = craftingCategory.ParentsReadOnly.Count;
                for (int i = 0; i < parentCount; i++) {
                    child.AddParent(craftingCategory.ParentsReadOnly[i]);
                }
            }

            while (craftingCategory.ParentsReadOnly.Count != 0) {
                var parent = craftingCategory.ParentsReadOnly[0];
                craftingCategory.RemoveParent(parent);
            }

            Unregister(craftingCategory);
        }
    }
}