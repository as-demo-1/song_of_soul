/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Utility
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Editor.Managers;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// The item definition editor utility.
    /// </summary>
    public static class ItemDefinitionEditorUtility
    {
        /// <summary>
        /// Add an itemDefinition to the database.
        /// </summary>
        /// <param name="newItemDefinitionName">The name of the Item Definition.</param>
        /// <param name="itemCategory">The itemCategory.</param>
        /// <param name="database">The database.</param>
        /// <param name="assetDirectory">The asset directory.</param>
        public static ItemDefinition AddItemDefinition(string newItemDefinitionName, ItemCategory itemCategory, InventorySystemDatabase database, string assetDirectory)
        {
            Undo.RegisterCompleteObjectUndo(database, "Add Item Definition");
            Undo.RegisterCompleteObjectUndo(itemCategory, "Add Item Definition");
            // Create the ScriptableObject representing the category and add the category to the database.
            var itemDefinition = ItemDefinition.Create(newItemDefinitionName, itemCategory);
            if (itemDefinition == null) {
                Debug.LogError("Error: The Item Definition cannot be created.");
                return null;
            }

            itemDefinition.ItemDefinitionAttributeCollection.ReevaluateAll(false);
            itemDefinition.DefaultItem.ItemAttributeCollection.ReevaluateAll(false);

            Undo.RegisterCreatedObjectUndo(itemDefinition, "Add Item Definition");

            database.AddItemDefinition(itemDefinition);

            AssetDatabaseUtility.CreateAsset(itemDefinition,
                $"{assetDirectory}\\ItemDefinitions\\{itemDefinition.name}",
                new string[] { database.name, itemDefinition.Category.name });

            ItemCategoryEditorUtility.SetItemCategoryDirty(itemCategory, false);
            Shared.Editor.Utility.EditorUtility.SetDirty(database);
            return itemDefinition;
        }

        /// <summary>
        /// Removes the Item Definition from any of its connections with the database.
        /// </summary>
        /// <param name="itemDefinition">The item definition.</param>
        /// <param name="database">The database.</param>
        public static void RemoveItemDefinition(ItemDefinition itemDefinition, InventorySystemDatabase database)
        {
            if (itemDefinition == null) { return; }
            var childrenArray = new ItemDefinition[itemDefinition.ChildrenReadOnly.Count];
            for (int i = 0; i < childrenArray.Length; i++) { childrenArray[i] = itemDefinition.ChildrenReadOnly[i]; }
            foreach (var child in childrenArray) {
                if (child == null) { continue; }

                SetItemDefinitionParent(child, itemDefinition.Parent);
            }

            //Set parent to null to remove it.
            SetItemDefinitionParent(itemDefinition, null);

            ItemCategoryEditorUtility.RemoveItemDefinitionFromCategory(itemDefinition.Category, itemDefinition);

            database.RemoveItemDefinition(itemDefinition);
            AssetDatabaseUtility.DeleteAsset(itemDefinition);
            Shared.Editor.Utility.EditorUtility.SetDirty(database);
        }

        /// <summary>
        /// Add an itemDefinition to the database.
        /// </summary>
        /// <param name="newItemDefinitionName">The name of the Item Definition.</param>
        /// <param name="itemCategory">The itemCategory.</param>
        /// <param name="database">The database.</param>
        /// <param name="assetDirectory">The asset directory.</param>
        public static ItemDefinition DuplicateItemDefinition(ItemDefinition original, InventorySystemDatabase database, string assetDirectory)
        {
            if (original == null) { return null; }

            Undo.RegisterCompleteObjectUndo(database, "Add Item Definition");
            Undo.RegisterCompleteObjectUndo(original.Category, "Add Item Definition");

            var name = AssetDatabaseUtility.FindValidName(original.name, database.ItemDefinitions);

            // Create the ScriptableObject representing the category and add the category to the database.
            var itemDefinition = ItemDefinition.Create(name, original.Category, original.Parent);

            if (itemDefinition == null) {
                Debug.LogError("Error: The Item Definition cannot be created.");
                return null;
            }

            for (int i = 0; i < itemDefinition.ItemDefinitionAttributeCollection.Count; i++) {
                var newAttribute = itemDefinition.ItemDefinitionAttributeCollection[i];
                original.ItemDefinitionAttributeCollection.TryGetAttribute(newAttribute.Name, out var originalAttribute);

                AttributeEditorUtility.DeepCopyAttribute(originalAttribute, newAttribute);
            }

            if (original.DefaultItem != null) {
                for (int i = 0; i < itemDefinition.DefaultItem.ItemAttributeCollection.Count; i++) {
                    var newAttribute = itemDefinition.DefaultItem.ItemAttributeCollection[i];
                    original.DefaultItem.ItemAttributeCollection.TryGetAttribute(newAttribute.Name, out var originalAttribute);

                    AttributeEditorUtility.DeepCopyAttribute(originalAttribute, newAttribute);
                }
            }

            itemDefinition.ItemDefinitionAttributeCollection.ReevaluateAll(false);
            itemDefinition.DefaultItem.ItemAttributeCollection.ReevaluateAll(false);

            Undo.RegisterCreatedObjectUndo(itemDefinition, "Add Item Definition");

            database.AddItemDefinition(itemDefinition);

            AssetDatabaseUtility.CreateAsset(itemDefinition,
                $"{assetDirectory}\\ItemDefinitions\\{itemDefinition.name}",
                new string[] { database.name, itemDefinition.Category.name });

            SetItemDefinitionDirty(itemDefinition, true);
            ItemCategoryEditorUtility.SetItemCategoryDirty(original.Category, false);
            Shared.Editor.Utility.EditorUtility.SetDirty(database);
            return itemDefinition;
        }

        /// <summary>
        /// Set Item Definition Dirty & sets the children array if necessary
        /// </summary>
        /// <param name="itemDefinition">The ItemDefinition to dirty.</param>
        /// <param name="force">Force the dirtying.</param>
        public static void SetItemDefinitionDirty(ItemDefinition itemDefinition, bool force)
        {
            if (itemDefinition.Dirty == false && !force) { return; }

            itemDefinition.Serialize();
            itemDefinition.DefaultItem.Serialize();

            AttributeEditorUtility.SerializeAttributes(itemDefinition);

            Shared.Editor.Utility.EditorUtility.SetDirty(itemDefinition);
            itemDefinition.Dirty = false;
        }

        /// <summary>
        /// Set the ItemDefinition Category and serialize.
        /// </summary>
        /// <param name="itemDefinition">The ItemDefinition.</param>
        /// <param name="newCategory">The new ItemCategory.</param>
        /// <param name="relation">The relations that should be affected.</param>
        public static void SetItemDefinitionCategory(ItemDefinition itemDefinition, ItemCategory newCategory, Relation relation)
        {
            if (itemDefinition == null) { return; }
            if (newCategory == null) { newCategory = DatabaseValidator.UncategorizedItemCategory; }

            var previousCategory = itemDefinition.Category;
            var pooledItemDefFamily = GenericObjectPool.Get<ItemDefinition[]>();
            var itemDefinitionFamilyCount = itemDefinition.GetAllFamily(ref pooledItemDefFamily);

            if (previousCategory != null) { Undo.RegisterCompleteObjectUndo(previousCategory, "Category Change"); }
            for (int i = 0; i < itemDefinitionFamilyCount; i++) {
                if (pooledItemDefFamily[i] != null) { Undo.RegisterCompleteObjectUndo(pooledItemDefFamily[i], "Category Change"); }
            }
            if (newCategory != null) { Undo.RegisterCompleteObjectUndo(newCategory, "Category Change"); }

            itemDefinition.SetCategory(newCategory, relation);
            itemDefinition.ItemDefinitionAttributeCollection.ReevaluateAll(false);
            itemDefinition.DefaultItem.ItemAttributeCollection.ReevaluateAll(false);

            if (previousCategory != null) { ItemCategoryEditorUtility.SetItemCategoryDirty(previousCategory, false); }
            for (int i = 0; i < itemDefinitionFamilyCount; i++) {
                if (pooledItemDefFamily[i] != null) { SetItemDefinitionDirty(pooledItemDefFamily[i], false); }
            }
            if (newCategory != null) { ItemCategoryEditorUtility.SetItemCategoryDirty(newCategory, false); }

            GenericObjectPool.Return(pooledItemDefFamily);
        }

        /// <summary>
        /// Set the ItemDefinition Parent.
        /// </summary>
        /// <param name="itemDefinition">The itemDefinition.</param>
        /// <param name="newParent">The new parent.</param>
        public static void SetItemDefinitionParent(ItemDefinition itemDefinition, ItemDefinition newParent)
        {
            var previousParent = itemDefinition.Parent;
            if (previousParent != null) { Undo.RegisterCompleteObjectUndo(previousParent, "Parent Change"); }
            if (itemDefinition != null) { Undo.RegisterCompleteObjectUndo(itemDefinition, "Parent Change"); }
            if (newParent != null) { Undo.RegisterCompleteObjectUndo(newParent, "Parent Change"); }

            itemDefinition.SetParent(newParent);

            if (previousParent != null) { SetItemDefinitionDirty(previousParent, false); }
            if (itemDefinition != null) { SetItemDefinitionDirty(itemDefinition, false); }
            if (newParent != null) { SetItemDefinitionDirty(newParent, false); }
        }

        /// <summary>
        /// Set the Icon.
        /// </summary>
        /// <param name="itemDefinition">The item definition.</param>
        /// <param name="newIcon">The new icon.</param>
        public static void SetIcon(ItemDefinition itemDefinition, Sprite newIcon)
        {
            Undo.RegisterCompleteObjectUndo(itemDefinition, "Icon Change");
            itemDefinition.m_EditorIcon = newIcon;
            SetItemDefinitionDirty(itemDefinition, true);
        }

        /// <summary>
        /// Returns a list of options for sorting.
        /// </summary>
        /// <returns>The list of sort options.</returns>
        public static IList<SortOption> SortOptions()
        {
            return new SortOption[]
            {
                new SortOption("A-Z", list => (list as List<ItemDefinition>).Sort(
                    (x, y) =>
                    {
                        if (x == y) { return 0; }
                        if (x == null) { return -1;}
                        if (y == null) { return 1; }
                        return x?.name.CompareTo(y?.name ?? "") ?? -1;
                    })),
                new SortOption("Z-A", list => (list as List<ItemDefinition>).Sort(
                    (x, y) =>
                    {
                        if (x == y) { return 0; }
                        if (x == null) { return -1;}
                        if (y == null) { return 1; }
                        return y?.name.CompareTo(x?.name ?? "") ?? 1;
                    })),
                new SortOption("Category A-Z",list => (list as List<ItemDefinition>).Sort(
                    (x, y) =>
                    {
                        if (x?.Category == y?.Category) { return 0; }
                        if (x?.Category == DatabaseValidator.UncategorizedItemCategory) { return -1; }
                        if (y?.Category == DatabaseValidator.UncategorizedItemCategory) { return 1; }
                        return x?.Category.name.CompareTo(y?.Category.name ?? "") ?? -1;
                    })),
                new SortOption("Category Z-A",list => (list as List<ItemDefinition>).Sort(
                    (x, y) =>
                    {
                        if (x?.Category == y?.Category) { return 0; }
                        if (x?.Category == DatabaseValidator.UncategorizedItemCategory) { return -1; }
                        if (y?.Category == DatabaseValidator.UncategorizedItemCategory) { return 1; }
                        return y?.Category.name.CompareTo(x?.Category.name ?? "") ?? 1;
                    })),
            };
        }

        /// <summary>
        /// Search filter for the Item Definition list.
        /// </summary>
        /// <param name="list">The list to search.</param>
        /// <param name="searchValue">The search string.</param>
        /// <returns>A new filtered list.</returns>
        public static IList<ItemDefinition> SearchFilter(IList<ItemDefinition> list, string searchValue)
        {
            var compareInfo = CultureInfo.CurrentCulture.CompareInfo;

            return ManagerUtility.SearchFilter(list, searchValue,
                new (string prefix, Func<string, ItemDefinition, bool>)[]
                {
                    //Search by category.
                    ("c:",(searchWord, itemDefinition) =>{
                        // Case insensitive Contains(string).
                        if (compareInfo.IndexOf(itemDefinition.Category.name, searchWord,
                                CompareOptions.IgnoreCase) >= 0) { return true; }

                        return false;
                    }),
                    // search by category inherently
                    ("i:",(searchWord, itemDefinition) =>{
                        // Case insensitive Contains(string).
                        var ancestorCategories = new ItemCategory[0];
                        var ancestorsCount = itemDefinition.Category.GetAllParents(ref ancestorCategories, true);
                        for (int i = 0; i < ancestorsCount; i++) {
                            if (compareInfo.IndexOf(ancestorCategories[i].name, searchWord,
                                CompareOptions.IgnoreCase) >= 0) { return true; }
                        }
                        
                        return false;
                    }),
                    //Search by attribute.
                    ("a:", (searchWord, itemDefinition) =>
                    {
                       var allAttributesCount = itemDefinition.GetAttributeCount(false);
                        for (int k = 0; k < allAttributesCount; k++) {
                            var attribute = itemDefinition.GetAttributeAt(k,false);
                            if (compareInfo.IndexOf(attribute.Name, searchWord, CompareOptions.IgnoreCase) < 0) { continue; }

                            return true;
                        }

                        return false;
                    })
                });
        }
    }
}