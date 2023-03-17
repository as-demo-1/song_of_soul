/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Utility
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Storage;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// base class of category object utility functions
    /// </summary>
    /// <typeparam name="Tc">The category type.</typeparam>
    /// <typeparam name="Te">The element type.</typeparam>
    public abstract class CategoryEditorUtility<Tc, Te> where Tc : ObjectCategoryBase<Tc, Te> where Te : ScriptableObject, ICategoryElement<Tc, Te>
    {
        /// <summary>
        /// Adds the Category to the database
        /// </summary>
        /// <param name="category">The category to add.</param>
        /// <param name="database">The database to add to.</param>
        /// <param name="assetDirectoryPath">The database to add to.</param>
        /// <param name="undoKey">The database to add to.</param>
        protected bool AddCategoryInternal(Tc category, InventorySystemDatabase database,
            string assetDirectoryPath, string undoKey)
        {
            // Create the ScriptableObject representing the category and add the category to the database.
            if (category == null) {
                Debug.LogError("Error: The category cannot be created.");
                return false;
            }
            Undo.RegisterCompleteObjectUndo(database, undoKey);
            Undo.RegisterCreatedObjectUndo(category, undoKey);

            AddToDatabaseInternal(category, database);

            var success = AssetDatabaseUtility.CreateAsset(category, assetDirectoryPath + category.name, new string[] { database.name });
            Shared.Editor.Utility.EditorUtility.SetDirty(database);
            if (!success) { return false; }

            return true;
        }

        /// <summary>
        /// Removes the Category from any of its connections with the database
        /// </summary>
        /// <param name="category">The category to remove.</param>
        /// <param name="database">The database to remove from.</param>
        public void RemoveCategoryInternal(Tc category, InventorySystemDatabase database)
        {
            RegisterUndoCategoryFamilyConnectionsInternal(category, "Remove Category");
            MoveDirectElementsToRelativeInternal(category);

            var affectedCategories = new List<Tc>();
            while (category.ChildrenReadOnly.Count != 0) {
                var child = category.ChildrenReadOnly[0];
                child.RemoveParent((Tc)category);
                for (int i = 0; i < category.ParentsReadOnly.Count; i++) {
                    child.AddParent(category.ParentsReadOnly[i]);
                    affectedCategories.Add(category.ParentsReadOnly[i]);
                }
                affectedCategories.Add(child);
            }

            while (category.ParentsReadOnly.Count != 0) {
                var parent = category.ParentsReadOnly[0];
                category.RemoveParent(parent);
                affectedCategories.Add(parent);
            }

            for (int i = 0; i < affectedCategories.Count; i++) {
                SetCategoryDirtyInternal(affectedCategories[i], false);
            }

            RemoveFromDatabaseInternal(category, database);
            AssetDatabaseUtility.DeleteAsset(category);
            Shared.Editor.Utility.EditorUtility.SetDirty(database);
        }

        /// <summary>
        /// Add the category from the database.
        /// </summary>
        /// <param name="category">The category to Add.</param>
        /// <param name="database">The database.</param>
        protected abstract void AddToDatabaseInternal(Tc category, InventorySystemDatabase database);

        /// <summary>
        /// Remove the category from the database.
        /// </summary>
        /// <param name="category">The category to remove.</param>
        /// <param name="database">The database.</param>
        protected abstract void RemoveFromDatabaseInternal(Tc category, InventorySystemDatabase database);

        /// <summary>
        /// Moves the direct Definitions to a relative if there is one.
        /// </summary>
        /// <param name="category">The new category for the element.</param>
        protected void MoveDirectElementsToRelativeInternal(Tc category)
        {
            //Definitions are set to a child of the Category, or null if none exist
            var elementsArrayCopy = new Te[category.ElementsReadOnly.Count];
            for (int i = 0; i < elementsArrayCopy.Length; i++) {
                elementsArrayCopy[i] = category.ElementsReadOnly[i];
            }

            foreach (var element in elementsArrayCopy) {
                Tc newCategory = null;
                for (int i = 0; i < category.Children.Count; i++) {
                    if (category.Children[i].IsAbstract) { continue; }

                    newCategory = category.Children[i];
                }

                SetElementCategoryInternal(element, newCategory);
            }
        }

        /// <summary>
        /// Set the element category.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="newCategory">Its new category.</param>
        protected abstract void SetElementCategoryInternal(Te element, Tc newCategory);

        /// <summary>
        /// Set the category IsAbstract, returns the new value of isAbstract, which can be different then the value provided 
        /// if something is preventing from letting you set the abstract field
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="isAbstract">The new is abstract value.</param>
        /// <returns>The new is abstract value.</returns>
        public bool SetIsAbstractInternal(Tc category, bool isAbstract)
        {
            RegisterUndoCategoryFamilyConnectionsInternal(category, "Abstract Change");

            if (category.ElementsReadOnly.Count != 0 && isAbstract) {
                MoveDirectElementsToRelativeInternal(category);
            }
            category.IsAbstract = isAbstract;

            SetCategoryDirtyInternal(category, false);
            return isAbstract;
        }

        /// <summary>
        /// Set the category Color
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="newColor">The new color.</param>
        public void SetColorInternal(Tc category, Color newColor)
        {
            Undo.RegisterCompleteObjectUndo(category, "Color Change");
            category.m_Color = newColor;
            SetCategoryDirtyInternal(category, true);
        }

        /// <summary>
        /// Set the category Icon
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="newIcon">The new icon.</param>
        public void SetIconInternal(Tc category, Sprite newIcon)
        {
            Undo.RegisterCompleteObjectUndo(category, "Icon Change");
            category.m_EditorIcon = newIcon;
            SetCategoryDirtyInternal(category, true);
        }

        /// <summary>
        /// Add an parent from its category.
        /// </summary>
        /// <param name="category">The Category.</param>
        /// <param name="parent">The parent.</param>
        public void AddCategoryParentInternal(Tc category, Tc parent)
        {
            if (parent != null) {
                Undo.RegisterCompleteObjectUndo(parent, "Add Category Parent");
            }
            RegisterUndoCategoryChildrenConnectionsInternal(category, "Add Category Parent");

            var success = category.AddParent(parent);
            if (success) {
                SerializeCategoryFamilyConnectionsInternal(category);
            }
        }

        /// <summary>
        /// Remove an parent from its category.
        /// </summary>
        /// <param name="category">The Category.</param>
        /// <param name="parent">The parent.</param>
        public void RemoveCategoryParentInternal(Tc category, Tc parent)
        {
            RegisterUndoCategoryChildrenConnectionsInternal(category, "Remove Category Parent");

            var success = category.RemoveParent(parent);
            if (success) {
                SerializeCategoryFamilyConnectionsInternal(category);
            }
            SetCategoryDirtyInternal(parent, false);
        }

        /// <summary>
        /// Removes an element from its category, use when the element changes category or is deleted.
        /// </summary>
        /// <param name="category">The category that the element belongs to.</param>
        /// <param name="element">The element that should be changed/removed.</param>
        /// <returns>True if the element was removed.</returns>
        public bool RemoveElementFromCategoryInternal(Tc category, Te element)
        {
            if (element.Category != category) {
                Debug.LogError("Error: The element is not part of the category.");
            }

            if (category.DirectlyContains(element) == false) {
                Debug.LogError("Error: The element has already been removed.");
            }

            category.RemoveElement(element);

            SetCategoryDirtyInternal(category, false);
            return true;
        }

        /// <summary>
        /// Set Definition Dirty & sets the children array if necessary
        /// </summary>
        /// <param name="category">The Definition to dirty.</param>
        /// <param name="force">Force the dirtyingIf true it will set the children array correctly.</param>
        public virtual void SetCategoryDirtyInternal(Tc category, bool force)
        {
            if (category.Dirty == false && !force) { return; }

            category.Serialize();

            Shared.Editor.Utility.EditorUtility.SetDirty(category);
            category.Dirty = false;
        }

        /// <summary>
        /// Serialize the entire family of category
        /// </summary>
        /// <param name="category">The category to serialize.</param>
        public void SerializeCategoryFamilyConnectionsInternal(Tc category)
        {
            var pooledAllFamily = GenericObjectPool.Get<Tc[]>();
            var categoryFamilyCount = category.GetAllFamily(ref pooledAllFamily);
            for (int i = 0; i < categoryFamilyCount; i++) {
                SetCategoryDirtyInternal(pooledAllFamily[i], false);
            }
            GenericObjectPool.Return(pooledAllFamily);

            var pooledDefs = GenericObjectPool.Get<Te[]>();
            var elementFamilyCount = category.GetAllFamilyElements(ref pooledDefs);
            for (int i = 0; i < elementFamilyCount; i++) {
                SetElementDirtyInternal(pooledDefs[i]);
            }
            GenericObjectPool.Return(pooledDefs);
        }

        protected abstract void SetElementDirtyInternal(Te element);

        /// <summary>
        /// Serialize the entire family of category
        /// </summary>
        /// <param name="category">The category to serialize.</param>
        public void SerializeCategoryChildrenConnectionsInternal(Tc category)
        {
            var pooledAllFamily = GenericObjectPool.Get<Tc[]>();
            var categoryChildrenCount = category.GetAllChildren(ref pooledAllFamily, true);
            for (int i = 0; i < categoryChildrenCount; i++) {
                SetCategoryDirtyInternal(pooledAllFamily[i], false);
            }
            GenericObjectPool.Return(pooledAllFamily);

            var pooledDefs = GenericObjectPool.Get<Te[]>();
            var elementChildrenCount = category.GetAllChildrenElements(ref pooledDefs);
            for (int i = 0; i < elementChildrenCount; i++) {
                SetElementDirtyInternal(pooledDefs[i]);
            }
            GenericObjectPool.Return(pooledDefs);
        }

        /// <summary>
        /// Register all connections of Category for undo.
        /// </summary>
        /// <param name="category">The Category.</param>
        /// <param name="undoKey">The undo key.</param>
        public void RegisterUndoCategoryFamilyConnectionsInternal(Tc category, string undoKey)
        {
            var categoryFamily = category.GetAllFamilyConnectedObjects();
            Undo.RegisterCompleteObjectUndo(categoryFamily, undoKey);
        }

        /// <summary>
        /// Register all children connections of Category for undo.
        /// </summary>
        /// <param name="Category">The Category.</param>
        /// <param name="undoKey">The undo key.</param>
        public void RegisterUndoCategoryChildrenConnectionsInternal(Tc Category, string undoKey)
        {
            var categoryChildren = Category.GetAllChildrenConnectedObjects(true);
            for (int i = 0; i < categoryChildren.Length; i++) {
                Undo.RegisterCompleteObjectUndo(categoryChildren[i], undoKey);
            }
        }
    }
}