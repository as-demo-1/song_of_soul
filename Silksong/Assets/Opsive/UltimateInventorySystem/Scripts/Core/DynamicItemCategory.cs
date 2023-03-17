/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core
{
    using Opsive.UltimateInventorySystem.Crafting;
    using Opsive.UltimateInventorySystem.Exchange;
    using System;
    using UnityEngine;

    /// <summary>
    /// Dynamic Item Category Array serializes Item Categories.
    /// Swaps object by an equivalent one if it does not exist in current database.
    /// </summary>
    [Serializable]
    public struct DynamicItemCategoryArray
    {
        [Tooltip("Serialized Dynamic Item Category.")]
        [SerializeField] private DynamicItemCategory[] m_ItemCategories;

        private ItemCategory[] m_CachedItemCategories;
        private bool m_IsInitialized;

        public int Count => m_ItemCategories?.Length ?? 0;

        public DynamicItemCategoryArray(ItemCategory[] value)
        {
            m_ItemCategories = Array.ConvertAll(value, x => (DynamicItemCategory)x); ;
            m_IsInitialized = false;
            m_CachedItemCategories = null;
        }

        public ItemCategory[] Value {
            get {
                if (m_ItemCategories == null) { return null; }

                if (Application.isPlaying == false) {
                    return Array.ConvertAll(m_ItemCategories, x => (ItemCategory)x);
                }

                if (m_IsInitialized) { return m_CachedItemCategories; }

                m_CachedItemCategories = Array.ConvertAll(m_ItemCategories, x => (ItemCategory)x);

                m_IsInitialized = true;
                return m_CachedItemCategories;
            }
        }

        public static implicit operator ItemCategory[](DynamicItemCategoryArray x)
        {
            return x.Value;
        }

        public static implicit operator DynamicItemCategoryArray(ItemCategory[] x)
        {
            return new DynamicItemCategoryArray(x);
        }
    }

    /// <summary>
    /// Dynamic Item Category serialized Item Categories.
    /// Swaps object by an equivalent one if it does not exist in current database.
    /// </summary>
    [Serializable]
    public struct DynamicItemCategory
    {
        [Tooltip("The item category name.")]
        [SerializeField] private string m_Name;
        [Tooltip("The item category.")]
        [SerializeField] private ItemCategory m_ItemCategory;

        private ItemCategory m_ValidItemCategory;
        private bool m_IsInitialized;
        private bool m_HasValue;

        public ItemCategory OriginalSerializedValue => m_ItemCategory;
        public bool SerializedValueIsValid => m_ItemCategory == Value;

        public bool HasValue {
            get {
                if (m_IsInitialized) { return m_HasValue; }

                var value = Value;
                return m_HasValue;
            }
        }

        public DynamicItemCategory(ItemCategory value)
        {
            m_ItemCategory = value;
            m_Name = value != null ? m_ItemCategory.name : null;
            m_IsInitialized = false;
            m_ValidItemCategory = null;
            m_HasValue = false;
        }

        public ItemCategory Value {
            get {
                if (Application.isPlaying == false) { return m_ItemCategory; }

                if (m_IsInitialized) { return m_ValidItemCategory; }

                if (m_ItemCategory != null) {

                    if (InventorySystemManager.ItemCategoryRegister.IsRegistered(m_ItemCategory, false) == false) {
                        m_ValidItemCategory = InventorySystemManager.GetItemCategory(m_ItemCategory.name);
                    } else {
                        m_ValidItemCategory = m_ItemCategory;
                    }

                } else {
                    if (string.IsNullOrWhiteSpace(m_Name) == false) {
                        m_ValidItemCategory = InventorySystemManager.GetItemCategory(m_Name);
                    }
                }

                m_IsInitialized = true;
                m_HasValue = m_ValidItemCategory != null;
                return m_ValidItemCategory;
            }
        }

        public static implicit operator ItemCategory(DynamicItemCategory x)
        {
            return x.Value;
        }

        public static implicit operator DynamicItemCategory(ItemCategory x)
        {
            return new DynamicItemCategory(x);
        }
    }
    
    /// <summary>
    /// Dynamic Item Category Array serializes Item Definitions.
    /// Swaps object by an equivalent one if it does not exist in current database.
    /// </summary>
    [Serializable]
    public struct DynamicItemDefinitionArray
    {
        [Tooltip("Serialized Dynamic Item Definitions.")]
        [SerializeField] private DynamicItemDefinition[] m_ItemDefinitions;

        private ItemDefinition[] m_CachedItemDefinitions;
        private bool m_IsInitialized;

        public int Count => m_ItemDefinitions?.Length ?? 0;

        public DynamicItemDefinitionArray(ItemDefinition[] value)
        {
            m_ItemDefinitions = Array.ConvertAll(value, x => (DynamicItemDefinition)x); ;
            m_IsInitialized = false;
            m_CachedItemDefinitions = null;
        }

        public ItemDefinition[] Value {
            get {
                if (m_ItemDefinitions == null) { return null; }

                if (Application.isPlaying == false) {
                    return Array.ConvertAll(m_ItemDefinitions, x => (ItemDefinition)x);
                }

                if (m_IsInitialized) { return m_CachedItemDefinitions; }

                m_CachedItemDefinitions = Array.ConvertAll(m_ItemDefinitions, x => (ItemDefinition)x);

                m_IsInitialized = true;
                return m_CachedItemDefinitions;
            }
        }

        public static implicit operator ItemDefinition[](DynamicItemDefinitionArray x)
        {
            return x.Value;
        }

        public static implicit operator DynamicItemDefinitionArray(ItemDefinition[] x)
        {
            return new DynamicItemDefinitionArray(x);
        }
    }


    /// <summary>
    /// Dynamic Item Definition Serializes Item Definitions.
    /// Swaps object by an equivalent one if it does not exist in current database.
    /// </summary>
    [Serializable]
    public struct DynamicItemDefinition
    {
        [Tooltip("The item category name.")]
        [SerializeField] private string m_Name;
        [Tooltip("The item category.")]
        [SerializeField] private ItemDefinition m_ItemDefinition;

        private ItemDefinition m_ValidItemDefinition;
        private bool m_IsInitialized;
        private bool m_HasValue;

        public string OriginalSerializedName => m_Name;
        public ItemDefinition OriginalSerializedValue => m_ItemDefinition;

        public bool HasValue {
            get {
                if (m_IsInitialized) { return m_HasValue; }

                var value = Value;
                return m_HasValue;
            }
        }

        public DynamicItemDefinition(ItemDefinition value)
        {
            m_ItemDefinition = value;
            m_Name = value != null ? m_ItemDefinition.name : null;
            m_IsInitialized = false;
            m_ValidItemDefinition = null;
            m_HasValue = false;
        }

        public ItemDefinition Value {
            get {
                if (Application.isPlaying == false) { return m_ItemDefinition; }

                if (m_IsInitialized) { return m_ValidItemDefinition; }

                if (m_ItemDefinition != null) {

                    if (InventorySystemManager.ItemDefinitionRegister.IsRegistered(m_ItemDefinition) == false) {
                        m_ValidItemDefinition = InventorySystemManager.GetItemDefinition(m_ItemDefinition.name);
                    } else {
                        m_ValidItemDefinition = m_ItemDefinition;
                    }

                } else {
                    if (string.IsNullOrWhiteSpace(m_Name) == false) {
                        m_ValidItemDefinition = InventorySystemManager.GetItemDefinition(m_Name);
                    }
                }

                m_IsInitialized = true;
                m_HasValue = m_ValidItemDefinition != null;
                return m_ValidItemDefinition;
            }
        }

        public static implicit operator ItemDefinition(DynamicItemDefinition x)
        {
            return x.Value;
        }

        public static implicit operator DynamicItemDefinition(ItemDefinition x)
        {
            return new DynamicItemDefinition(x);
        }
    }

    /// <summary>
    /// Dynamic Crafting Category Array serializes Crafting Categories.
    /// Swaps object by an equivalent one if it does not exist in current database.
    /// </summary>
    [Serializable]
    public struct DynamicCraftingCategoryArray
    {
        [Tooltip("Serialized Dynamic Crafting Category.")]
        [SerializeField] private DynamicCraftingCategory[] m_CraftingCategories;

        private CraftingCategory[] m_CachedCategories;
        private bool m_IsInitialized;

        public DynamicCraftingCategoryArray(CraftingCategory[] value)
        {
            m_CraftingCategories = Array.ConvertAll(value, x => (DynamicCraftingCategory)x); ;
            m_IsInitialized = false;
            m_CachedCategories = null;
        }

        public CraftingCategory[] Value {
            get {
                if (m_CraftingCategories == null) { return null; }

                if (Application.isPlaying == false) {
                    return Array.ConvertAll(m_CraftingCategories, x => (CraftingCategory)x);
                }

                if (m_IsInitialized) { return m_CachedCategories; }

                m_CachedCategories = Array.ConvertAll(m_CraftingCategories, x => (CraftingCategory)x);

                m_IsInitialized = true;
                return m_CachedCategories;
            }
        }

        public static implicit operator CraftingCategory[](DynamicCraftingCategoryArray x)
        {
            return x.Value;
        }

        public static implicit operator DynamicCraftingCategoryArray(CraftingCategory[] x)
        {
            return new DynamicCraftingCategoryArray(x);
        }
    }

    /// <summary>
    /// Dynamic Crafting Category.
    /// Swaps object by an equivalent one if it does not exist in current database.
    /// </summary>
    [Serializable]
    public struct DynamicCraftingCategory
    {
        [Tooltip("The item category name.")]
        [SerializeField] private string m_Name;
        [Tooltip("The item category.")]
        [SerializeField] private CraftingCategory m_CraftingCategory;

        private CraftingCategory m_ValidCraftingCategory;
        private bool m_IsInitialized;
        private bool m_HasValue;

        public bool HasValue {
            get {
                if (m_IsInitialized) { return m_HasValue; }

                var value = Value;
                return m_HasValue;
            }
        }

        public DynamicCraftingCategory(CraftingCategory value)
        {
            m_CraftingCategory = value;
            m_Name = value != null ? m_CraftingCategory.name : null;
            m_IsInitialized = false;
            m_ValidCraftingCategory = null;
            m_HasValue = false;
        }

        public CraftingCategory Value {
            get {
                if (Application.isPlaying == false) { return m_CraftingCategory; }

                if (m_IsInitialized) { return m_ValidCraftingCategory; }

                if (m_CraftingCategory != null) {
                    if (InventorySystemManager.CraftingCategoryRegister.IsRegistered(m_CraftingCategory) == false) {
                        m_ValidCraftingCategory = InventorySystemManager.GetCraftingCategory(m_CraftingCategory.name);
                    } else {
                        m_ValidCraftingCategory = m_CraftingCategory;
                    }

                } else {
                    if (string.IsNullOrWhiteSpace(m_Name) == false) {
                        m_ValidCraftingCategory = InventorySystemManager.GetCraftingCategory(m_Name);
                    }
                }

                m_IsInitialized = true;
                m_HasValue = m_ValidCraftingCategory != null;
                return m_ValidCraftingCategory;
            }
        }

        public static implicit operator CraftingCategory(DynamicCraftingCategory x)
        {
            return x.Value;
        }

        public static implicit operator DynamicCraftingCategory(CraftingCategory x)
        {
            return new DynamicCraftingCategory(x);
        }
    }

    /// <summary>
    /// Dynamic Crafting Recipe Array.
    /// Swaps object by an equivalent one if it does not exist in current database.
    /// </summary>
    [Serializable]
    public struct DynamicCraftingRecipeArray
    {
        [Tooltip("Serialized Dynamic Crafting Category.")]
        [SerializeField] private DynamicCraftingRecipe[] m_CraftingRecipes;

        private CraftingRecipe[] m_CachedRecipes;
        private bool m_IsInitialized;

        public DynamicCraftingRecipeArray(CraftingRecipe[] value)
        {
            m_CraftingRecipes = Array.ConvertAll(value, x => (DynamicCraftingRecipe)x); ;
            m_IsInitialized = false;
            m_CachedRecipes = null;
        }

        public CraftingRecipe[] Value {
            get {
                if (m_CraftingRecipes == null) { return null; }

                if (Application.isPlaying == false) {
                    return Array.ConvertAll(m_CraftingRecipes, x => (CraftingRecipe)x);
                }

                if (m_IsInitialized) { return m_CachedRecipes; }

                m_CachedRecipes = Array.ConvertAll(m_CraftingRecipes, x => (CraftingRecipe)x);

                m_IsInitialized = true;
                return m_CachedRecipes;
            }
        }

        public static implicit operator CraftingRecipe[](DynamicCraftingRecipeArray x)
        {
            return x.Value;
        }

        public static implicit operator DynamicCraftingRecipeArray(CraftingRecipe[] x)
        {
            return new DynamicCraftingRecipeArray(x);
        }
    }

    /// <summary>
    /// Dynamic Crafting Recipes serializes Crafting Recipe.
    /// Swaps object by an equivalent one if it does not exist in current database.
    /// </summary>
    [Serializable]
    public struct DynamicCraftingRecipe
    {
        [Tooltip("The recipe name.")]
        [SerializeField] private string m_Name;
        [Tooltip("The recipe.")]
        [SerializeField] private CraftingRecipe m_CraftingRecipe;

        private CraftingRecipe m_ValidCraftingRecipe;
        private bool m_IsInitialized;
        private bool m_HasValue;

        public bool HasValue {
            get {
                if (m_IsInitialized) { return m_HasValue; }

                var value = Value;
                return m_HasValue;
            }
        }

        public DynamicCraftingRecipe(CraftingRecipe value)
        {
            m_CraftingRecipe = value;
            m_Name = value != null ? m_CraftingRecipe.name : null;
            m_IsInitialized = false;
            m_ValidCraftingRecipe = null;
            m_HasValue = false;
        }

        public CraftingRecipe Value {
            get {
                if (Application.isPlaying == false) { return m_CraftingRecipe; }

                if (m_IsInitialized) { return m_ValidCraftingRecipe; }

                if (m_CraftingRecipe != null) {
                    if (InventorySystemManager.CraftingRecipeRegister.IsRegistered(m_CraftingRecipe) == false) {
                        m_ValidCraftingRecipe = InventorySystemManager.GetCraftingRecipe(m_CraftingRecipe.name);
                    } else {
                        m_ValidCraftingRecipe = m_CraftingRecipe;
                    }

                } else {
                    if (string.IsNullOrWhiteSpace(m_Name) == false) {
                        m_ValidCraftingRecipe = InventorySystemManager.GetCraftingRecipe(m_Name);
                    }
                }

                m_IsInitialized = true;
                m_HasValue = m_ValidCraftingRecipe != null;
                return m_ValidCraftingRecipe;
            }
        }

        public static implicit operator CraftingRecipe(DynamicCraftingRecipe x)
        {
            return x.Value;
        }

        public static implicit operator DynamicCraftingRecipe(CraftingRecipe x)
        {
            return new DynamicCraftingRecipe(x);
        }
    }

    /// <summary>
    /// Dynamic Currency serializes currency
    /// Swaps object by an equivalent one if it does not exist in current database.
    /// </summary>
    [Serializable]
    public struct DynamicCurrency
    {
        [Tooltip("The currency name.")]
        [SerializeField] private string m_Name;
        [Tooltip("The currency.")]
        [SerializeField] private Currency m_Currency;

        private Currency m_ValidItemCategory;
        private bool m_IsInitialized;
        private bool m_HasValue;

        public bool HasValue {
            get {
                if (m_IsInitialized) { return m_HasValue; }

                var value = Value;
                return m_HasValue;
            }
        }

        public DynamicCurrency(Currency value)
        {
            m_Currency = value;
            m_Name = value != null ? m_Currency.name : null;
            m_IsInitialized = false;
            m_ValidItemCategory = null;
            m_HasValue = false;
        }

        public Currency Value {
            get {
                if (Application.isPlaying == false) { return m_Currency; }

                if (m_IsInitialized) { return m_ValidItemCategory; }

                if (m_Currency != null) {
                    if (InventorySystemManager.CurrencyRegister.IsRegistered(m_Currency, false) == false) {
                        m_ValidItemCategory = InventorySystemManager.GetCurrency(m_Currency.name);
                    } else {
                        m_ValidItemCategory = m_Currency;
                    }

                } else {
                    if (string.IsNullOrWhiteSpace(m_Name) == false) {
                        m_ValidItemCategory = InventorySystemManager.GetCurrency(m_Name);
                    }
                }

                m_IsInitialized = true;
                m_HasValue = m_ValidItemCategory != null;
                return m_ValidItemCategory;
            }
        }

        public static implicit operator Currency(DynamicCurrency x)
        {
            return x.Value;
        }

        public static implicit operator DynamicCurrency(Currency x)
        {
            return new DynamicCurrency(x);
        }
    }
}