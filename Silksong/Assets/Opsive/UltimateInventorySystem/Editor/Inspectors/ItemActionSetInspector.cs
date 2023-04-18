/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Inspectors
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.ItemActions;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine.UIElements;

    /// <summary>
    /// Custom editor to display the category item actions.
    /// </summary>
    [CustomEditor(typeof(ItemActionSet), true)]
    public class ItemActionSetInspector : DatabaseInspectorBase
    {
        protected const string c_ItemCategory = "m_ItemCategory";
        protected const string c_ItemActions = "m_ItemActionCollection";

        protected override List<string> ExcludedFields => new List<string>() { c_ItemCategory, c_ItemActions, "m_ExceptionCategories" };

        protected ItemActionSet m_ItemActionSet;
        private ItemCategoryField m_ItemCategoryField;

        protected ItemCategoryReorderableList m_ExceptionCategories;

        /// <summary>
        /// Initialize the inspector when it is first selected.
        /// </summary>
        protected override void InitializeInspector()
        {
            m_ItemActionSet = target as ItemActionSet;

            if (m_ItemActionSet.ExceptionCategories == null) {
                m_ItemActionSet.ExceptionCategories = new ItemCategory[0];
                return;
            }

            base.InitializeInspector();
        }

        /// <summary>
        /// Create the inspector.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {

            var itemActionCollection = m_ItemActionSet.ItemActionCollection;

            m_ItemCategoryField = new ItemCategoryField(
                "Item Category",
                m_DatabaseField.value as InventorySystemDatabase,
                new (string, Action<ItemCategory>)[]
                {
                    ("Set Category", (x) =>
                    {
                        m_ItemActionSet.ItemCategory = x;
                        m_ItemCategoryField.Refresh(m_ItemActionSet.ItemCategory);
                        Shared.Editor.Utility.EditorUtility.SetDirty(m_ItemActionSet);
                    })
                },
                (x) => true);
            m_ItemCategoryField.Refresh(m_ItemActionSet.ItemCategory);

            container.Add(m_ItemCategoryField);

            m_ExceptionCategories = new ItemCategoryReorderableList("Exception Categories", m_Database, () =>
                {
                    return m_ItemActionSet.ExceptionCategories;
                },
                (value) =>
                {
                    m_ItemActionSet.ExceptionCategories = value;
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_ItemActionSet);
                });
            container.Add(m_ExceptionCategories);

            if (itemActionCollection == null) {
                itemActionCollection = new ItemActionCollection();
                m_ItemActionSet.ItemActionCollection = itemActionCollection;
            }

            var itemActionsField = new ItemActionCollectionField(m_ItemActionSet, itemActionCollection, "Item Actions", null);

            container.Add(itemActionsField);
        }
    }
}