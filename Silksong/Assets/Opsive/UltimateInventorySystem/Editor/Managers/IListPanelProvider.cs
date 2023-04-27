/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers
{
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using System.Collections.Generic;
    using UnityEngine.UIElements;

    /// <summary>
    /// Interface used by the managers to specify that they should provide the content for the ListPanel.
    /// </summary>
    public interface IListPanelProvider<T>
    {
        /// <summary>
        /// Returns the list that the ReorderableList should use.
        /// </summary>
        /// <returns>The list that the ReorderableList should use.</returns>
        IList<T> GetList();

        /// <summary>
        /// Returns a list of options for sorting.
        /// </summary>
        /// <returns>The list of sort options.</returns>
        IList<SortOption> GetSortOptions();

        /// <summary>
        /// Creates the new ReorderableList item.
        /// </summary>
        /// <param name="parent">The parent ReorderableList item.</param>
        /// <param name="index">The index of the item.</param>
        void MakeListItem(VisualElement parent, int index);

        /// <summary>
        /// Binds the ReorderableList item to the specified index.
        /// </summary>
        /// <param name="parent">The ReorderableList item that is being bound.</param>
        /// <param name="index">The index of the item.</param>
        void BindListItem(VisualElement parent, int index);

        /// <summary>
        /// The item at the specified index has been selected.
        /// </summary>
        /// <param name="index">The selected index.</param>
        void OnSelected(int index);

        /// <summary>
        /// Returns true if the name can be added.
        /// </summary>
        /// <param name="name">The desired name.</param>
        /// <returns>True if the name can be added.</returns>
        bool IsObjectNameValidAndUnique(string name);

        /// <summary>
        /// The add button has been pressed.
        /// </summary>
        /// <param name="name">The name of the added item.</param>
        T OnAdd(string name);

        /// <summary>
        /// The remove button has been pressed.
        /// </summary>
        /// <param name="index">The index of the selected category.</param>
        void OnRemove(int index);

        /// <summary>
        /// Search Filter used to filter the list using the search string.
        /// </summary>
        /// <param name="list">The list of elements.</param>
        /// <param name="searchValue">The search string.</param>
        /// <returns>The filtered list.</returns>
        IList<T> GetSearchFilter(IList<T> list, string searchValue);
    }
}