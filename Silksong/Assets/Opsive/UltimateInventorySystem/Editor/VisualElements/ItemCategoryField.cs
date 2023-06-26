/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Editor.Inspectors;
    using Opsive.UltimateInventorySystem.Editor.Utility;
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using System.Collections.Generic;
    using UnityEngine.UIElements;

    /// <summary>
    /// The field used to select an item category.
    /// </summary>
    public class ItemCategoryField : InventoryObjectField<ItemCategory>
    {
        private ItemCategorySearchableListWindow m_ItemCategorySearchableListWindow;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="inventorySystemDatabase">The inventory system database.</param>
        /// <param name="mainManagerWindow">The main manager window used to position the popup.</param>
        /// <param name="actions">The actions that can be performed on the selected object.</param>
        /// <param name="preFilterCondition">Use a condition to prefilter the source list.</param>
        public ItemCategoryField(string label,
            InventorySystemDatabase inventorySystemDatabase,
            IList<(string, Action<ItemCategory>)> actions,
            Func<ItemCategory, bool> preFilterCondition)
            : base(label, inventorySystemDatabase, actions, preFilterCondition)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="inventorySystemDatabase">The inventory system database.</param>
        /// <param name="mainManagerWindow">The main manager window used to position the popup.</param>
        /// <param name="action">The action that can be performed on the selected object.</param>
        /// <param name="preFilterCondition">Use a condition to prefilter the source list.</param>
        public ItemCategoryField(string label,
            InventorySystemDatabase inventorySystemDatabase,
            Action<ItemCategory> action,
            Func<ItemCategory, bool> preFilterCondition) : base(label, inventorySystemDatabase, action, preFilterCondition)
        {
        }


        public override InventoryObjectSearchableListWindow<ItemCategory> CreateSearchableListWindow(IList<(string, Action<ItemCategory>)> actions, Func<ItemCategory, bool> preFilterCondition)
        {
            m_ItemCategorySearchableListWindow = new ItemCategorySearchableListWindow(m_InventorySystemDatabase, actions, preFilterCondition, true);
            return m_ItemCategorySearchableListWindow;
        }

        /// <summary>
        /// Make the field view name.
        /// </summary>
        /// <returns>The new field view name.</returns>
        protected override ViewName<ItemCategory> MakeFieldViewName()
        {
            return new ItemCategoryViewName();
        }

    }

    public class ItemCategoryReorderableList : InventoryObjectReorderableList<ItemCategory>
    {

        // <summary>
        public ItemCategoryReorderableList(string title, InventorySystemDatabase database,
            Func<ItemCategory[]> getObject, Action<ItemCategory[]> setObjects) :
            base(title, getObject, setObjects, () => new ItemCategoryListElement(database))
        {
            SearchableListWindow = new ItemCategorySearchableListWindow(database, AddObjectToList, null, false);
        }

        /// The list element for the tabs.
        /// </summary>
        public class ItemCategoryListElement : InventoryObjectListElement<ItemCategory>
        {
            protected ItemCategoryField m_CategoryObjectField;

            /// <summary>
            /// The list element.
            /// </summary>
            public ItemCategoryListElement(InventorySystemDatabase database) : base(database)
            {
                m_CategoryObjectField = new ItemCategoryField(
                    "",
                    database, new (string, Action<ItemCategory>)[]
                    {
                        ("Set Category", (x) =>
                        {
                            HandleValueChanged(x);
                        })
                    },
                    (x) => true);
                Add(m_CategoryObjectField);
            }

            /// <summary>
            /// Update the visuals.
            /// </summary>
            /// <param name="value">The new value.</param>
            public override void Refresh(ItemCategory value)
            {
                m_CategoryObjectField.Refresh(value);
            }
        }
    }
    
    /// <summary>
    /// The item definition field.
    /// </summary>
    public class ItemCategorySearchableListWindow : InventoryObjectSearchableListWindow<ItemCategory>
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inventorySystemDatabase">The inventory system database.</param>
        /// <param name="actions">The actions that can be performed on the selected object.</param>
        /// <param name="preFilterCondition">Use a condition to prefilter the source list.</param>
        public ItemCategorySearchableListWindow(
            InventorySystemDatabase inventorySystemDatabase,
            IList<(string, Action<ItemCategory>)> actions,
            Func<ItemCategory, bool> preFilterCondition, bool closeOnActionComplete )
            : base(inventorySystemDatabase, actions, preFilterCondition, closeOnActionComplete)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inventorySystemDatabase">The inventory system database.</param>
        /// <param name="action">The action that can be performed on the selected object.</param>
        /// <param name="preFilterCondition">Use a condition to prefilter the source list.</param>
        public ItemCategorySearchableListWindow(
            InventorySystemDatabase inventorySystemDatabase,
            Action<ItemCategory> action,
            Func<ItemCategory, bool> preFilterCondition,
            bool closeOnActionComplete ) : base(inventorySystemDatabase, action, preFilterCondition, closeOnActionComplete)
        {
        }

        /// <summary>
        /// Bind the list item.
        /// </summary>
        /// <param name="parent">The parent visual element.</param>
        /// <param name="index">The index.</param>
        protected override void BindItem(VisualElement parent, int index)
        {
            if (index < 0 || index >= m_SearchableList.ItemList.Count) {
                return;
            }
            var viewName = parent.ElementAt(0) as ItemCategoryViewName;
            var ItemCategory = m_SearchableList.ItemList[index];
            viewName.Refresh(ItemCategory);
        }

        /// <summary>
        /// Make the list item.
        /// </summary>
        /// <param name="parent">The parent visual element.</param>
        /// <param name="index">The index.</param>
        protected override void MakeItem(VisualElement parent, int index)
        {
            var viewName = new ItemCategoryViewName();
            parent.Add(viewName);
        }

        /// <summary>
        /// Return the source of the list.
        /// </summary>
        /// <returns>The list source.</returns>
        protected override IList<ItemCategory> GetSourceInternal()
        {
            return m_InventorySystemDatabase?.ItemCategories;
        }

        /// <summary>
        /// The sort options.
        /// </summary>
        /// <returns>The sort options.</returns>
        protected override IList<SortOption> GetSortOptions()
        {
            return ItemCategoryEditorUtility.SortOptions();
        }

        /// <summary>
        /// Filter options for the search list.
        /// </summary>
        /// <param name="list">The list source.</param>
        /// <param name="searchValue">The search value.</param>
        /// <returns>The filtered list.</returns>
        protected override IList<ItemCategory> FilterOptions(IList<ItemCategory> list, string searchValue)
        {
            return ItemCategoryEditorUtility.SearchFilter(list, searchValue);
        }

    }
}