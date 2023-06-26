/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.Registers
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The register for Item Definitions.
    /// </summary>
    public class ItemDefinitionRegister : InventoryObjectRegister<ItemDefinition>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="register">The inventory system register.</param>
        public ItemDefinitionRegister(InventorySystemRegister register) : base(register)
        {
        }

        /// <summary>
        /// Remove internal removes the object form the dictionaries.
        /// </summary>
        /// <param name="obj">The object.</param>
        protected override void RemoveInternal(ItemDefinition obj)
        {
            base.RemoveInternal(obj);
            m_Register.ItemRegister.ItemsOrganizedByItemDefinitionID.Remove(obj.ID);
        }

        /// <summary>
        /// Register Conditions.
        /// </summary>
        /// <param name="itemDefinition">The item definition.</param>
        /// <returns>True if the object can be registered.</returns>
        public override bool RegisterConditions(ItemDefinition itemDefinition)
        {
            if (base.RegisterConditions(itemDefinition) == false) { return false; }

            if (itemDefinition.Category == null) {
                Debug.LogWarning("Cannot register Item Definition with null category.");
                return false;
            }

            if (m_Register.ItemCategoryRegister.IsRegistered(itemDefinition.Category) == false) {
                Debug.LogWarning("The category of the Item Definition is not registered, please register it first.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Delete an itemCategory.
        /// </summary>
        /// <param name="itemDefinition">The item definition.</param>
        public override void Delete(ItemDefinition itemDefinition)
        {
            while (itemDefinition.ChildrenReadOnly.Count != 0) {
                var child = itemDefinition.ChildrenReadOnly[0];
                child.RemoveParent();
                child.SetParent(itemDefinition.Parent);
            }

            itemDefinition.RemoveParent();

            var itemList = new List<Item>(m_Register.ItemRegister.ItemsWithItemDefinition(itemDefinition));

            foreach (var item in itemList) {
                m_Register.ItemRegister.Delete(item);
            }

            Unregister(itemDefinition);
        }
    }
}