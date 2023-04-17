/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Editor.Utility;
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using System.Collections.Generic;
    using Opsive.UltimateInventorySystem.Editor.Inspectors;
    using UnityEngine.UIElements;

    /// <summary>
    /// The item definition field.
    /// </summary>
    public class ItemDefinitionField : InventoryObjectField<ItemDefinition>
    {
        private ItemDefinitionSearchableListWindow m_ItemDefinitionSearchableListWindow;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="inventorySystemDatabase">The inventory system database.</param>
        /// <param name="mainManagerWindow">The main manager window used to position the popup.</param>
        /// <param name="actions">The actions that can be performed on the selected object.</param>
        /// <param name="preFilterCondition">Use a condition to prefilter the source list.</param>
        public ItemDefinitionField(string label,
            InventorySystemDatabase inventorySystemDatabase,
            IList<(string, Action<ItemDefinition>)> actions,
            Func<ItemDefinition, bool> preFilterCondition)
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
        public ItemDefinitionField(string label,
            InventorySystemDatabase inventorySystemDatabase,
            Action<ItemDefinition> action,
            Func<ItemDefinition, bool> preFilterCondition) : base(label, inventorySystemDatabase, action, preFilterCondition)
        {
        }

        public override InventoryObjectSearchableListWindow<ItemDefinition> CreateSearchableListWindow(IList<(string, Action<ItemDefinition>)> actions, Func<ItemDefinition, bool> preFilterCondition)
        {
            m_ItemDefinitionSearchableListWindow =
                new ItemDefinitionSearchableListWindow(m_InventorySystemDatabase, actions, preFilterCondition, true);
            return m_ItemDefinitionSearchableListWindow;
        }

        /// <summary>
        /// Make the field view name.
        /// </summary>
        /// <returns>The new field view name.</returns>
        protected override ViewName<ItemDefinition> MakeFieldViewName()
        {
            return new ItemDefinitionViewName();
        }
    }
    
    public class ItemDefinitionReorderableList : InventoryObjectReorderableList<ItemDefinition>
    {

        // <summary>
        public ItemDefinitionReorderableList(string title, InventorySystemDatabase database,
            Func<ItemDefinition[]> getObject, Action<ItemDefinition[]> setObjects) :
            base(title, getObject, setObjects, () => new ItemDefinitionListElement(database))
        {
            SearchableListWindow = new ItemDefinitionSearchableListWindow(database, AddObjectToList, null, false);
        }

        /// The list element for the tabs.
        /// </summary>
        public class ItemDefinitionListElement : InventoryObjectListElement<ItemDefinition>
        {
            protected ItemDefinitionField m_DefinitionObjectField;

            /// <summary>
            /// The list element.
            /// </summary>
            public ItemDefinitionListElement(InventorySystemDatabase database) : base(database)
            {
                m_DefinitionObjectField = new ItemDefinitionField(
                    "",
                    database, new (string, Action<ItemDefinition>)[]
                    {
                        ("Set Definition", (x) =>
                        {
                            HandleValueChanged(x);
                        })
                    },
                    (x) => true);
                Add(m_DefinitionObjectField);
            }

            /// <summary>
            /// Update the visuals.
            /// </summary>
            /// <param name="value">The new value.</param>
            public override void Refresh(ItemDefinition value)
            {
                m_DefinitionObjectField.Refresh(value);
            }
        }
    }
    
    /// <summary>
    /// The Searchable list.
    /// </summary>
    public class ItemDefinitionSearchableListWindow : InventoryObjectSearchableListWindow<ItemDefinition>
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inventorySystemDatabase">The inventory system database.</param>
        /// <param name="actions">The actions that can be performed on the selected object.</param>
        /// <param name="preFilterCondition">Use a condition to prefilter the source list.</param>
        public ItemDefinitionSearchableListWindow(
            InventorySystemDatabase inventorySystemDatabase,
            IList<(string, Action<ItemDefinition>)> actions,
            Func<ItemDefinition, bool> preFilterCondition, bool closeOnActionComplete )
            : base(inventorySystemDatabase, actions, preFilterCondition, closeOnActionComplete)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inventorySystemDatabase">The inventory system database.</param>
        /// <param name="action">The action that can be performed on the selected object.</param>
        /// <param name="preFilterCondition">Use a condition to prefilter the source list.</param>
        public ItemDefinitionSearchableListWindow(
            InventorySystemDatabase inventorySystemDatabase,
            Action<ItemDefinition> action,
            Func<ItemDefinition, bool> preFilterCondition,
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
            var viewName = parent.ElementAt(0) as ItemDefinitionViewName;
            var itemDefinition = m_SearchableList.ItemList[index];
            viewName.Refresh(itemDefinition);
        }

        /// <summary>
        /// Make the list item.
        /// </summary>
        /// <param name="parent">The parent visual element.</param>
        /// <param name="index">The index.</param>
        protected override void MakeItem(VisualElement parent, int index)
        {
            var viewName = new ItemDefinitionViewName();
            parent.Add(viewName);
        }

        /// <summary>
        /// Return the source of the list.
        /// </summary>
        /// <returns>The list source.</returns>
        protected override IList<ItemDefinition> GetSourceInternal()
        {
            return m_InventorySystemDatabase?.ItemDefinitions;
        }

        /// <summary>
        /// The sort options.
        /// </summary>
        /// <returns>The sort options.</returns>
        protected override IList<SortOption> GetSortOptions()
        {
            return ItemDefinitionEditorUtility.SortOptions();
        }

        /// <summary>
        /// Filter options for the search list.
        /// </summary>
        /// <param name="list">The list source.</param>
        /// <param name="searchValue">The search value.</param>
        /// <returns>The filtered list.</returns>
        protected override IList<ItemDefinition> FilterOptions(IList<ItemDefinition> list, string searchValue)
        {
            return ItemDefinitionEditorUtility.SearchFilter(list, searchValue);
        }

    }
}