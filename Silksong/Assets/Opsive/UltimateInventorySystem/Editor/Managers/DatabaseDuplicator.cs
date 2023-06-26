/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Crafting;
    using Opsive.UltimateInventorySystem.Editor.Utility;
    using Opsive.UltimateInventorySystem.Exchange;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// A class used to duplicate databases. 
    /// </summary>
    public class DatabaseDuplicator
    {
        public static InventorySystemDatabase DuplicateDatabase(InventorySystemDatabase oldDatabase,
            string newDatabasePath)
        {
            var databaseDuplicator = new DatabaseDuplicator();
            return databaseDuplicator.Duplicate(oldDatabase, newDatabasePath);
        }

        private Dictionary<string, ItemCategory> m_ItemCategoryDictionary;
        private Dictionary<string, ItemDefinition> m_ItemDefinitionDictionary;
        private Dictionary<string, Currency> m_CurrencyDictionary;
        private Dictionary<string, CraftingCategory> m_CraftingCategoryDictionary;
        private Dictionary<string, CraftingRecipe> m_CraftingRecipeDictionary;

        private InventorySystemDatabase m_NewDatabase;
        private HashSet<object> m_HashSet;

        /// <summary>
        /// Duplicate the inventory database at the path provided.
        /// </summary>
        /// <param name="oldDatabase">The database to duplicate.</param>
        /// <param name="newDatabasePath">The path where the database should be saved.</param>
        /// <returns>The new duplicated database.</returns>
        private InventorySystemDatabase Duplicate(InventorySystemDatabase oldDatabase, string newDatabasePath)
        {
            var previousDatabasePath = AssetDatabase.GetAssetPath(oldDatabase);
            newDatabasePath = AssetDatabaseUtility.GetPathForNewDatabase(newDatabasePath, out var newFolderPath);

            Debug.Log($"Duplicating the database to {newDatabasePath}.");

            var databaseResult = AssetDatabase.CopyAsset(previousDatabasePath, newDatabasePath);

            if (databaseResult == false) {
                Debug.LogWarning("The database could not be duplicated.");
                return null;
            }

            ShowProgressBar(0.01f);

            m_NewDatabase =
                (InventorySystemDatabase)AssetDatabase.LoadAssetAtPath(newDatabasePath,
                    typeof(InventorySystemDatabase));

            // Duplicate all objects in the database:
            var maxProgress = 1 + 3f * (m_NewDatabase.ItemCategories.Length + m_NewDatabase.ItemDefinitions.Length) +
                              2f * (m_NewDatabase.Currencies.Length + m_NewDatabase.CraftingCategories.Length + m_NewDatabase.CraftingRecipes.Length);
            var progress = 0f;

            m_ItemCategoryDictionary =
                DuplicateObjectArray<ItemCategory>(m_NewDatabase.ItemCategories, newFolderPath + "/ItemCategories/");

            progress += m_NewDatabase.ItemCategories.Length;
            ShowProgressBar(progress / maxProgress);

            if (m_ItemCategoryDictionary == null) {
                EditorUtility.ClearProgressBar();
                return null;
            }

            m_ItemDefinitionDictionary =
                DuplicateObjectArray<ItemDefinition>(m_NewDatabase.ItemDefinitions,
                    newFolderPath + "/ItemDefinitions/");

            progress += m_NewDatabase.ItemDefinitions.Length;
            ShowProgressBar(progress / maxProgress);

            if (m_ItemDefinitionDictionary == null) {
                EditorUtility.ClearProgressBar();
                return null;
            }

            m_CurrencyDictionary =
                DuplicateObjectArray<Currency>(m_NewDatabase.Currencies, newFolderPath + "/Currencies/");

            progress += m_NewDatabase.Currencies.Length;
            ShowProgressBar(progress / maxProgress);

            if (m_CurrencyDictionary == null) {
                EditorUtility.ClearProgressBar();
                return null;
            }

            m_CraftingCategoryDictionary =
                DuplicateObjectArray<CraftingCategory>(m_NewDatabase.CraftingCategories,
                    newFolderPath + "/CraftingCategories/");

            progress += m_NewDatabase.CraftingCategories.Length;
            ShowProgressBar(progress / maxProgress);

            if (m_CraftingCategoryDictionary == null) {
                EditorUtility.ClearProgressBar();
                return null;
            }

            m_CraftingRecipeDictionary =
                DuplicateObjectArray<CraftingRecipe>(m_NewDatabase.CraftingRecipes,
                    newFolderPath + "/CraftingRecipes/");

            progress += m_NewDatabase.CraftingRecipes.Length;
            ShowProgressBar(progress / maxProgress);

            if (m_CraftingRecipeDictionary == null) {
                EditorUtility.ClearProgressBar();
                return null;
            }



            //Fix all the inner object references:

            // Item Category.
            for (int i = 0; i < m_NewDatabase.ItemCategories.Length; i++) {
                var category = m_NewDatabase.ItemCategories[i];

                category.Deserialize();

                for (int j = 0; j < category.Children.Count; j++) {
                    category.Children[j] = m_ItemCategoryDictionary[category.Children[j].name];
                }

                for (int j = 0; j < category.Parents.Count; j++) {
                    category.Parents[j] = m_ItemCategoryDictionary[category.Parents[j].name];
                }

                for (int j = 0; j < category.Elements.Count; j++) {
                    category.Elements[j] = m_ItemDefinitionDictionary[category.Elements[j].name];
                }

                var categoryAttributesCount = category.GetAttributesCount();
                for (int j = 0; j < categoryAttributesCount; j++) {
                    var attribute = category.GetAttributesAt(j);
                    FixAttributeReferences(attribute);
                }

                category.Serialize();
            }
            progress += m_NewDatabase.ItemCategories.Length;
            ShowProgressBar(progress / maxProgress);

            // Item Definition.
            for (int i = 0; i < m_NewDatabase.ItemDefinitions.Length; i++) {
                var definition = m_NewDatabase.ItemDefinitions[i];

                definition.Deserialize();
                definition.SetCategoryWithoutNotify(m_ItemCategoryDictionary[definition.Category.name]);

                for (int j = 0; j < definition.Children.Count; j++) {
                    definition.Children[j] = m_ItemDefinitionDictionary[definition.Children[j].name];
                }

                if (definition.Parent != null) {
                    definition.SetParentWithoutNotify(m_ItemDefinitionDictionary[definition.Parent.name]);
                }

                var definitionAttributes = definition.GetAttributeList();
                for (int j = 0; j < definitionAttributes.Count; j++) {
                    var attribute = definitionAttributes[j];
                    FixAttributeReferences(attribute);
                }

                var itemAttributes = definition.DefaultItem?.GetAttributeList();
                if (itemAttributes != null) {
                    for (int j = 0; j < itemAttributes.Count; j++) {
                        var attribute = itemAttributes[j];
                        FixAttributeReferences(attribute);
                    }
                }

                definition.Serialize();
            }
            progress += m_NewDatabase.ItemDefinitions.Length;
            ShowProgressBar(progress / maxProgress);

            // Currency.
            for (int i = 0; i < m_NewDatabase.Currencies.Length; i++) {
                var currency = m_NewDatabase.Currencies[i];

                currency.Deserialize();

                for (int j = 0; j < currency.Children.Count; j++) {
                    currency.Children[j] = m_CurrencyDictionary[currency.Children[j].name];
                }

                if (currency.Parent != null) {
                    currency.SetParentWithoutNotify(m_CurrencyDictionary[currency.Parent.name]);
                }

                if (currency.OverflowCurrency != null) {
                    currency.SetOverflowCurrencyWithoutNotify(m_CurrencyDictionary[currency.OverflowCurrency.name]);
                }

                if (currency.FractionCurrency != null) {
                    currency.SetFractionCurrencyWithoutNotify(m_CurrencyDictionary[currency.FractionCurrency.name]);
                }

                currency.Serialize();

            }
            progress += m_NewDatabase.Currencies.Length;
            ShowProgressBar(progress / maxProgress);

            // Crafting Category.
            for (int i = 0; i < m_NewDatabase.CraftingCategories.Length; i++) {
                var category = m_NewDatabase.CraftingCategories[i];

                category.Deserialize();

                for (int j = 0; j < category.Children.Count; j++) {
                    category.Children[j] = m_CraftingCategoryDictionary[category.Children[j].name];
                }

                for (int j = 0; j < category.Parents.Count; j++) {
                    category.Parents[j] = m_CraftingCategoryDictionary[category.Parents[j].name];
                }

                for (int j = 0; j < category.Elements.Count; j++) {
                    category.Elements[j] = m_CraftingRecipeDictionary[category.Elements[j].name];
                }

                category.Serialize();

            }
            progress += m_NewDatabase.CraftingCategories.Length;
            ShowProgressBar(progress / maxProgress);

            // Crafting recipe.
            for (int i = 0; i < m_NewDatabase.CraftingRecipes.Length; i++) {
                var recipe = m_NewDatabase.CraftingRecipes[i];

                recipe.Deserialize();

                recipe.SetCategoryWithoutNotify(m_CraftingCategoryDictionary[recipe.Category.name]);

                m_HashSet = new HashSet<object>();
                var ingredients = (object)recipe.Ingredients;
                SwapInventoryObjectReferences(ref ingredients);

                m_HashSet = new HashSet<object>();
                var defaultOutput = (object)recipe.DefaultOutput;
                SwapInventoryObjectReferences(ref defaultOutput);

                recipe.Serialize();

            }
            progress += m_NewDatabase.CraftingRecipes.Length;
            ShowProgressBar(progress / maxProgress);

            //Re-evaluate Attributes:

            // Item Categories.
            for (int i = 0; i < m_NewDatabase.ItemCategories.Length; i++) {
                var category = m_NewDatabase.ItemCategories[i];

                category.ReevaluateAllAttributes();
            }
            progress += m_NewDatabase.ItemCategories.Length;
            ShowProgressBar(progress / maxProgress);

            // Item Definition.
            for (int i = 0; i < m_NewDatabase.ItemDefinitions.Length; i++) {
                var definition = m_NewDatabase.ItemDefinitions[i];

                definition.ReevaluateAttributes();
                definition.DefaultItem?.ReevaluateAttributes();
            }
            progress += m_NewDatabase.ItemDefinitions.Length;
            ShowProgressBar(progress / maxProgress);

            DirtyAndSaveEverything();
            Debug.Log($"Successfully duplicated the database from {oldDatabase.name} to {m_NewDatabase.name}. The new database should be set in the Inventory System Manager.");

            EditorUtility.ClearProgressBar();
            return m_NewDatabase;
        }

        /// <summary>
        /// Show the progress bar.
        /// </summary>
        /// <param name="percentage">The percentage between 0 and 1.</param>
        private static void ShowProgressBar(float percentage)
        {
            EditorUtility.DisplayProgressBar("Duplicating the database", "This process may take a while.", percentage);
        }

        /// <summary>
        /// Dirty all the new objects and save them.
        /// </summary>
        private void DirtyAndSaveEverything()
        {
            Shared.Editor.Utility.EditorUtility.SetDirty(m_NewDatabase);

            for (int i = 0; i < m_NewDatabase.ItemCategories.Length; i++) {
                Shared.Editor.Utility.EditorUtility.SetDirty(m_NewDatabase.ItemCategories[i]);
            }
            for (int i = 0; i < m_NewDatabase.ItemDefinitions.Length; i++) {
                Shared.Editor.Utility.EditorUtility.SetDirty(m_NewDatabase.ItemDefinitions[i]);
            }
            for (int i = 0; i < m_NewDatabase.Currencies.Length; i++) {
                Shared.Editor.Utility.EditorUtility.SetDirty(m_NewDatabase.Currencies[i]);
            }
            for (int i = 0; i < m_NewDatabase.CraftingCategories.Length; i++) {
                Shared.Editor.Utility.EditorUtility.SetDirty(m_NewDatabase.CraftingCategories[i]);
            }
            for (int i = 0; i < m_NewDatabase.CraftingRecipes.Length; i++) {
                Shared.Editor.Utility.EditorUtility.SetDirty(m_NewDatabase.CraftingRecipes[i]);
            }

            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Fix the attribute references to objects in the database.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        private void FixAttributeReferences(AttributeBase attribute)
        {
            m_HashSet = new HashSet<object>();
            var value = attribute.GetOverrideValueAsObject();
            if (value == null) { return; }

            var type = value.GetType();

            if (!type.IsSubclassOf(typeof(UnityEngine.Object))) {
                SwapInventoryObjectReferences(ref value);
                attribute.SetOverrideValueAsObjectWithoutNotify(value);
                return;
            }

            //If the attribute is an inventory object replace it directly.
            if (type == typeof(ItemCategory) || type.IsSubclassOf(typeof(ItemCategory))) {
                var itemCategory = value as ItemCategory;
                if (itemCategory != null) { attribute.SetOverrideValueAsObjectWithoutNotify(m_ItemCategoryDictionary[itemCategory.name]); }
                return;
            }
            if (type == typeof(ItemDefinition) || type.IsSubclassOf(typeof(ItemDefinition))) {
                var itemDefinition = value as ItemDefinition;
                if (itemDefinition != null) { attribute.SetOverrideValueAsObjectWithoutNotify(m_ItemDefinitionDictionary[itemDefinition.name]); }
                return;
            }
            if (type == typeof(Currency) || type.IsSubclassOf(typeof(Currency))) {
                var currency = value as Currency;
                if (currency != null) { attribute.SetOverrideValueAsObjectWithoutNotify(m_CurrencyDictionary[currency.name]); }
                return;
            }
            if (type == typeof(CraftingCategory) || type.IsSubclassOf(typeof(CraftingCategory))) {
                var craftingCategory = value as CraftingCategory;
                if (craftingCategory != null) { attribute.SetOverrideValueAsObjectWithoutNotify(m_CraftingCategoryDictionary[craftingCategory.name]); }
                return;
            }
            if (type == typeof(CraftingRecipe) || type.IsSubclassOf(typeof(CraftingRecipe))) {
                var craftingRecipe = value as CraftingRecipe;
                if (craftingRecipe != null) { attribute.SetOverrideValueAsObjectWithoutNotify(m_CraftingRecipeDictionary[craftingRecipe.name]); }
                return;
            }
        }

        /// <summary>
        /// Swap the reference from the old database to the new one for objects of random type.
        /// </summary>
        /// <param name="obj">The object which may have a reference to an old database object.</param>
        private void SwapInventoryObjectReferences(ref object obj)
        {
            if (obj == null) { return; }

            if (m_HashSet.Contains(obj)) {
                return;
            }

            m_HashSet.Add(obj);

            var objType = obj.GetType();

            var fields = objType.GetFields(BindingFlags.Default | BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

            for (int i = 0; i < fields.Length; i++) {

                var field = fields[i];

                // Don't show the field if:
                // - The visibility is public but the field is private/protected without the SerializeField attribute.
                // - The field has the HideInInspector attribute.
                if ((field.IsPrivate || field.IsFamily) && TypeUtility.GetAttribute(field, typeof(SerializeField)) == null) {
                    continue;
                }

                if (field.IsNotSerialized) {
                    continue;
                }

                var type = field.FieldType;

                if (type.IsArray) {
                    var array = field.GetValue(obj) as Array;
                    if (array == null) { continue; }

                    var length = array.Length;
                    var list = (IList)array;

                    for (var j = 0; j < list.Count; j++) {
                        var element = list[j];

                        SwapInventoryObjectReferences(ref element);

                        list[j] = element;
                    }

                    continue;
                }

                if (type.IsSubclassOf(typeof(UnityEngine.Object))) {
                    SwapObjectByNewOne(ref obj, type, field);
                    continue;
                }

                var value = field.GetValue(obj);
                if (value != null) { SwapInventoryObjectReferences(ref value); }
            }
        }

        /// <summary>
        /// Do the swap from the old object to the new one.
        /// </summary>
        /// <param name="obj">The object reference.</param>
        /// <param name="type">The object type.</param>
        /// <param name="field">The field.</param>
        private void SwapObjectByNewOne(ref object obj, Type type, FieldInfo field)
        {
            if (type == typeof(ItemCategory) || type.IsSubclassOf(typeof(ItemCategory))) {
                var itemCategory = field.GetValue(obj) as ItemCategory;
                if (itemCategory != null) { field.SetValue(obj, m_ItemCategoryDictionary[itemCategory.name]); }
            }

            if (type == typeof(ItemDefinition) || type.IsSubclassOf(typeof(ItemDefinition))) {
                var itemDefinition = field.GetValue(obj) as ItemDefinition;
                if (itemDefinition != null) { field.SetValue(obj, m_ItemDefinitionDictionary[itemDefinition.name]); }
            }

            if (type == typeof(Currency) || type.IsSubclassOf(typeof(Currency))) {
                var currency = field.GetValue(obj) as Currency;
                if (currency != null) { field.SetValue(obj, m_CurrencyDictionary[currency.name]); }
            }


            if (type == typeof(CraftingCategory) || type.IsSubclassOf(typeof(CraftingCategory))) {
                var craftingCategory = field.GetValue(obj) as CraftingCategory;
                if (craftingCategory != null) { field.SetValue(obj, m_CraftingCategoryDictionary[craftingCategory.name]); }
            }


            if (type == typeof(CraftingRecipe) || type.IsSubclassOf(typeof(CraftingRecipe))) {
                var craftingRecipe = field.GetValue(obj) as CraftingRecipe;
                if (craftingRecipe != null) { field.SetValue(obj, m_CraftingRecipeDictionary[craftingRecipe.name]); }
            }

        }

        /// <summary>
        /// Duplicates the object in the array.
        /// </summary>
        /// <param name="array">The array of object ot duplicate.</param>
        /// <param name="folderPath">The folder where the objects should be added.</param>
        /// <typeparam name="T">The object type.</typeparam>
        /// <returns>A dictionary of name to object.</returns>
        private static Dictionary<string, T> DuplicateObjectArray<T>(T[] array, string folderPath) where T : ScriptableObject, IObjectWithID
        {
            var dictionary = new Dictionary<string, T>();

            for (int i = 0; i < array.Length; i++) {
                var previousPath = AssetDatabase.GetAssetPath(array[i]);

                var newPath = folderPath + array[i].name + ".asset";
                newPath = AssetDatabaseUtility.ValidatePath(newPath); ;

                if (newPath == null) { return null; }

                var result = AssetDatabase.CopyAsset(previousPath, newPath);

                if (result == false) {
                    Debug.LogWarning($"The {typeof(T)} : {array[i].name} in the database failed to duplicate.");
                    return null;
                }

                var obj = (T)AssetDatabase.LoadAssetAtPath(newPath, typeof(T));

                dictionary.Add(obj.name, obj);
                array[i] = obj;
            }

            return dictionary;
        }
    }
}