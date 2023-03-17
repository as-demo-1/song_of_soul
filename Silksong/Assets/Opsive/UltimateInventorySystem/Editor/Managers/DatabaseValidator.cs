/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Crafting;
    using Opsive.UltimateInventorySystem.Editor.Utility;
    using Opsive.UltimateInventorySystem.Exchange;
    using Opsive.UltimateInventorySystem.Storage;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// A class used to check if a database is valid, and can automatically fix simple issues. 
    /// </summary>
    public static class DatabaseValidator
    {
        public static readonly string s_UncategorizedCategoryName = "Uncategorized";
        private static InventorySystemDatabase s_Database;
        public static InventorySystemDatabase Database => s_Database;

        /// <summary>
        /// The uncategorized item category is required for the system to work correctly.
        /// </summary>
        public static ItemCategory UncategorizedItemCategory {
            get {
                if (s_Database == null) { return null; }
                for (int i = 0; i < s_Database.ItemCategories.Length; i++) {
                    if (s_Database.ItemCategories[i] == null) { continue; }
                    if (s_Database.ItemCategories[i].name != s_UncategorizedCategoryName) {
                        continue;
                    }

                    return s_Database.ItemCategories[i];
                }

                return ItemCategoryEditorUtility.AddItemCategory(s_UncategorizedCategoryName, s_Database, GetDatabaseDirectory(s_Database));
            }
        }

        /// <summary>
        /// The uncategorized crafting category is required for the system to work correctly.
        /// </summary>
        public static CraftingCategory UncategorizedCraftingCategory {
            get {
                if (s_Database == null) { return null; }
                for (int i = 0; i < s_Database.CraftingCategories.Length; i++) {
                    if (s_Database.CraftingCategories[i] == null) { continue; }
                    if (s_Database.CraftingCategories[i].name != s_UncategorizedCategoryName) {
                        continue;
                    }

                    return s_Database.CraftingCategories[i];
                }

                return CraftingCategoryEditorUtility.AddCraftingCategory(s_UncategorizedCategoryName, s_Database, GetDatabaseDirectory(s_Database));
            }
        }

        /// <summary>
        /// Returns the location of the database directory.
        /// </summary>
        /// <returns>The location of the database directory.</returns>
        public static string GetDatabaseDirectory(InventorySystemDatabase database)
        {
            if (database == null) {
                return string.Empty;
            }
            var databasePath = AssetDatabase.GetAssetPath(database);
            return System.IO.Path.GetDirectoryName(databasePath);
        }

        /// <summary>
        /// Checks if the database provided is valid and prints out any problems.
        /// </summary>
        /// <param name="database">The database that needs checking.</param>
        /// <param name="autoFix">If true the validator will try to auto fix the problems.</param>
        /// <returns>True if the database is valid.</returns>
        public static bool CheckIfValid(InventorySystemDatabase database, bool autoFix)
        {
            s_Database = database;

            bool isValid = true;

            database.CleanNulls();

            if (UncategorizedItemCategory == null || UncategorizedCraftingCategory == null) {
                Debug.LogError("Uncategorized categories are null.");
            }

            database.Initialize(true, false);

            if (autoFix) {
                AssignUncategorizedCategory(database);
            }

            for (int i = 0; i < database.ItemCategories.Length; i++) {
                if (Validate(database.ItemCategories[i], autoFix)) { continue; }

                isValid = false;
            }

            for (int i = 0; i < database.ItemDefinitions.Length; i++) {
                if (Validate(database.ItemDefinitions[i], autoFix)) { continue; }

                isValid = false;
            }

            for (int i = 0; i < database.CraftingCategories.Length; i++) {
                if (Validate(database.CraftingCategories[i], autoFix)) { continue; }

                isValid = false;
            }

            for (int i = 0; i < database.CraftingRecipes.Length; i++) {
                if (Validate(database.CraftingRecipes[i], autoFix)) { continue; }

                isValid = false;
            }

            for (int i = 0; i < database.Currencies.Length; i++) {
                if (Validate(database.Currencies[i], autoFix)) { continue; }

                isValid = false;
            }

            if (isValid == false && Application.isPlaying == false) {
                Shared.Editor.Utility.EditorUtility.SetDirty(database);
            }

            if (isValid) {
                for (int i = 0; i < database.ItemCategories.Length; i++) {
                    database.ItemCategories[i]?.UpdateCategoryAttributes();
                    database.ItemCategories[i]?.UpdateDefinitionAttributes();
                    database.ItemCategories[i]?.UpdateItemAttributes();
                }

                for (int i = 0; i < database.ItemDefinitions.Length; i++) {
                    database.ItemDefinitions[i]?.UpdateAttributes();
                    database.ItemDefinitions[i]?.DefaultItem?.UpdateAttributes();
                }
            }

            return isValid;
        }

        /// <summary>
        /// Assign the Uncategorized ItemCategory to itemDefinitions that have a null ItemCategory.
        /// </summary>
        private static void AssignUncategorizedCategory(InventorySystemDatabase database)
        {
            var itemDefinitions = database.ItemDefinitions;
            for (int i = 0; i < itemDefinitions.Length; i++) {
                var itemCategory = itemDefinitions[i].Category;
                if (itemCategory == null || (itemCategory.name == s_UncategorizedCategoryName && itemCategory != UncategorizedItemCategory)) {
                    ItemDefinitionEditorUtility.SetItemDefinitionCategory(itemDefinitions[i], UncategorizedItemCategory, Relation.Family);
                }
            }
            var recipes = database.CraftingRecipes;
            for (int i = 0; i < recipes.Length; i++) {
                var craftingCategory = recipes[i].Category;
                if (craftingCategory == null || (craftingCategory.name == s_UncategorizedCategoryName && craftingCategory != UncategorizedCraftingCategory)) {
                    CraftingRecipeEditorUtility.SetCraftingRecipeCategory(recipes[i], UncategorizedCraftingCategory);
                }
            }
        }

        /// <summary>
        /// Validate the ItemCategory
        /// </summary>
        /// <param name="itemCategory">The ItemCategory.</param>
        /// <param name="autoFix">Automatically fix the database?</param>
        /// <returns>True if valid.</returns>
        private static bool Validate(ItemCategory itemCategory, bool autoFix)
        {
            var isValid = true;

            if (itemCategory == null) {
                Debug.LogWarning($"Item Category is null.");
                return false;
            }

            if (RandomID.IsIDEmpty(itemCategory.ID)) {
                if (autoFix) {
                    AssignNewUniqueID(itemCategory, s_Database.ItemCategories);
                } else {
                    Debug.LogWarning($"Item Category {itemCategory} has an empty ID.");
                    isValid = false;
                }
            }

            var attributeNameList = new List<string>();

            var allAttributesCount = itemCategory.GetAttributesCount();
            for (int i = 0; i < allAttributesCount; i++) {
                var attribute = itemCategory.GetAttributesAt(i);

                if (attribute == null) {
                    Debug.LogWarning($"Item Category {itemCategory} has a null attribute.");
                    isValid = false;
                    continue;
                }

                if (attributeNameList.Contains(attribute.Name)) {
                    Debug.LogWarning($"Item Category {itemCategory} has two definition with the same name: {attribute.Name}.");
                    isValid = false;
                    continue;
                }
                attributeNameList.Add(attribute.Name);
            }

            //Validate Parents.
            if (autoFix) {
                var amountRemoved = itemCategory.Parents.RemoveAll(x => x == null || x.IsInitialized == false);
                if (amountRemoved != 0) {
                    ItemCategoryEditorUtility.SetItemCategoryDirty(itemCategory, true);
                }

                for (int i = 0; i < itemCategory.Parents.Count; i++) {
                    var parent = itemCategory.Parents[i];
                    if (parent.Children.Contains(itemCategory)) { continue; }

                    itemCategory.RemoveParentNoNotify(parent);
                    itemCategory.AddParentNoNotify(parent);
                    ItemCategoryEditorUtility.SetItemCategoryDirty(itemCategory, true);
                    ItemCategoryEditorUtility.SetItemCategoryDirty(parent, true);
                }
            }

            for (int i = 0; i < itemCategory.Parents.Count; i++) {
                var parent = itemCategory.Parents[i];
                if (parent == null) {

                    Debug.LogWarning($"Item Category {itemCategory} has a null parent.");
                    isValid = false;
                    continue;
                }

                if (parent.IsInitialized == false) {

                    Debug.LogWarning($"Item Category {itemCategory} has a parent {parent} that is not part of the database.");
                    isValid = false;
                    continue;
                }

                if (parent.Children.Contains(itemCategory) == false) {
                    Debug.LogWarning(
                        $"Item Category {itemCategory} has a parent {parent} which does not reference it as a child.");
                    isValid = false;
                }

                var parentAttributeCount = parent.GetCategoryAttributeCount();
                for (int j = 0; j < parentAttributeCount; j++) {
                    var parentAttribute = parent.GetCategoryAttributeAt(j);
                    if (itemCategory.TryGetCategoryAttribute(parentAttribute.Name, out var itemCategoryAttribute) == false) {
                        Debug.LogWarning($"Item Category {itemCategory} is missing a Category Attribute named ${parentAttribute.Name}.");
                        isValid = false;
                    } else if (itemCategoryAttribute?.GetValueType() != parentAttribute?.GetValueType()) {
                        Debug.LogWarning($"Item Category {itemCategory} has a category attribute ${parentAttribute.Name} with a mismatched type {itemCategoryAttribute?.GetValueType()} instead of {parentAttribute?.GetValueType()}.");
                        isValid = false;
                    }
                }

                var parentItemDefinitionAttributes = parent.GetDefinitionAttributeList();
                for (int j = 0; j < parentItemDefinitionAttributes.Count; j++) {
                    var parentAttribute = parentItemDefinitionAttributes[j];
                    if (itemCategory.TryGetDefinitionAttribute(parentAttribute.Name, out var itemDefinitionAttribute) == false) {
                        Debug.LogWarning($"Item Category {itemCategory} is missing a Item Definition Attribute named ${parentAttribute.Name}.");
                        isValid = false;
                    } else if (itemDefinitionAttribute?.GetValueType() != parentAttribute?.GetValueType()) {
                        Debug.LogWarning($"Item Category {itemCategory} has a definition attribute ${parentAttribute.Name} with a mismatched type {itemDefinitionAttribute?.GetValueType()} instead of {parentAttribute?.GetValueType()}.");
                        isValid = false;
                    }
                }

                var parentItemAttributes = parent.GetItemAttributeList();
                for (int j = 0; j < parentItemAttributes.Count; j++) {
                    var parentAttribute = parentItemAttributes[j];
                    if (itemCategory.TryGetItemAttribute(parentAttribute.Name, out var itemAttribute) == false) {
                        Debug.LogWarning($"Item Category {itemCategory} is missing a Item Attribute named ${parentAttribute.Name}.");
                        isValid = false;
                    } else if (itemAttribute?.GetValueType() != parentAttribute?.GetValueType()) {
                        Debug.LogWarning($"Item Category {itemCategory} has an item attribute ${parentAttribute.Name} with a mismatched type {itemAttribute?.GetValueType()} instead of {parentAttribute?.GetValueType()}.");
                        isValid = false;
                    }
                }

            }

            //Validate Children.
            if (autoFix) {
                var amountRemoved = itemCategory.Children.RemoveAll(x => x == null || x.IsInitialized == false);
                if (amountRemoved != 0) {
                    ItemCategoryEditorUtility.SetItemCategoryDirty(itemCategory, true);
                }

                for (int i = itemCategory.Children.Count - 1; i >= 0; i--) {
                    var child = itemCategory.Children[i];
                    if (child.Parents.Contains(itemCategory)) { continue; }

                    child.RemoveParentNoNotify(itemCategory);

                    if (itemCategory.InherentlyContains(child)) {
                        // If the category is inherently contained then there is no reason for the parent to be added.
                        itemCategory.Children.RemoveAt(i);
                    } else {
                        child.AddParentNoNotify(itemCategory);
                    }

                    ItemCategoryEditorUtility.SetItemCategoryDirty(itemCategory, true);
                    ItemCategoryEditorUtility.SetItemCategoryDirty(child, true);
                }
            }

            for (int i = 0; i < itemCategory.Children.Count; i++) {
                var child = itemCategory.Children[i];
                if (child == null) {
                    Debug.LogWarning($"Item Category {itemCategory} has a null child.");
                    isValid = false;
                    continue;
                }

                if (child.IsInitialized == false) {
                    Debug.LogWarning($"Item Category {itemCategory} has a child {child} which is not part of the database.");
                    isValid = false;
                    continue;
                }

                if (child.Parents.Contains(itemCategory) == false) {
                    Debug.LogWarning(
                        $"Item Category {itemCategory} has a child {child} which does not reference it as a parent.");
                    isValid = false;
                    continue;
                }

                var itemCategoryAttributeCount = itemCategory.GetCategoryAttributeCount();
                for (int j = 0; j < itemCategoryAttributeCount; j++) {
                    var parentAttribute = itemCategory.GetCategoryAttributeAt(j);
                    if (child.TryGetCategoryAttribute(parentAttribute.Name, out var itemCategoryAttribute) == false) {
                        Debug.LogWarning($"Item Category {child} is missing a Category Attribute named ${parentAttribute.Name}.");
                        isValid = false;
                    } else if (itemCategoryAttribute?.GetValueType() != parentAttribute?.GetValueType()) {
                        Debug.LogWarning($"Item Category {itemCategory} has a category attribute ${parentAttribute.Name} with a mismatched type {itemCategoryAttribute?.GetValueType()} instead of {parentAttribute?.GetValueType()}.");
                        isValid = false;
                    }
                }

                var parentItemDefinitionAttributes = itemCategory.GetDefinitionAttributeList();
                for (int j = 0; j < parentItemDefinitionAttributes.Count; j++) {
                    var parentAttribute = parentItemDefinitionAttributes[j];
                    if (child.TryGetDefinitionAttribute(parentAttribute.Name, out var itemDefinitionAttribute) == false) {
                        Debug.LogWarning($"Item Category {child} is missing an Item Definition Attribute named ${parentAttribute.Name}.");
                        isValid = false;
                    } else if (itemDefinitionAttribute?.GetValueType() != parentAttribute?.GetValueType()) {
                        Debug.LogWarning($"Item Category {itemCategory} has a definition attribute ${parentAttribute.Name} with a mismatched type {itemDefinitionAttribute?.GetValueType()} instead of {parentAttribute?.GetValueType()}.");
                        isValid = false;
                    }
                }

                var parentItemAttributes = itemCategory.GetItemAttributeList();
                for (int j = 0; j < parentItemAttributes.Count; j++) {
                    var parentAttribute = parentItemAttributes[j];
                    if (child.TryGetItemAttribute(parentAttribute.Name, out var itemAttribute) == false) {
                        Debug.LogWarning($"Item Category {child} is missing an Item Attribute named ${parentAttribute.Name}.");
                        isValid = false;
                    } else if (itemAttribute?.GetValueType() != parentAttribute?.GetValueType()) {
                        Debug.LogWarning($"Item Category {itemCategory} has an item attribute ${parentAttribute.Name} with a mismatched type {itemAttribute?.GetValueType()} instead of {parentAttribute?.GetValueType()}.");
                        isValid = false;
                    }
                }
            }

            //Validate ItemDefinitions.
            if (autoFix) {
                var amountRemoved = itemCategory.Elements.RemoveAll(x => x == null || x.IsInitialized == false || x.Category != itemCategory);
                if (amountRemoved != 0) {
                    ItemCategoryEditorUtility.SetItemCategoryDirty(itemCategory, true);
                }
            }

            if (itemCategory.IsAbstract && itemCategory.Elements.Count > 0) {

                if (autoFix) {
                    for (int i = itemCategory.Elements.Count - 1; i >= 0; i--) {
                        if (itemCategory.Elements.Count <= i) { continue; }
                        ItemDefinitionEditorUtility.SetItemDefinitionCategory(itemCategory.Elements[i], UncategorizedItemCategory, Relation.Family);
                    }
                }

                Debug.LogWarning($"Item Category {itemCategory} is abstract, yet it directly contains Item Definitions: {itemCategory.Elements}.");
                isValid = false;
            }

            for (int i = 0; i < itemCategory.Elements.Count; i++) {
                var itemDefinition = itemCategory.Elements[i];
                if (itemDefinition == null) {
                    Debug.LogWarning($"Item Category {itemCategory} has a null Item Definition.");
                    isValid = false;
                    continue;
                }

                if (itemDefinition.IsInitialized == false) {
                    Debug.LogWarning($"Item Category {itemCategory} has an Item Definition {itemDefinition} that is not part of the database.");
                    isValid = false;
                    continue;
                }

                if (itemDefinition.Category != itemCategory) {
                    Debug.LogWarning(
                        $"Item Category {itemCategory} has an Item Definition {itemDefinition} " +
                        $"which does not reference it as a category.");
                    isValid = false;
                    continue;
                }

                if (autoFix) {
                    itemDefinition.UpdateAttributes();
                }

                var parentItemDefinitionAttributes = itemCategory.GetDefinitionAttributeList();
                for (int j = 0; j < parentItemDefinitionAttributes.Count; j++) {
                    var parentAttribute = parentItemDefinitionAttributes[j];
                    if (itemDefinition.TryGetAttribute(parentAttribute.Name, out var itemDefinitionAttribute) ==
                        false) {
                        Debug.LogWarning($"ItemDefinition {itemDefinition} is missing an ItemDefinition Attribute named '{parentAttribute.Name}'.");
                        isValid = false;
                    } else if (itemDefinitionAttribute?.GetValueType() != parentAttribute?.GetValueType()) {
                        Debug.LogWarning($"ItemDefinition {itemDefinition} has an definition attribute '{parentAttribute.Name}' with the wrong type {itemDefinitionAttribute?.GetValueType()} instead of {parentAttribute?.GetValueType()}.");
                        isValid = false;
                    }
                }

                var parentItemAttributes = itemCategory.GetItemAttributeList();
                for (int j = 0; j < parentItemAttributes.Count; j++) {
                    var parentAttribute = parentItemAttributes[j];

                    if (itemDefinition.DefaultItem.TryGetAttribute(parentAttribute.Name, out var itemAttribute) ==
                        false) {
                        Debug.LogWarning($"ItemDefinition {itemDefinition} is missing an Item Attribute named '{parentAttribute.Name}'.");
                        isValid = false;
                    } else if (itemAttribute?.GetValueType() != parentAttribute?.GetValueType()) {
                        Debug.LogWarning($"ItemDefinition {itemDefinition} has an item attribute '{parentAttribute.Name}' with the wrong type {itemAttribute?.GetValueType()} instead of {parentAttribute?.GetValueType()}.");
                        isValid = false;
                    }
                }
            }

            return isValid;
        }

        /// <summary>
        /// Validates the ItemDefinition.
        /// </summary>
        /// <param name="itemDefinition">The itemDefinition.</param>
        /// <param name="autoFix">Automatically fix the itemDefinitions?</param>
        /// <returns>True if valid.</returns>
        private static bool Validate(ItemDefinition itemDefinition, bool autoFix)
        {
            var isValid = true;

            if (itemDefinition == null) {
                Debug.LogWarning($"Item Definition is null.");
                return false;
            }

            if (RandomID.IsIDEmpty(itemDefinition.ID)) {
                if (autoFix) {
                    AssignNewUniqueID(itemDefinition, s_Database.ItemDefinitions);
                } else {
                    Debug.LogWarning($"Item Definition {itemDefinition} has an empty ID.");
                    isValid = false;
                }
            }

            if (itemDefinition.Category == null) {
                Debug.LogWarning($"Item Definition {itemDefinition} has a null Item Category.");
                return false;
            }

            if (itemDefinition.Category.IsInitialized == false) {

                if (autoFix) {
                    ItemDefinitionEditorUtility.SetItemDefinitionCategory(itemDefinition, UncategorizedItemCategory, Relation.Family);
                } else {
                    Debug.LogWarning($"Item Definition {itemDefinition} has an Item Category {itemDefinition.Category} that is not part of the database.");
                    return false;
                }
            }

            if (itemDefinition.Category.DirectlyContains(itemDefinition) == false) {

                if (autoFix) {
                    ItemDefinitionEditorUtility.SetItemDefinitionCategory(itemDefinition, itemDefinition.Category, Relation.Family);
                }

                Debug.LogWarning($"Item Definition {itemDefinition} has Item Category {itemDefinition.Category} " +
                                 $"but the category does not reference it as a direct Item Definition.");
                return false;
            }

            if (itemDefinition.Category.IsAbstract) {
                Debug.LogWarning($"Item Definition {itemDefinition} has Item Category {itemDefinition.Category} " +
                                 $"that is abstract, abstract categories should not have Item Definitions.");
                return false;
            }

            var parent = itemDefinition.Parent;
            if (parent != null && parent.IsInitialized) {
                var foundMatch = false;
                for (int i = parent.Children.Count - 1; i >= 0; i--) {
                    var sibling = parent.Children[i];
                    if (sibling == itemDefinition) { foundMatch = true; }
                }

                if (!foundMatch) {
                    if (autoFix) {
                        itemDefinition.Parent.Children.Add(itemDefinition);
                        ItemDefinitionEditorUtility.SetItemDefinitionDirty(itemDefinition.Parent, true);
                    } else {
                        Debug.LogWarning($"Item Definition {itemDefinition} has a parent {parent}" +
                                         $", which does not reference it as a child.");
                        isValid = false;
                    }
                }
            } else if (autoFix && parent != null) {
                ItemDefinitionEditorUtility.SetItemDefinitionParent(itemDefinition, null);
            }

            if (autoFix) {
                var amountRemoved = itemDefinition.Children.RemoveAll(x => x == null || x.IsInitialized == false);
                if (amountRemoved != 0) {
                    ItemDefinitionEditorUtility.SetItemDefinitionDirty(itemDefinition, true);
                }
            }

            for (int i = itemDefinition.Children.Count - 1; i >= 0; i--) {
                var child = itemDefinition.Children[i];

                if (autoFix) {
                    if (child == null || child.IsInitialized == false || child.Category != itemDefinition.Category || child.Parent == null) {
                        //Remove child connection
                        itemDefinition.Children.RemoveAt(i);
                        ItemDefinitionEditorUtility.SetItemDefinitionDirty(itemDefinition, true);
                    } else if (child.Parent != itemDefinition) {
                        //Reconnect child
                        child.SetParent(itemDefinition);
                        ItemDefinitionEditorUtility.SetItemDefinitionDirty(child, true);
                    }
                    continue;
                }

                if (child == null) {
                    Debug.LogWarning(
                        $"Item Definition {itemDefinition} has a null child.");
                    isValid = false;
                    continue;
                }

                if (child.IsInitialized == false) {
                    Debug.LogWarning(
                        $"Item Definition {itemDefinition} has a child {child} that is not part of the database.");
                    isValid = false;
                    continue;
                }

                if (child.Parent != itemDefinition) {
                    Debug.LogWarning(
                        $"Item Definition {itemDefinition} has a child {child} which does not reference it as a parent.");
                    isValid = false;
                }

                if (child.Category != itemDefinition.Category) {
                    Debug.LogWarning(
                        $"Item Definition {itemDefinition} has a child {child} " +
                        $"which does share the same category: {itemDefinition.Category} != {child.Category}.");
                    isValid = false;
                }
            }

            return isValid;
        }

        /// <summary>
        /// Validate the recipeCategory.
        /// </summary>
        /// <param name="category">The recipeCategory.</param>
        /// <param name="autoFix">Automatically fix the categories?</param>
        /// <returns>True if valid.</returns>
        private static bool Validate(CraftingCategory category, bool autoFix)
        {
            var isValid = true;

            if (category == null) {
                Debug.LogWarning($"Crafting Category is null.");
                return false;
            }

            if (RandomID.IsIDEmpty(category.ID)) {
                if (autoFix) {
                    AssignNewUniqueID(category, s_Database.CraftingCategories);
                } else {
                    Debug.LogWarning($"Crafting Category {category} has an empty ID.");
                    isValid = false;
                }
            }

            if (category.RecipeType == null || category.RecipeType.ToString() == "NULL") {

                if (autoFix) {
                    CraftingCategoryEditorUtility.SetRecipeType(category, typeof(CraftingRecipe));
                } else {
                    Debug.LogWarning($"Crafting Category {category} has a null Recipe Type.");
                    isValid = false;
                }
            }

            // Validate Parents.
            if (autoFix) {
                var amountRemoved = category.Parents.RemoveAll(x => x == null || x.IsInitialized == false);
                if (amountRemoved != 0) {
                    CraftingCategoryEditorUtility.SetCraftingCategoryDirty(category, true);
                }

                for (int i = 0; i < category.Parents.Count; i++) {
                    var parent = category.Parents[i];
                    if (parent.Children.Contains(category)) { continue; }

                    category.RemoveParent(parent);
                    category.AddParent(parent);
                    CraftingCategoryEditorUtility.SetCraftingCategoryDirty(category, true);
                    CraftingCategoryEditorUtility.SetCraftingCategoryDirty(parent, true);
                }
            }

            for (int i = 0; i < category.Parents.Count; i++) {
                var parent = category.Parents[i];
                if (parent == null) {
                    Debug.LogWarning($"Crafting Category {category} has a null parent.");
                    isValid = false;
                    continue;
                }

                if (parent.IsInitialized == false) {
                    Debug.LogWarning($"Crafting Category {category} has a parent {parent} that is not part of the database.");
                    isValid = false;
                    continue;
                }

                if (parent.Children.Contains(category) == false) {
                    Debug.LogWarning(
                        $"Crafting Category {category} has a parent {parent} which does not reference it as a child.");
                    isValid = false;
                }
            }

            //Validate Children.
            if (autoFix) {
                var amountRemoved = category.Children.RemoveAll(x => x == null || x.IsInitialized == false);
                if (amountRemoved != 0) {
                    CraftingCategoryEditorUtility.SetCraftingCategoryDirty(category, true);
                }

                for (int i = 0; i < category.Children.Count; i++) {
                    var child = category.Children[i];
                    if (child.Parents.Contains(category)) { continue; }

                    child.RemoveParent(category);
                    child.AddParent(category);
                    CraftingCategoryEditorUtility.SetCraftingCategoryDirty(category, true);
                    CraftingCategoryEditorUtility.SetCraftingCategoryDirty(child, true);
                }
            }

            for (int i = 0; i < category.Children.Count; i++) {
                var child = category.Children[i];
                if (child == null) {
                    Debug.LogWarning($"Crafting Category {category} has a null child.");
                    isValid = false;
                    continue;
                }

                if (child.IsInitialized == false) {
                    Debug.LogWarning($"Crafting Category {category} has a child {child} that is not part of the database.");
                    isValid = false;
                    continue;
                }

                if (child.Parents.Contains(category) == false) {
                    Debug.LogWarning(
                        $"Crafting Category {category} has a child {child} which does not reference it as a parent.");
                    isValid = false;
                    continue;
                }
            }

            //Validate Recipes.
            if (autoFix) {
                var amountRemoved = category.Elements.RemoveAll(x => x == null || x.IsInitialized == false || x.Category != category);
                if (amountRemoved != 0) {
                    CraftingCategoryEditorUtility.SetCraftingCategoryDirty(category, true);
                }
            }

            if (category.IsAbstract && category.Elements.Count > 0) {

                if (autoFix) {
                    for (int i = category.Elements.Count - 1; i >= 0; i--) {
                        if (category.Elements.Count <= i) { continue; }
                        CraftingRecipeEditorUtility.SetCraftingRecipeCategory(category.Elements[i], UncategorizedCraftingCategory);
                    }
                }

                Debug.LogWarning($"Crafting Category {category} is abstract, yet it directly contains Recipes: {category.Elements}.");
                isValid = false;
            }

            for (int i = 0; i < category.Elements.Count; i++) {
                var recipe = category.Elements[i];
                if (recipe == null) {
                    Debug.LogWarning($"Crafting Category {category} has a null Recipe.");
                    isValid = false;
                    continue;
                }

                if (recipe.IsInitialized == false) {
                    Debug.LogWarning($"Crafting Category {category} has a Recipe {recipe} that is not part of the database.");
                    isValid = false;
                    continue;
                }

                if (recipe.Category != category) {
                    Debug.LogWarning(
                        $"Crafting Category {category} has a Recipe {recipe} which does not reference it as a category.");
                    isValid = false;
                    continue;
                }
            }

            return isValid;
        }

        /// <summary>
        /// Validate the Recipe.
        /// </summary>
        /// <param name="recipe">The recipe.</param>
        /// <param name="autoFix">Automatically fix the recipes?</param>
        /// <returns>True if valid.</returns>
        private static bool Validate(CraftingRecipe recipe, bool autoFix)
        {
            var isValid = true;

            if (recipe == null) {
                Debug.LogWarning($"Recipe is null.");
                return false;
            }

            if (RandomID.IsIDEmpty(recipe.ID)) {
                if (autoFix) {
                    AssignNewUniqueID(recipe, s_Database.CraftingRecipes);
                } else {
                    Debug.LogWarning($"Recipe {recipe} has a empty ID.");
                    isValid = false;
                }
            }

            if (recipe.Category == null) {
                Debug.LogWarning($"Recipe {recipe} has a null Recipe Category.");
                return false;
            }

            if (recipe.Category.IsInitialized == false) {
                if (autoFix) {
                    CraftingRecipeEditorUtility.ChangeCraftingRecipeType(recipe, UncategorizedCraftingCategory, s_Database,
                        GetDatabaseDirectory(s_Database));
                } else {
                    Debug.LogWarning($"Recipe {recipe} has a Recipe Category {recipe.Category} that is not part of the database.");
                    return false;
                }
            }

            if (recipe.Category.RecipeType == null || recipe.Category.RecipeType.Name == "NULL") {
                Debug.LogWarning($"'{recipe}' has a category recipe type '{recipe.Category.RecipeType}' (null)");
                return false;
            }

            if (recipe.Category.RecipeType != recipe.GetType() && !recipe.GetType().IsSubclassOf(recipe.Category.RecipeType)) {
                if (autoFix) {
                    CraftingRecipeEditorUtility.ChangeCraftingRecipeType(recipe, recipe.Category, s_Database,
                        GetDatabaseDirectory(s_Database));
                } else {
                    Debug.LogWarning($"'{recipe}' has a category recipe type '{recipe.Category.RecipeType}' that is not a subclass of this recipe type {recipe.GetType()}");
                    isValid = false;
                }

            }

            if (recipe.Category.DirectlyContains(recipe) == false) {

                if (autoFix) {
                    CraftingRecipeEditorUtility.SetCraftingRecipeCategory(recipe, recipe.Category);
                } else {
                    Debug.LogWarning($"Recipe {recipe} has Recipe Category {recipe.Category} but the category does not reference it as a direct Recipe.");
                    isValid = false;
                }
            }

            if (recipe.Category.IsAbstract) {
                Debug.LogWarning($"Recipe {recipe} has Crafting Category {recipe.Category} " +
                                 $"that is abstract, abstract categories should not have Elements.");
                return false;
            }

            return isValid;
        }

        /// <summary>
        /// Validates the ItemDefinition.
        /// </summary>
        /// <param name="currency">The itemDefinition.</param>
        /// <param name="autoFix">Automatically fix the currencies?</param>
        /// <returns>True if valid.</returns>
        private static bool Validate(Currency currency, bool autoFix)
        {
            var isValid = true;

            if (currency == null) {
                Debug.LogWarning($"Currency is null.");
                return false;
            }

            if (RandomID.IsIDEmpty(currency.ID)) {
                if (autoFix) {
                    AssignNewUniqueID(currency, s_Database.Currencies);
                } else {
                    Debug.LogWarning($"Currency {currency} has a empty ID.");
                    isValid = false;
                }
            }

            var parent = currency.Parent;
            if (parent != null) {
                var foundMatch = false;
                for (int i = parent.Children.Count - 1; i >= 0; i--) {
                    var sibling = parent.Children[i];
                    if (sibling == currency) { foundMatch = true; }
                }

                if (!foundMatch) {
                    if (autoFix) {
                        parent.Children.Add(currency);
                        CurrencyEditorUtility.SetCurrencyDirty(parent, true);
                    } else {
                        Debug.LogWarning($"Currency {currency} has a parent {parent}" +
                                         $", which does not reference it as a child.");
                        isValid = false;
                    }
                }
            }

            if (autoFix) {
                var amountRemoved = currency.Children.RemoveAll(x => x == null || x.IsInitialized == false);
                if (amountRemoved != 0) {
                    CurrencyEditorUtility.SetCurrencyDirty(currency, true);
                }
            }

            for (int i = currency.Children.Count - 1; i >= 0; i--) {
                var child = currency.Children[i];

                if (autoFix) {
                    if (child == null || child.IsInitialized == false || child.Parent == null) {
                        //Remove child connection
                        currency.Children.RemoveAt(i);
                        CurrencyEditorUtility.SetCurrencyDirty(currency, true);
                    } else if (child.Parent != currency) {
                        //Reconnect child
                        CurrencyEditorUtility.SetParent(child, parent);
                    }
                    continue;
                }

                if (child == null) {
                    Debug.LogWarning(
                        $"Currency {currency} has a null child.");
                    isValid = false;
                    continue;
                }

                if (child.IsInitialized == false) {
                    Debug.LogWarning(
                        $"Currency {currency} has a null child.");
                    isValid = false;
                    continue;
                }

                if (child.Parent != currency) {
                    Debug.LogWarning(
                            $"Currency {currency} has a child {child} which does not reference it as a parent.");
                    isValid = false;
                }
            }

            if (currency.SetMaxAmountCondition(currency.MaxAmount) == false) {
                if (autoFix) {
                    CurrencyEditorUtility.SetMaxAmount(currency, int.MaxValue);
                } else {
                    Debug.LogWarning(
                        $"Currency {currency} has an invalid maxAmount.");
                    isValid = false;
                }
            }

            if (currency.SetParentExchangeRateCondition(currency.ExchangeRateToParent) == false) {
                if (autoFix) {
                    CurrencyEditorUtility.SetExchangeRateToParent(currency, 1);
                } else {
                    Debug.LogWarning(
                        $"Currency {currency} has an invalid exchange rate to parent.");
                    isValid = false;
                }
            }

            if (currency.SetOverflowCurrencyCondition(currency.OverflowCurrency) == false) {
                if (autoFix) {
                    CurrencyEditorUtility.SetOverflowCurrency(currency, null);
                } else {
                    Debug.LogWarning(
                        $"Currency {currency} has an overflow currency.");
                    isValid = false;
                }
            }

            if (currency.SetFractionCurrencyCondition(currency.FractionCurrency) == false) {
                if (autoFix) {
                    CurrencyEditorUtility.SetFractionCurrency(currency, null);
                } else {
                    Debug.LogWarning(
                        $"Currency {currency} has an fraction currency.");
                    isValid = false;
                }
            }

            return isValid;
        }

        /// <summary>
        /// Assign a new and unique ID to an object by making sure it does not clash with existing ones.
        /// </summary>
        /// <param name="obj">The object which needs a new id.</param>
        /// <param name="arr">The array of existing object with ids.</param>
        /// <typeparam name="T">The object type.</typeparam>
        internal static void AssignNewUniqueID<T>(T obj, T[] arr) where T : IObjectWithID
        {
            var newID = RandomID.Generate();
            while (IDIsAvailable(newID, arr) == false) {
                newID = RandomID.Generate();
            }

            obj.ID = newID;
            if (obj is ScriptableObject scriptableObject) {
                Shared.Editor.Utility.EditorUtility.SetDirty(scriptableObject);
            }

        }

        /// <summary>
        /// Check if an id is available and does not clash with ids within the array.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="arr">The array containing existing ids.</param>
        /// <typeparam name="T">The object type.</typeparam>
        /// <returns>True if the id does not overlap.</returns>
        internal static bool IDIsAvailable<T>(uint id, T[] arr) where T : IObjectWithID
        {
            if (RandomID.IsIDEmpty(id)) { return false; }
            for (int i = 0; i < arr.Length; i++) {
                if (arr[i].ID == id) { return false; }
            }

            return true;
        }
    }
}
