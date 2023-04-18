/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.Registers
{
    /// <summary>
    /// The register for Item Categories.
    /// </summary>
    public class ItemCategoryRegister : InventoryObjectRegister<ItemCategory>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="register">The inventory system register.</param>
        public ItemCategoryRegister(InventorySystemRegister register) : base(register)
        {
        }

        /// <summary>
        /// Remove internal removes the object form the dictionaries.
        /// </summary>
        /// <param name="obj">The object.</param>
        protected override void RemoveInternal(ItemCategory obj)
        {
            base.RemoveInternal(obj);
            m_Register.ItemRegister.ItemsOrganizedByCategoryID.Remove(obj.ID);
        }

        /// <summary>
        /// Delete an itemCategory.
        /// </summary>
        /// <param name="itemCategory">The item category.</param>
        public override void Delete(ItemCategory itemCategory)
        {
            var itemDefinitionsCount = itemCategory.ElementsReadOnly.Count;
            for (int i = 0; i < itemDefinitionsCount; i++) {
                m_Register.ItemDefinitionRegister.Delete(itemCategory.ElementsReadOnly[i]);
            }

            while (itemCategory.ChildrenReadOnly.Count != 0) {
                var child = itemCategory.ChildrenReadOnly[0];
                child.RemoveParent(itemCategory);

                var parentCount = itemCategory.ParentsReadOnly.Count;
                for (int i = 0; i < parentCount; i++) {
                    child.AddParent(itemCategory.ParentsReadOnly[i]);
                }
            }

            while (itemCategory.ParentsReadOnly.Count != 0) {
                var parent = itemCategory.ParentsReadOnly[0];
                itemCategory.RemoveParent(parent);
            }

            Unregister(itemCategory);
        }
    }
}