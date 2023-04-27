/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Utility
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Editor.Managers;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// The item category editor utility.
    /// </summary>
    public class ItemCategoryEditorUtility : CategoryEditorUtility<ItemCategory, ItemDefinition>
    {
        private static ItemCategoryEditorUtility s_Instance;

        public static ItemCategoryEditorUtility Instance => s_Instance ?? (s_Instance = new ItemCategoryEditorUtility());

        /// <summary>
        /// Adds the ItemCategory to the database
        /// </summary>
        /// <param name="newItemCategoryName">The category name.</param>
        /// <param name="database">The database.</param>
        /// <param name="databaseDirectory">The database directory.</param>
        public static ItemCategory AddItemCategory(string newItemCategoryName, InventorySystemDatabase database, string databaseDirectory)
        {
            var itemCategory = ItemCategory.Create(newItemCategoryName);

            itemCategory.ItemCategoryAttributeCollection.ReevaluateAll(false);
            itemCategory.ItemDefinitionAttributeCollection.ReevaluateAll(false);
            itemCategory.ItemAttributeCollection.ReevaluateAll(false);

            Instance.AddCategoryInternal(
                itemCategory, database,
                databaseDirectory + "\\ItemCategories\\",
                "Add ItemCategory");

            return itemCategory;
        }

        /// <summary>
        /// Removes the ItemCategory from any of its connections with the database
        /// </summary>
        /// <param name="itemCategory">The category.</param>
        /// <param name="database">The database.</param>
        public static void RemoveItemCategory(ItemCategory itemCategory, InventorySystemDatabase database)
        {
            var parents = itemCategory.Parents;
            var children = itemCategory.Children;
            Instance.RemoveCategoryInternal(itemCategory, database);

            for (int i = 0; i < parents.Count; i++) {
                SetItemCategoryDirty(parents[i], true);
            }
            for (int i = 0; i < children.Count; i++) {
                SetItemCategoryDirty(children[i], true);
            }
        }

        /// <summary>
        /// Duplicate the ItemCategory from any of its connections with the database
        /// </summary>
        /// <param name="originalItemCategory">The category to duplicate.</param>
        /// <param name="database">The database.</param>
        public static ItemCategory DuplicateItemCategory(ItemCategory originalItemCategory, InventorySystemDatabase database, string databaseDirectory)
        {

            var name = AssetDatabaseUtility.FindValidName(originalItemCategory.name, database.ItemCategories);

            var itemCategory = ItemCategory.Create(name, originalItemCategory.IsMutable, originalItemCategory.IsUnique,
                originalItemCategory.IsAbstract);

            itemCategory.AddParents(originalItemCategory.Parents);

            itemCategory.AddOrOverrideCategoryAttributes(originalItemCategory.ItemCategoryAttributeCollection.Attributes);
            itemCategory.AddDefinitionAttributes(originalItemCategory.ItemDefinitionAttributeCollection.Attributes);
            itemCategory.AddItemAttributes(originalItemCategory.ItemAttributeCollection.Attributes);

            itemCategory.ItemCategoryAttributeCollection.OverrideAttributes(originalItemCategory.ItemCategoryAttributeCollection.Attributes);
            itemCategory.ItemDefinitionAttributeCollection.OverrideAttributes(originalItemCategory.ItemDefinitionAttributeCollection.Attributes);
            itemCategory.ItemAttributeCollection.OverrideAttributes(originalItemCategory.ItemAttributeCollection.Attributes);

            // Deep copy the attribute such that calsses does not reference the original attribute value.
            var attributesCount = itemCategory.GetAttributesCount();
            for (int i = 0; i < attributesCount; i++) {
                var attribute = itemCategory.GetAttributesAt(i);
                AttributeEditorUtility.DeepCopyAttribute(attribute, attribute);
            }

            itemCategory.ItemCategoryAttributeCollection.ReevaluateAll(false);
            itemCategory.ItemDefinitionAttributeCollection.ReevaluateAll(false);
            itemCategory.ItemAttributeCollection.ReevaluateAll(false);

            Instance.AddCategoryInternal(
                itemCategory, database,
                databaseDirectory + "\\ItemCategories\\",
                "Add ItemCategory");

            for (int i = 0; i < itemCategory.Parents.Count; i++) {
                SetItemCategoryDirty(itemCategory.Parents[i], true);
            }

            SetItemCategoryDirty(itemCategory, true);

            return itemCategory;
        }

        /// <summary>
        /// Set the category IsAbstract, returns the new value of isAbstract, which can be different then the value provided 
        /// if something is preventing from letting you set the abstract field
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="isAbstract">Is the category abstract.</param>
        /// <returns>The new is abstract value.</returns>
        public static bool SetIsAbstract(ItemCategory category, bool isAbstract)
        {
            return Instance.SetIsAbstractInternal(category, isAbstract);
        }

        /// <summary>
        /// Set the category Color.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="newColor">The new color.</param>
        public static void SetColor(ItemCategory category, Color newColor)
        {
            Instance.SetColorInternal(category, newColor);
        }

        /// <summary>
        /// Set the category Icon.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="newIcon">The new icon.</param>
        public static void SetIcon(ItemCategory category, Sprite newIcon)
        {
            Instance.SetIconInternal(category, newIcon);
        }

        /// <summary>
        /// Set the category IsMutable, returns the new value of isMutable, which can be different then the value provided 
        /// if something is preventing from letting you set the mutablity field
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="isMutable">Is the category mutable.</param>
        /// <returns>The new value.</returns>
        public static bool SetIsMutable(ItemCategory category, bool isMutable)
        {
            Undo.RegisterCompleteObjectUndo(category, "Mutable Change");
            category.IsMutable = isMutable;
            SetItemCategoryDirty(category, false);

            return isMutable;
        }

        /// <summary>
        /// Set the category IsUnique, returns the new value of isUnique, which can be different then the value provided 
        /// if something is preventing from letting you set the unique field
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="isUnique">Is the category mutable.</param>
        /// <returns>The new value.</returns>
        public static bool SetIsUnique(ItemCategory category, bool isUnique)
        {
            Undo.RegisterCompleteObjectUndo(category, "Unique Change");
            category.IsUnique = isUnique;
            SetItemCategoryDirty(category, false);

            return isUnique;
        }

        /// <summary>
        /// Add an parent from its category.
        /// </summary>
        /// <param name="category">The ItemCategory.</param>
        /// <param name="parent">The parent.</param>
        public static void AddItemCategoryParent(ItemCategory category, ItemCategory parent)
        {
            Instance.AddCategoryParentInternal(category, parent);
            category.ItemCategoryAttributeCollection.ReevaluateAll(false);
            category.ItemDefinitionAttributeCollection.ReevaluateAll(false);
            category.ItemAttributeCollection.ReevaluateAll(false);
        }

        /// <summary>
        /// Remove an parent from its category.
        /// </summary>
        /// <param name="category">The ItemCategory.</param>
        /// <param name="parent">The parent.</param>
        public static void RemoveItemCategoryParent(ItemCategory category, ItemCategory parent)
        {
            Instance.RemoveCategoryParentInternal(category, parent);
        }

        /// <summary>
        /// Remove an ItemDefinition from its category, use when the ItemDefinition changes category or is deleted
        /// </summary>
        /// <param name="category">The category that the element belongs to.</param>
        /// <param name="element">The element that should be changed/removed.</param>
        /// <returns>True if the item definition was removed.</returns>
        public static bool RemoveItemDefinitionFromCategory(ItemCategory category, ItemDefinition element)
        {
            return Instance.RemoveElementFromCategoryInternal(category, element);
        }

        /// <summary>
        /// Add an attribute to the category.
        /// </summary>
        /// <param name="itemCategory">The Item Category.</param>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="selectedCollectionIndex">The index of the selected attribute collection.</param>
        public static AttributeBase AddAttribute(ItemCategory itemCategory, string attributeName, int selectedCollectionIndex)
        {
            RegisterUndoItemCategoryChildrenConnections(itemCategory, "Add Attribute");

            // Create the attribute from the generic type.
            var attributeType = typeof(Attribute<>).MakeGenericType(typeof(int));
            var attribute = System.Activator.CreateInstance(attributeType) as AttributeBase;

            // Populate the attribute values.
            var baseName = attributeName;
            var index = 1;
            while (itemCategory.IsNewAttributeValid(attributeName, attribute.GetValueType(), selectedCollectionIndex) == false) {
                attributeName = baseName + "_" + index;
                index++;
            }
            attribute.Name = attributeName;
            attribute.ReevaluateValue(true);
            var added = false;
            if (selectedCollectionIndex == 0) { // Item Category Attributes.
                added = itemCategory.AddOrOverrideCategoryAttribute(attribute);
            } else if (selectedCollectionIndex == 1) { // Item Definition Attributes.
                added = itemCategory.AddDefinitionAttribute(attribute);
            } else { // Item Attributes.
                added = itemCategory.AddItemAttribute(attribute);
            }
            if (added) {
                SerializeCategoryChildrenConnections(itemCategory);
            }

            return attribute;
        }

        /// <summary>
        /// Duplicate an attribute within the category.
        /// </summary>
        /// <param name="category">The category with the attribute.</param>
        /// <param name="attribute">The attribute to duplicate.</param>
        public static void DuplicateAttribute(ItemCategory category, AttributeBase attribute)
        {
            if (category == null || attribute == null) { return; }

            var pooledArray = GenericObjectPool.Get<AttributeBase[]>();
            var sources = attribute.GetAttributeFamilySources(ref pooledArray);
            var attributeCollectionIndex = attribute.AttributeCollectionIndex;

            // First find a valid name for the duplicate attribute.
            var baseName = AssetDatabaseUtility.GetBaseName(attribute.Name);
            var validName = baseName;
            var count = 1;
            var foundValidName = false;
            while (foundValidName == false) {
                foundValidName = true;

                for (int i = 0; i < sources.Count; i++) {
                    var sourceCategory = sources[i].AttachedItemCategory;
                    if (sourceCategory.IsNewAttributeValid(validName, attribute.GetValueType(), attributeCollectionIndex) == false) {
                        validName = baseName + "_" + count;
                        count++;
                        foundValidName = false;
                        break;
                    }
                }
            }

            // Create the duplicate attribute and assign it to the sources
            for (int i = 0; i < sources.Count; i++) {
                var sourceCategory = sources[i].AttachedItemCategory;
                var newAttribute = System.Activator.CreateInstance(attribute.GetType()) as AttributeBase;
                newAttribute.Name = validName;
                newAttribute.ReevaluateValue(true);

                if (attributeCollectionIndex == 0) {
                    sourceCategory.AddOrOverrideCategoryAttribute(newAttribute);
                } else if (attributeCollectionIndex == 1) {
                    sourceCategory.AddDefinitionAttribute(newAttribute);
                } else {
                    sourceCategory.AddItemAttribute(newAttribute);
                }
            }

            // Copy the values from the original attributes to the duplicates.
            var familyAttributes = attribute.GetAttributeFamily();
            for (int i = 0; i < familyAttributes.Count; i++) {
                var originalAttribute = familyAttributes[i];
                originalAttribute.AttributeCollection.TryGetAttribute(validName, out var newAttribute);

                AttributeEditorUtility.DeepCopyAttribute(originalAttribute, newAttribute);
            }

            // Serialize everything
            for (int i = 0; i < sources.Count; i++) {
                SerializeCategoryChildrenConnections(sources[i].AttachedItemCategory);
            }

            GenericObjectPool.Return(pooledArray);

        }

        /// <summary>
        /// Sets the category as changed.
        /// </summary>
        /// <param name="itemCategory">The Item Category that is dirty.</param>
        /// <param name="force">Force dirty.</param>
        public static void SetItemCategoryDirty(ItemCategory itemCategory, bool force)
        {
            Instance.SetCategoryDirtyInternal(itemCategory, force);
        }

        /// <summary>
        /// Serialize the entire family of category.
        /// </summary>
        /// <param name="category">The category.</param>
        public static void SerializeCategoryFamilyConnections(ItemCategory category)
        {
            Instance.SerializeCategoryFamilyConnectionsInternal(category);
        }

        /// <summary>
        /// Serialize the entire family of category.
        /// </summary>
        /// <param name="category">The category.</param>
        public static void SerializeCategoryChildrenConnections(ItemCategory category)
        {
            Instance.SerializeCategoryChildrenConnectionsInternal(category);
        }

        /// <summary>
        /// Register all connections of ItemCategory for undo.
        /// </summary>
        /// <param name="itemCategory">The itemCategory.</param>
        /// <param name="undoKey">The undo key.</param>
        public static void RegisterUndoItemCategoryFamilyConnections(ItemCategory itemCategory, string undoKey)
        {
            Instance.RegisterUndoCategoryFamilyConnectionsInternal(itemCategory, undoKey);
        }

        /// <summary>
        /// Register all children connections of ItemCategory for undo.
        /// </summary>
        /// <param name="itemCategory">The itemCategory.</param>
        /// <param name="undoKey">The undo key.</param>
        public static void RegisterUndoItemCategoryChildrenConnections(ItemCategory itemCategory, string undoKey)
        {
            Instance.RegisterUndoCategoryChildrenConnectionsInternal(itemCategory, undoKey);
        }

        /// <summary>
        /// Set the category to dirty.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="force">Force the dirtying.</param>
        public override void SetCategoryDirtyInternal(ItemCategory category, bool force)
        {
            if (category.Dirty == false && !force) { return; }

            category.Serialize();
            AttributeEditorUtility.SerializeAttributes(category);

            Shared.Editor.Utility.EditorUtility.SetDirty(category);
            category.Dirty = false;
        }

        /// <summary>
        /// Set the element to dirty.
        /// </summary>
        /// <param name="element">The item definition.</param>
        protected override void SetElementDirtyInternal(ItemDefinition element)
        {
            ItemDefinitionEditorUtility.SetItemDefinitionDirty(element, false);
        }

        /// <summary>
        /// Add the category to the database.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="database">The database.</param>
        protected override void AddToDatabaseInternal(ItemCategory category, InventorySystemDatabase database)
        {
            database.AddItemCategory(category);
        }

        /// <summary>
        /// Remove the category from the database.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="database">The database.</param>
        protected override void RemoveFromDatabaseInternal(ItemCategory category, InventorySystemDatabase database)
        {
            database.RemoveItemCategory(category);
        }

        /// <summary>
        /// Set the element category.
        /// </summary>
        /// <param name="element">The item definition.</param>
        /// <param name="newCategory">The new category.</param>
        protected override void SetElementCategoryInternal(ItemDefinition element, ItemCategory newCategory)
        {
            ItemDefinitionEditorUtility.SetItemDefinitionCategory(element, newCategory, Relation.Family);
        }

        /// <summary>
        /// Returns a list of options for sorting.
        /// </summary>
        /// <returns>The list of sort options.</returns>
        public static IList<SortOption> SortOptions()
        {
            return new SortOption[]
            {
                new SortOption("A-Z", list => (list as List<ItemCategory>).Sort(
                    (x, y) =>
                    {
                        if (x == null) { return -1; }
                        if (y == null) { return 1; }
                        if (x == DatabaseValidator.UncategorizedItemCategory) { return -1; }
                        if (y == DatabaseValidator.UncategorizedItemCategory) { return 1; }
                        return x?.name.CompareTo(y?.name) ?? 1;
                    })),
                new SortOption("Z-A", list => (list as List<ItemCategory>).Sort(
                    (x, y) =>
                    {
                        if (x == null) { return -1; }
                        if (y == null) { return 1; }
                        if (x == DatabaseValidator.UncategorizedItemCategory) { return -1; }
                        if (y == DatabaseValidator.UncategorizedItemCategory) { return 1; }
                        return y?.name.CompareTo(x?.name) ?? 1;
                    })),
                new SortOption("Hierarchy",list => (list as List<ItemCategory>).Sort(
                    (x, y) =>
                    {
                        if (x == null) { return -1; }
                        if (y == null) { return 1; }
                        if (x == DatabaseValidator.UncategorizedItemCategory) { return -1; }
                        if (y == DatabaseValidator.UncategorizedItemCategory) { return 1; }
                        return 0;
                    }))
            };
        }

        /// <summary>
        /// Search filter for the ItemCategory list
        /// </summary>
        /// <param name="list">The list to search.</param>
        /// <param name="searchValue">The search string.</param>
        /// <returns>A new filtered list.</returns>
        public static IList<ItemCategory> SearchFilter(IList<ItemCategory> list, string searchValue)
        {
            var compareInfo = CultureInfo.CurrentCulture.CompareInfo;

            return ManagerUtility.SearchFilter(list, searchValue,
                new (string prefix, Func<string, ItemCategory, bool>)[]
                {
                    //Search by category.
                    ("c:",(searchWord, itemCategory) =>{
                        // Case insensitive Contains(string).
                        for (int i = 0; i < itemCategory.Parents.Count; i++) {
                            if (compareInfo.IndexOf(itemCategory.Parents[i].name, searchWord,
                                CompareOptions.IgnoreCase) >= 0) { return true; }
                        }
                        
                        return false;
                    }),
                    ("i:",(searchWord, itemCategory) =>{
                        // Case insensitive Contains(string).
                        var ancestorCategories = new ItemCategory[0];
                        var ancestorsCount = itemCategory.GetAllParents(ref ancestorCategories, true);
                        for (int i = 0; i < ancestorsCount; i++) {
                            if (compareInfo.IndexOf(ancestorCategories[i].name, searchWord,
                                CompareOptions.IgnoreCase) >= 0) { return true; }
                        }
                        
                        return false;
                    }),
                    ("a:", (searchWord, category) =>
                    {
                        var allAttributesCount = category.GetAttributesCount();
                        for (int k = 0; k < allAttributesCount; k++) {
                            var attribute = category.GetAttributesAt(k);
                            if (compareInfo.IndexOf(attribute.Name, searchWord, CompareOptions.IgnoreCase) < 0) { continue; }

                            return true;
                        }

                        return false;
                    })
                });
        }
    }
}

