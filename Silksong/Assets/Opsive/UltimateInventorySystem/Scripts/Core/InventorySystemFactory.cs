/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core
{
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Crafting;
    using Opsive.UltimateInventorySystem.Exchange;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The inventory system factory lets you define all the factories that will be used for each object.
    /// </summary>
    public class InventorySystemFactory
    {
        protected IInventorySystemManager m_Manager;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="manager">The manager.</param>
        public InventorySystemFactory(IInventorySystemManager manager)
        {
            m_Manager = manager;
        }

        /// <summary>
        /// Creates an ItemCategory.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The itemCategory.</returns>
        public virtual ItemCategory CreateItemCategory(string name)
        {
            return CreateItemCategory(name, true, true, false);
        }

        /// <summary>
        /// Creates an ItemCategory.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="isMutable">Is it mutable.</param>
        /// <returns>The itemCategory.</returns>
        public virtual ItemCategory CreateItemCategory(string name, bool isMutable)
        {
            return CreateItemCategory(name, isMutable, isMutable, false);
        }

        /// <summary>
        /// Creates an ItemCategory.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="isMutable">Is it mutable.</param>
        /// <param name="isUnique">If unique all the items in this category will be unique.</param>
        /// <param name="isAbstract">Is it abstract.</param>
        /// <returns>The itemCategory.</returns>
        public virtual ItemCategory CreateItemCategory(
            string name, bool isMutable, bool isUnique, bool isAbstract = false)
        {
            if (string.IsNullOrWhiteSpace(name)) { name = "MyCategory"; }
            return ItemCategory.Create(name, isMutable, isUnique, isAbstract, m_Manager);
        }

        /// <summary>
        /// Creates a new Item Definition.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="category">The Item Category.</param>
        /// <param name="parentDefinition">The parent Item Definition.</param>
        /// <param name="definitionAttributesOverrides">The override attributes.</param>
        /// <param name="defaultItemAttributesOverrides">The default item override attributes.</param>
        /// <returns>The Item Definition.</returns>
        public virtual ItemDefinition CreateItemDefinition(
            string name, ItemCategory category, ItemDefinition parentDefinition = null,
            IReadOnlyList<AttributeBase> definitionAttributesOverrides = null,
            IReadOnlyList<AttributeBase> defaultItemAttributesOverrides = null)
        {
            return ItemDefinition.Create(name, category, parentDefinition,
                definitionAttributesOverrides, defaultItemAttributesOverrides);
        }

        /// <summary>
        /// Creates an Item.
        /// </summary>
        /// <param name="itemDef">The item Definition.</param>
        /// <param name="overrideAttributes">The override attributes for the item.</param>
        /// <returns>The item.</returns>
        public virtual Item CreateItem(ItemDefinition itemDef, IReadOnlyList<AttributeBase> overrideAttributes = null)
        {
            return Item.Create(itemDef, overrideAttributes);
        }

        /// <summary>
        /// Creates an Item.
        /// </summary>
        /// <param name="itemDef">The item Definition.</param>
        /// <param name="id">The item ID.</param>
        /// <param name="overrideAttributes">The override attributes for the item.</param>
        /// <returns>The item.</returns>
        public virtual Item CreateItem(ItemDefinition itemDef, uint id, IReadOnlyList<AttributeBase> overrideAttributes = null)
        {
            return Item.Create(itemDef, id, overrideAttributes);
        }

        /// <summary>
        /// Creates an Item.
        /// </summary>
        /// <param name="item">The item to copy.</param>
        /// <param name="overrideAttributes">The override attributes for the item.</param>
        /// <returns>The item.</returns>
        public virtual Item CreateItem(Item item, IReadOnlyList<AttributeBase> overrideAttributes = null)
        {
            return Item.Create(item, overrideAttributes);
        }

        /// <summary>
        /// Creates an Item.
        /// </summary>
        /// <param name="item">The item to copy.</param>
        /// <param name="id">The item ID.</param>
        /// <param name="overrideAttributes">The override attributes for the item.</param>
        /// <returns>The item.</returns>
        public virtual Item CreateItem(Item item, uint id, IReadOnlyList<AttributeBase> overrideAttributes = null)
        {
            return Item.Create(item, id, overrideAttributes);
        }

        /// <summary>
        /// Creates a crafting category.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="isAbstract">Is the category abstract.</param>
        /// <param name="recipeType">The recipe type for this category.</param>
        /// <returns>The crafting category.</returns>
        public virtual CraftingCategory CreateCraftingCategory(
            string name, bool isAbstract = false, Type recipeType = null)
        {
            return CraftingCategory.Create(name, isAbstract, recipeType, m_Manager);
        }

        /// <summary>
        /// Creates a crafting recipe.
        /// </summary>
        /// <param name="name">The names.</param>
        /// <param name="category">The crafting category.</param>
        /// <param name="otherRecipe">The recipe to copy.</param>
        /// <returns>The crafting recipe.</returns>
        public virtual CraftingRecipe CreateCraftingRecipe(string name, CraftingCategory category,
            CraftingRecipe otherRecipe = null)
        {
            if (otherRecipe != null) {
                return CraftingRecipe.CreateFrom(name, category, otherRecipe);
            }
            return CraftingRecipe.Create(name, category);
        }

        /// <summary>
        /// Creates a currency.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="exchangeRateToParent">The exchange rate with the parent currency.</param>
        /// <returns>The Currency.</returns>
        public virtual Currency CreateCurrency(
            string name, Currency parent = null, double exchangeRateToParent = 1)
        {
            return Currency.Create(name, parent, exchangeRateToParent, m_Manager);
        }
    }
}