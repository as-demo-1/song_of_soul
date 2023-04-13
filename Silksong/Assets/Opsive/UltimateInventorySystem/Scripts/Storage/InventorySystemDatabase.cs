/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Storage
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Crafting;
    using Opsive.UltimateInventorySystem.Exchange;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The database is a scriptable component that groups all the Inventory objects in the game.
    /// </summary>
    public class InventorySystemDatabase : ScriptableObject, IInventorySystemDatabase
    {
        [Tooltip("An array of all of the possible Categories.")]
        [SerializeField] protected ItemCategory[] m_ItemCategories;
        [Tooltip("An array of all of the possible ItemDefinition.")]
        [SerializeField] protected ItemDefinition[] m_ItemDefinitions;
        [Tooltip("An array of all of the possible Currencies.")]
        [SerializeField] protected Currency[] m_Currencies;
        [Tooltip("An array of all of the possible Crafting Recipes.")]
        [SerializeField] protected CraftingRecipe[] m_CraftingRecipes;
        [Tooltip("An array of all of the possible Crafting Recipes.")]
        [SerializeField] protected CraftingCategory[] m_CraftingCategories;

        public ItemCategory[] ItemCategories => m_ItemCategories;
        public ItemDefinition[] ItemDefinitions => m_ItemDefinitions;
        public Currency[] Currencies => m_Currencies;
        public CraftingRecipe[] CraftingRecipes => m_CraftingRecipes;
        public CraftingCategory[] CraftingCategories => m_CraftingCategories;

        [System.NonSerialized] private bool m_Initialized;

        /// <summary>
        /// Add an itemDefinition to the array.
        /// </summary>
        /// <param name="itemDefinition">The item definition.</param>
        public virtual void AddItemDefinition(ItemDefinition itemDefinition)
        {
            if (itemDefinition == null) { return; }

            if (RandomID.IsIDEmpty(itemDefinition.ID)) { itemDefinition.ID = RandomID.Generate(); }
            if (RandomID.IsIDEmpty(itemDefinition.DefaultItem.ID)) { itemDefinition.DefaultItem.ID = RandomID.Generate(); }

            for (int i = 0; i < m_ItemDefinitions.Length; i++) {
                if (m_ItemDefinitions[i] == itemDefinition) { return; }
                if (m_ItemDefinitions[i].ID == itemDefinition.ID) { itemDefinition.ID = RandomID.Generate(); }
                if (m_ItemDefinitions[i].DefaultItem.ID == itemDefinition.DefaultItem.ID) { itemDefinition.DefaultItem.ID = RandomID.Generate(); }
            }

            Array.Resize(ref m_ItemDefinitions, m_ItemDefinitions.Length + 1);
            m_ItemDefinitions[m_ItemDefinitions.Length - 1] = itemDefinition;
        }

        /// <summary>
        /// Remove an itemDefinition from the array.
        /// </summary>
        /// <param name="itemDefinition">The item definition.</param>
        public virtual void RemoveItemDefinition(ItemDefinition itemDefinition)
        {
            var list = new List<ItemDefinition>(m_ItemDefinitions);
            list.Remove(itemDefinition);
            m_ItemDefinitions = list.ToArray();
        }

        /// <summary>
        /// Add an itemCategory to the array.
        /// </summary>
        /// <param name="itemCategory">The item category.</param>
        public virtual void AddItemCategory(ItemCategory itemCategory)
        {
            if (itemCategory == null) { return; }

            if (RandomID.IsIDEmpty(itemCategory.ID)) { itemCategory.ID = RandomID.Generate(); }

            for (int i = 0; i < m_ItemCategories.Length; i++) {
                if (m_ItemCategories[i] == itemCategory) { return; }
                if (m_ItemCategories[i].ID == itemCategory.ID) { itemCategory.ID = RandomID.Generate(); }
            }

            Array.Resize(ref m_ItemCategories, m_ItemCategories.Length + 1);
            m_ItemCategories[m_ItemCategories.Length - 1] = itemCategory;
        }

        /// <summary>
        /// Remove an itemCategory from the array.
        /// </summary>
        /// <param name="itemCategory">The item category.</param>
        public virtual void RemoveItemCategory(ItemCategory itemCategory)
        {
            var list = new List<ItemCategory>(m_ItemCategories);
            list.Remove(itemCategory);
            m_ItemCategories = list.ToArray();
        }

        /// <summary>
        /// Add an itemCategory to the array.
        /// </summary>
        /// <param name="currency">The currency.</param>
        public virtual void AddCurrency(Currency currency)
        {
            if (currency == null) { return; }

            if (RandomID.IsIDEmpty(currency.ID)) { currency.ID = RandomID.Generate(); }

            for (int i = 0; i < m_Currencies.Length; i++) {
                if (m_Currencies[i] == currency) { return; }
                if (m_Currencies[i].ID == currency.ID) { currency.ID = RandomID.Generate(); }
            }

            Array.Resize(ref m_Currencies, m_Currencies.Length + 1);
            m_Currencies[m_Currencies.Length - 1] = currency;
        }

        /// <summary>
        /// Remove an itemDefinition from the array.
        /// </summary>
        /// <param name="currency">The currency.</param>
        public virtual void RemoveCurrency(Currency currency)
        {
            var list = new List<Currency>(m_Currencies);
            list.Remove(currency);
            m_Currencies = list.ToArray();
        }

        /// <summary>
        /// Add an Crafting Recipes to the array.
        /// </summary>
        /// <param name="recipe">The recipe.</param>
        public virtual void AddRecipe(CraftingRecipe recipe)
        {
            if (recipe == null) { return; }

            if (RandomID.IsIDEmpty(recipe.ID)) { recipe.ID = RandomID.Generate(); }

            for (int i = 0; i < m_CraftingRecipes.Length; i++) {
                if (m_CraftingRecipes[i] == recipe) { return; }
                if (m_CraftingRecipes[i].ID == recipe.ID) { recipe.ID = RandomID.Generate(); }
            }

            Array.Resize(ref m_CraftingRecipes, m_CraftingRecipes.Length + 1);
            m_CraftingRecipes[m_CraftingRecipes.Length - 1] = recipe;
        }

        /// <summary>
        /// Remove an Crafting Recipes from the array.
        /// </summary>
        /// <param name="recipe">The recipe.</param>
        public virtual void RemoveRecipe(CraftingRecipe recipe)
        {
            var list = new List<CraftingRecipe>(m_CraftingRecipes);
            list.Remove(recipe);
            m_CraftingRecipes = list.ToArray();
        }

        /// <summary>
        /// Add an Crafting Recipes to the array.
        /// </summary>
        /// <param name="category">The crafting category.</param>
        public virtual void AddCraftingCategory(CraftingCategory category)
        {
            if (category == null) { return; }

            if (RandomID.IsIDEmpty(category.ID)) { category.ID = RandomID.Generate(); }

            for (int i = 0; i < m_CraftingCategories.Length; i++) {
                if (m_CraftingCategories[i] == category) { return; }
                if (m_CraftingCategories[i].ID == category.ID) { category.ID = RandomID.Generate(); }
            }

            Array.Resize(ref m_CraftingCategories, m_CraftingCategories.Length + 1);
            m_CraftingCategories[m_CraftingCategories.Length - 1] = category;
        }

        /// <summary>
        /// Remove an Crafting Recipes from the array.
        /// </summary>
        /// <param name="category">The crafting category.</param>
        public virtual void RemoveRecipeCategory(CraftingCategory category)
        {
            var list = new List<CraftingCategory>(m_CraftingCategories);
            list.Remove(category);
            m_CraftingCategories = list.ToArray();
        }

        /// <summary>
        /// Removes any null object referenced by the database.
        /// </summary>
        public virtual void CleanNulls()
        {
            if (m_ItemCategories != null) {
                var categoryList = new List<ItemCategory>(m_ItemCategories);
                categoryList.RemoveAll(x => x == null);
                m_ItemCategories = categoryList.ToArray();
            } else {
                m_ItemCategories = new ItemCategory[0];
            }

            if (m_ItemDefinitions != null) {
                var definitionList = new List<ItemDefinition>(m_ItemDefinitions);
                definitionList.RemoveAll(x => x == null);
                m_ItemDefinitions = definitionList.ToArray();
            } else {
                m_ItemDefinitions = new ItemDefinition[0];
            }

            if (m_Currencies != null) {
                var currencyList = new List<Currency>(m_Currencies);
                currencyList.RemoveAll(x => x == null);
                m_Currencies = currencyList.ToArray();
            } else {
                m_Currencies = new Currency[0];
            }

            if (m_CraftingCategories != null) {
                var recipeCategoryList = new List<CraftingCategory>(m_CraftingCategories);
                recipeCategoryList.RemoveAll(x => x == null);
                m_CraftingCategories = recipeCategoryList.ToArray();
            } else {
                m_CraftingCategories = new CraftingCategory[0];
            }

            if (m_CraftingRecipes != null) {
                var recipeList = new List<CraftingRecipe>(m_CraftingRecipes);
                recipeList.RemoveAll(x => x == null);
                m_CraftingRecipes = recipeList.ToArray();
            } else {
                m_CraftingRecipes = new CraftingRecipe[0];
            }
        }

        /// <summary>
        /// Initialize the database by deserializing the itemDefinitions and categories.
        /// </summary>
        /// <param name="force">Should the database be force initialized?</param>
        public virtual void Initialize(bool force, bool updateAttributes = true)
        {
            if (m_Initialized && !force) {
                return;
            }
            m_Initialized = true;

            if (m_ItemCategories != null) {
                for (int i = 0; i < m_ItemCategories.Length; ++i) {
                    m_ItemCategories[i].Initialize(force);
                }
            } else {
                m_ItemCategories = new ItemCategory[0];
            }
            if (m_ItemDefinitions != null) {
                for (int i = 0; i < m_ItemDefinitions.Length; ++i) {
                    m_ItemDefinitions[i].Initialize(force, updateAttributes);
                    m_ItemDefinitions[i].DefaultItem.Initialize(force, updateAttributes);
                }
            } else {
                m_ItemDefinitions = new ItemDefinition[0];
            }
            if (m_CraftingCategories != null) {
                for (int i = 0; i < m_CraftingCategories.Length; ++i) {
                    m_CraftingCategories[i].Initialize(force);
                }
            } else {
                m_CraftingCategories = new CraftingCategory[0];
            }
            if (m_CraftingRecipes != null) {
                for (int i = 0; i < m_CraftingRecipes.Length; ++i) {
                    m_CraftingRecipes[i].Initialize(force);
                }
            } else {
                m_CraftingRecipes = new CraftingRecipe[0];
            }
            if (m_Currencies != null) {
                for (int i = 0; i < m_Currencies.Length; ++i) {
                    m_Currencies[i].Initialize(force);
                }
            } else {
                m_Currencies = new Currency[0];
            }
        }

        /// <summary>
        /// Check if the database contains the items definition and category
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>True if it is contained in the database.</returns>
        public bool Contains(Item item)
        {
            if (item == null) { return true; }

            if (item.ItemDefinition == null || item.Category == null) { return true; }

            return Contains(item.ItemDefinition) && Contains(item.Category);
        }

        /// <summary>
        /// Check if the database contains the item definition
        /// </summary>
        /// <param name="itemDefinition">The item.</param>
        /// <returns>True if it is contained in the database.</returns>
        public bool Contains(ItemDefinition itemDefinition)
        {
            if (itemDefinition == null) { return true; }

            for (int i = 0; i < m_ItemDefinitions.Length; i++) {
                if (m_ItemDefinitions[i] == itemDefinition) { return true; }
            }

            return false;
        }

        /// <summary>
        /// Check if the database contains the item category
        /// </summary>
        /// <param name="itemCategory">The item.</param>
        /// <returns>True if it is contained in the database.</returns>
        public bool Contains(ItemCategory itemCategory)
        {
            if (itemCategory == null) { return true; }

            for (int i = 0; i < m_ItemCategories.Length; i++) {
                if (m_ItemCategories[i] == itemCategory) { return true; }
            }

            return false;
        }

        /// <summary>
        /// Check if the database contains the currency
        /// </summary>
        /// <param name="currency">The item.</param>
        /// <returns>True if it is contained in the database.</returns>
        public bool Contains(Currency currency)
        {
            if (currency == null) { return true; }

            for (int i = 0; i < m_Currencies.Length; i++) {
                if (m_Currencies[i] == currency) { return true; }
            }

            return false;
        }

        /// <summary>
        /// Check if the database contains the crafting category
        /// </summary>
        /// <param name="craftingCategory">The item.</param>
        /// <returns>True if it is contained in the database.</returns>
        public bool Contains(CraftingCategory craftingCategory)
        {
            if (craftingCategory == null) { return true; }

            for (int i = 0; i < m_CraftingCategories.Length; i++) {
                if (m_CraftingCategories[i] == craftingCategory) { return true; }
            }

            return false;
        }

        /// <summary>
        /// Check if the database contains the crafting recipe
        /// </summary>
        /// <param name="craftingRecipe">The item.</param>
        /// <returns>True if it is contained in the database.</returns>
        public bool Contains(CraftingRecipe craftingRecipe)
        {
            if (craftingRecipe == null) { return true; }

            for (int i = 0; i < m_CraftingRecipes.Length; i++) {
                if (m_CraftingRecipes[i] == craftingRecipe) { return true; }
            }

            return false;
        }
        
        /// <summary>
        /// Check if the database contains the items definition and category
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>True if it is contained in the database.</returns>
        public bool TryGet(string objName, out Item item)
        {
            if (TryGet(objName, out ItemDefinition itemDefinition) == false) {
                item = null;
                return false;
            }

            item = itemDefinition.DefaultItem;
            return true;
        }

        /// <summary>
        /// Check if the database contains the item definition
        /// </summary>
        /// <param name="itemDefinition">The item.</param>
        /// <returns>True if it is contained in the database.</returns>
        public bool TryGet(string objName, out ItemDefinition itemDefinition)
        {
            for (int i = 0; i < m_ItemDefinitions.Length; i++) {
                if (m_ItemDefinitions[i].name == objName) {
                    itemDefinition = m_ItemDefinitions[i];
                    return true;
                }
            }

            itemDefinition = null;
            return false;
        }

        /// <summary>
        /// Check if the database contains the item category
        /// </summary>
        /// <param name="itemCategory">The item.</param>
        /// <returns>True if it is contained in the database.</returns>
        public bool TryGet(string objName, out ItemCategory itemCategory)
        {
            for (int i = 0; i < m_ItemCategories.Length; i++) {
                if (m_ItemCategories[i].name == objName) {
                    itemCategory = m_ItemCategories[i];
                    return true;
                }
            }

            itemCategory = null;
            return false;
        }

        /// <summary>
        /// Check if the database contains the currency
        /// </summary>
        /// <param name="currency">The item.</param>
        /// <returns>True if it is contained in the database.</returns>
        public bool TryGet(string objName, out Currency currency)
        {
            for (int i = 0; i < m_Currencies.Length; i++) {
                if (m_Currencies[i].name == objName) {
                    currency = m_Currencies[i];
                    return true;
                }
            }

            currency = null;
            return false;
        }

        /// <summary>
        /// Check if the database contains the crafting category
        /// </summary>
        /// <param name="craftingCategory">The item.</param>
        /// <returns>True if it is contained in the database.</returns>
        public bool TryGet(string objName, out CraftingCategory craftingCategory)
        {
            for (int i = 0; i < m_CraftingCategories.Length; i++) {
                if (m_CraftingCategories[i].name == objName) {
                    craftingCategory = m_CraftingCategories[i];
                    return true;
                }
            }

            craftingCategory = null;
            return false;
        }

        /// <summary>
        /// Check if the database contains the crafting recipe
        /// </summary>
        /// <param name="craftingRecipe">The item.</param>
        /// <returns>True if it is contained in the database.</returns>
        public bool  TryGet(string objName, out CraftingRecipe craftingRecipe)
        {
            for (int i = 0; i < m_CraftingRecipes.Length; i++) {
                if (m_CraftingRecipes[i].name == objName) {
                    craftingRecipe = m_CraftingRecipes[i];
                    return true;
                }
            }

            craftingRecipe = null;
            return false;
        }

        /// <summary>
        /// Check if the database contains the items definition and category
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>True if it is contained in the database.</returns>
        public Item FindSimilar(Item item)
        {
            if (item == null) { return null; }

            if (item.ItemDefinition == null || item.Category == null) { return item; }

            var itemDefinition = FindSimilar(item.ItemDefinition);

            if (itemDefinition == null) { return null; }

            //Needs to deserialize the attributes
            item.ItemAttributeCollection.Initialize(item, false);

            var attributes = item.GetAttributeList();

            var newItem = Item.Create(itemDefinition, attributes);
            newItem.ID = item.ID;

            if (attributes == null || attributes.Count == 0) { return newItem; }

            //Need to serialize because attributes could have been overriden.
            newItem.Serialize();

            return newItem;
        }

        /// <summary>
        /// Check if the database contains the item definition
        /// </summary>
        /// <param name="itemDefinition">The item.</param>
        /// <returns>True if it is contained in the database.</returns>
        public ItemDefinition FindSimilar(ItemDefinition itemDefinition)
        {
            if (itemDefinition == null) { return null; }

            for (int i = 0; i < m_ItemDefinitions.Length; i++) {
                if (m_ItemDefinitions[i] == itemDefinition
                    || m_ItemDefinitions[i].ID == itemDefinition.ID
                    || m_ItemDefinitions[i].name == itemDefinition.name) { return m_ItemDefinitions[i]; }
            }

            return null;
        }

        /// <summary>
        /// Check if the database contains the item category
        /// </summary>
        /// <param name="itemCategory">The item.</param>
        /// <returns>True if it is contained in the database.</returns>
        public ItemCategory FindSimilar(ItemCategory itemCategory)
        {
            if (itemCategory == null) { return null; }

            for (int i = 0; i < m_ItemCategories.Length; i++) {
                if (m_ItemCategories[i] == itemCategory
                    || m_ItemCategories[i].ID == itemCategory.ID
                    || m_ItemCategories[i].name == itemCategory.name) { return m_ItemCategories[i]; }
            }

            return null;
        }

        /// <summary>
        /// Check if the database contains the currency
        /// </summary>
        /// <param name="currency">The item.</param>
        /// <returns>True if it is contained in the database.</returns>
        public Currency FindSimilar(Currency currency)
        {
            if (currency == null) { return null; }

            for (int i = 0; i < m_Currencies.Length; i++) {
                if (m_Currencies[i] == currency
                    || m_Currencies[i].ID == currency.ID
                    || m_Currencies[i].name == currency.name) { return m_Currencies[i]; }
            }

            return null;
        }

        /// <summary>
        /// Check if the database contains the crafting category
        /// </summary>
        /// <param name="craftingCategory">The item.</param>
        /// <returns>True if it is contained in the database.</returns>
        public CraftingCategory FindSimilar(CraftingCategory craftingCategory)
        {
            if (craftingCategory == null) { return null; }

            for (int i = 0; i < m_CraftingCategories.Length; i++) {
                if (m_CraftingCategories[i] == craftingCategory
                    || m_CraftingCategories[i].ID == craftingCategory.ID
                    || m_CraftingCategories[i].name == craftingCategory.name) { return m_CraftingCategories[i]; }

            }

            return null;
        }

        /// <summary>
        /// Check if the database contains the crafting recipe
        /// </summary>
        /// <param name="craftingRecipe">The item.</param>
        /// <returns>True if it is contained in the database.</returns>
        public CraftingRecipe FindSimilar(CraftingRecipe craftingRecipe)
        {
            if (craftingRecipe == null) { return null; }

            for (int i = 0; i < m_CraftingRecipes.Length; i++) {
                if (m_CraftingRecipes[i] == craftingRecipe
                    || m_CraftingRecipes[i].ID == craftingRecipe.ID
                    || m_CraftingRecipes[i].name == craftingRecipe.name) { return m_CraftingRecipes[i]; }
            }

            return null;
        }

        /// <summary>
        /// Get the object with the name.
        /// </summary>
        /// <param name="objName">The object name.</param>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <returns>The object with that name.</returns>
        internal T Get<T>(string objName) where T : class
        {
            if (typeof(T) == typeof(ItemCategory) || typeof(T).IsSubclassOf(typeof(ItemCategory))) {
                if (TryGet(objName, out ItemCategory itemCategory)) {
                    return itemCategory as T;
                }
            }
            
            if (typeof(T) == typeof(ItemDefinition) || typeof(T).IsSubclassOf(typeof(ItemDefinition))) {
                if (TryGet(objName, out ItemDefinition itemDefinition)) {
                    return itemDefinition as T;
                }
            }
            
            if (typeof(T) == typeof(CraftingCategory) || typeof(T).IsSubclassOf(typeof(CraftingCategory))) {
                if (TryGet(objName, out CraftingCategory craftingCategory)) {
                    return craftingCategory as T;
                }
            }
            
            if (typeof(T) == typeof(CraftingRecipe) || typeof(T).IsSubclassOf(typeof(CraftingRecipe))) {
                if (TryGet(objName, out CraftingRecipe craftingRecipe)) {
                    return craftingRecipe as T;
                }
            }
            
            if (typeof(T) == typeof(Currency) || typeof(T).IsSubclassOf(typeof(Currency))) {
                if (TryGet(objName, out Currency currency)) {
                    return currency as T;
                }
            }
            
            return null;
        }
    }
}

