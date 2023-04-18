/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements
{
    using Opsive.UltimateInventorySystem.Crafting;
    using Opsive.UltimateInventorySystem.Editor.Inspectors;
    using Opsive.UltimateInventorySystem.Editor.Utility;
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using System.Collections.Generic;
    using UnityEngine.UIElements;

    /// <summary>
    /// The crafting category field.
    /// </summary>
    public class CraftingRecipeField : InventoryObjectField<CraftingRecipe>
    {
        private CraftingRecipeSearchableListWindow m_CraftingRecipeSearchableListWindow;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="inventorySystemDatabase">The inventory system database.</param>
        /// <param name="actions">The actions that can be performed on the selected object.</param>
        /// <param name="preFilterCondition">Use a condition to prefilter the source list.</param>
        public CraftingRecipeField(string label,
            InventorySystemDatabase inventorySystemDatabase,
            IList<(string, Action<CraftingRecipe>)> actions,
            Func<CraftingRecipe, bool> preFilterCondition)
            : base(label, inventorySystemDatabase, actions, preFilterCondition)
        {
        }

        public override InventoryObjectSearchableListWindow<CraftingRecipe> CreateSearchableListWindow(IList<(string, Action<CraftingRecipe>)> actions, Func<CraftingRecipe, bool> preFilterCondition)
        {
            m_CraftingRecipeSearchableListWindow =
                new CraftingRecipeSearchableListWindow(m_InventorySystemDatabase, actions, preFilterCondition, true);
            return m_CraftingRecipeSearchableListWindow;
        }

        /// <summary>
        /// Make the field view name.
        /// </summary>
        /// <returns>The new field view name.</returns>
        protected override ViewName<CraftingRecipe> MakeFieldViewName()
        {
            return new CraftingRecipeViewName();
        }
        
    }

    public class CraftingRecipeReorderableList : InventoryObjectReorderableList<CraftingRecipe>
    {

        // <summary>
        public CraftingRecipeReorderableList(string title, InventorySystemDatabase database,
            Func<CraftingRecipe[]> getObject, Action<CraftingRecipe[]> setObjects) :
            base(title, getObject, setObjects, () => new RecipeListElement(database))
        {
            SearchableListWindow = new CraftingRecipeSearchableListWindow(database, AddObjectToList, null, false);
        }

        /// The list element for the tabs.
        /// </summary>
        public class RecipeListElement : InventoryObjectListElement<CraftingRecipe>
        {
            protected CraftingRecipeField m_ObjectField;

            /// <summary>
            /// The list element.
            /// </summary>
            public RecipeListElement(InventorySystemDatabase database) : base(database)
            {
                m_ObjectField = new CraftingRecipeField(
                    "",
                    database, new (string, Action<CraftingRecipe>)[]
                    {
                        ("Set Category", (x) =>
                        {
                            HandleValueChanged(x);
                        })
                    },
                    (x) => true);
                Add(m_ObjectField);
            }

            /// <summary>
            /// Update the visuals.
            /// </summary>
            /// <param name="value">The new value.</param>
            public override void Refresh(CraftingRecipe value)
            {
                m_ObjectField.Refresh(value);
            }
        }
    }
    
    /// <summary>
    /// The Searchable list.
    /// </summary>
    public class CraftingRecipeSearchableListWindow : InventoryObjectSearchableListWindow<CraftingRecipe>
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inventorySystemDatabase">The inventory system database.</param>
        /// <param name="actions">The actions that can be performed on the selected object.</param>
        /// <param name="preFilterCondition">Use a condition to prefilter the source list.</param>
        public CraftingRecipeSearchableListWindow(
            InventorySystemDatabase inventorySystemDatabase,
            IList<(string, Action<CraftingRecipe>)> actions,
            Func<CraftingRecipe, bool> preFilterCondition, bool closeOnActionComplete )
            : base(inventorySystemDatabase, actions, preFilterCondition, closeOnActionComplete)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inventorySystemDatabase">The inventory system database.</param>
        /// <param name="action">The action that can be performed on the selected object.</param>
        /// <param name="preFilterCondition">Use a condition to prefilter the source list.</param>
        public CraftingRecipeSearchableListWindow(
            InventorySystemDatabase inventorySystemDatabase,
            Action<CraftingRecipe> action,
            Func<CraftingRecipe, bool> preFilterCondition,
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
            var viewName = parent.ElementAt(0) as CraftingRecipeViewName;
            var CraftingRecipe = m_SearchableList.ItemList[index];
            viewName.Refresh(CraftingRecipe);
        }

        /// <summary>
        /// Make the list item.
        /// </summary>
        /// <param name="parent">The parent visual element.</param>
        /// <param name="index">The index.</param>
        protected override void MakeItem(VisualElement parent, int index)
        {
            var viewName = new CraftingRecipeViewName();
            parent.Add(viewName);
        }

        /// <summary>
        /// Return the source of the list.
        /// </summary>
        /// <returns>The list source.</returns>
        protected override IList<CraftingRecipe> GetSourceInternal()
        {
            return m_InventorySystemDatabase?.CraftingRecipes;
        }

        /// <summary>
        /// The sort options.
        /// </summary>
        /// <returns>The sort options.</returns>
        protected override IList<SortOption> GetSortOptions()
        {
            return CraftingRecipeEditorUtility.SortOptions();
        }

        /// <summary>
        /// Filter options for the search list.
        /// </summary>
        /// <param name="list">The list source.</param>
        /// <param name="searchValue">The search value.</param>
        /// <returns>The filtered list.</returns>
        protected override IList<CraftingRecipe> FilterOptions(IList<CraftingRecipe> list, string searchValue)
        {
            return CraftingRecipeEditorUtility.SearchFilter(list, searchValue);
        }

    }
}