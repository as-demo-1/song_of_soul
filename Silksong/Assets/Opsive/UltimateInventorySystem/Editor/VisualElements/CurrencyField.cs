/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements
{
    using Opsive.UltimateInventorySystem.Editor.Utility;
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames;
    using Opsive.UltimateInventorySystem.Exchange;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using System.Collections.Generic;
    using UnityEngine.UIElements;

    /// <summary>
    /// The currency field.
    /// </summary>
    public class CurrencyField : InventoryObjectField<Currency>
    {
        private CurrencySearchableListWindow m_CurrencySearchableListWindow;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="inventorySystemDatabase">The inventory system database.</param>
        /// <param name="mainManagerWindow">The main manager window used to position the popup.</param>
        /// <param name="actions">The actions that can be performed on the selected object.</param>
        /// <param name="preFilterCondition">Use a condition to prefilter the source list.</param>
        public CurrencyField(string label,
            InventorySystemDatabase inventorySystemDatabase,
            IList<(string, Action<Currency>)> actions,
            Func<Currency, bool> preFilterCondition)
            : base(label, inventorySystemDatabase, actions, preFilterCondition)
        {
        }
        
        public override InventoryObjectSearchableListWindow<Currency> CreateSearchableListWindow(IList<(string, Action<Currency>)> actions, Func<Currency, bool> preFilterCondition)
        {
            m_CurrencySearchableListWindow =
                new CurrencySearchableListWindow(m_InventorySystemDatabase, actions, preFilterCondition, true);
            return m_CurrencySearchableListWindow;
        }

        /// <summary>
        /// Make the field view name.
        /// </summary>
        /// <returns>The new field view name.</returns>
        protected override ViewName<Currency> MakeFieldViewName()
        {
            return new CurrencyViewName();
        }
    }
    
    /// <summary>
    /// The Searchable list.
    /// </summary>
    public class CurrencySearchableListWindow : InventoryObjectSearchableListWindow<Currency>
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inventorySystemDatabase">The inventory system database.</param>
        /// <param name="actions">The actions that can be performed on the selected object.</param>
        /// <param name="preFilterCondition">Use a condition to prefilter the source list.</param>
        public CurrencySearchableListWindow(
            InventorySystemDatabase inventorySystemDatabase,
            IList<(string, Action<Currency>)> actions,
            Func<Currency, bool> preFilterCondition, bool closeOnActionComplete )
            : base(inventorySystemDatabase, actions, preFilterCondition, closeOnActionComplete)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inventorySystemDatabase">The inventory system database.</param>
        /// <param name="action">The action that can be performed on the selected object.</param>
        /// <param name="preFilterCondition">Use a condition to prefilter the source list.</param>
        public CurrencySearchableListWindow(
            InventorySystemDatabase inventorySystemDatabase,
            Action<Currency> action,
            Func<Currency, bool> preFilterCondition,
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
            var viewName = parent.ElementAt(0) as CurrencyViewName;
            var Currency = m_SearchableList.ItemList[index];
            viewName.Refresh(Currency);
        }

        /// <summary>
        /// Make the list item.
        /// </summary>
        /// <param name="parent">The parent visual element.</param>
        /// <param name="index">The index.</param>
        protected override void MakeItem(VisualElement parent, int index)
        {
            var viewName = new CurrencyViewName();
            parent.Add(viewName);
        }

        /// <summary>
        /// Return the source of the list.
        /// </summary>
        /// <returns>The list source.</returns>
        protected override IList<Currency> GetSourceInternal()
        {
            return m_InventorySystemDatabase?.Currencies;
        }

        /// <summary>
        /// The sort options.
        /// </summary>
        /// <returns>The sort options.</returns>
        protected override IList<SortOption> GetSortOptions()
        {
            return CurrencyEditorUtility.SortOptions();
        }

        /// <summary>
        /// Filter options for the search list.
        /// </summary>
        /// <param name="list">The list source.</param>
        /// <param name="searchValue">The search value.</param>
        /// <returns>The filtered list.</returns>
        protected override IList<Currency> FilterOptions(IList<Currency> list, string searchValue)
        {
            return CurrencyEditorUtility.SearchFilter(list, searchValue);
        }

    }
}