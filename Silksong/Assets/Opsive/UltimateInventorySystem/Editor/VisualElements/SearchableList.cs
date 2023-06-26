/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;


    /// <summary>
    /// Searchable List allowing for searching and sorting a ReorderableList.
    /// </summary>
    public class SearchableList<T> : VisualElement where T : class
    {
        protected IList<T> m_ItemSource;
        protected List<T> m_ItemSourceCopy;
        protected T m_PreviousSelectedObject;
        protected string m_PreviousSearch;
        protected SortOption m_PreviousSortOption;
        protected Action<int> m_OnSelection;

        protected ObjectField m_FilterPresetField;
        protected ToolbarSearchField m_SearchField;
        protected SortOptionsDropDown m_SortOptionsDropDown;
        protected ReorderableList m_ReorderableList;

        protected IList<SortOption> m_SortOptions;
        protected Func<IList<T>, string, IList<T>> m_SearchFilter;

        public List<T> ItemList => m_ItemSourceCopy;

        public bool selectOnRefresh = true;

        public int SelectedIndex {
            get => m_ReorderableList.SelectedIndex;
            set => m_ReorderableList.SelectedIndex = value;
        }

        public T SelectedObject {
            get => SelectedIndex < 0 || SelectedIndex >= m_ItemSourceCopy.Count ? null : m_ItemSourceCopy[SelectedIndex];
            set => SelectObject(value);
        }

        public ObjectField FilterPresetField => m_FilterPresetField;
        
        private static string SearchableListFilterPresetKey => "Opsive.SearchableList.FilterPreset."+typeof(T) +"."+ Application.productName;

        // The database path is based on the project.
        private static SearchableListFilterPreset StoredSearchableListFilterPreset
        {
            get
            {
                var guid = EditorPrefs.GetString(SearchableListFilterPresetKey, string.Empty);
                if (!string.IsNullOrEmpty(guid)) {
                    return Shared.Editor.Utility.EditorUtility.LoadAsset<SearchableListFilterPreset>(guid);
                }

                return null;
            }
            set
            {
                if (value != null) {
                    EditorPrefs.SetString(SearchableListFilterPresetKey, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(value)));
                } else {
                    EditorPrefs.SetString(SearchableListFilterPresetKey, string.Empty);
                }
            }
        }

        /// <summary>
        /// SearchableList constructor.
        /// </summary>
        /// <param name="itemsSource">The list of items.</param>
        /// <param name="makeItem">The callback when each element is being created.</param>
        /// <param name="bindItem">the callback when the element is binding to the data.</param>
        /// <param name="header">The list header (can be null).</param>
        /// <param name="onSelection">The callback when an element is selected (can be null).</param>
        /// <param name="onAdd">The callback when the add button is pressed (can be null).</param>
        /// <param name="onRemove">The callback when the remove button is pressed (can be null).</param>
        /// <param name="onReorder">The callback when elements are reordered.</param>
        /// <param name="sortOptions">Options allowing for sorting.</param>
        /// <param name="searchFilter">The filter function.</param>
        public SearchableList(IList<T> itemsSource, Action<VisualElement, int> makeItem,
            Action<VisualElement, int> bindItem, Action<VisualElement> header,
            Action<int> onSelection, Action onAdd, Action<int> onRemove,
            Action<int, int> onReorder,
            IList<SortOption> sortOptions,
            Func<IList<T>, string, IList<T>> searchFilter)
        {
            m_SearchFilter = searchFilter;
            m_SortOptions = sortOptions;
            m_ItemSource = itemsSource;
            m_ItemSourceCopy = new List<T>();
            if (m_ItemSource != null) {
                for (int i = 0; i < m_ItemSource.Count; i++) { m_ItemSourceCopy.Add(m_ItemSource[i]); }
            }

            m_OnSelection = onSelection;

            AddToClassList(CommonStyles.s_SearchList);

            var filterPresetContainer = new VisualElement();
            filterPresetContainer.AddToClassList(CommonStyles.s_SearchList_FilterPresetContainer);
            
            m_FilterPresetField = new ObjectField();
            m_FilterPresetField.objectType = typeof(SearchableListFilterPreset);
            m_FilterPresetField.RegisterValueChangedCallback(evt =>
            {
                var filterPreset = evt.newValue as SearchableListFilterPreset;
                SetFilterPreset(filterPreset);
            });
            

            var newFilterPresetButton = new Button();
            newFilterPresetButton.text = "+";
            newFilterPresetButton.clicked += () =>
            {
                var fileName = m_SearchField.value.Replace(':', '_');
                var path = EditorUtility.SaveFilePanel("Create a SearchableListFilterPreset", "Assets", "FilterPreset_"+fileName, "asset");
                if (path.Length != 0 && Application.dataPath.Length < path.Length) {
                    var obj = ScriptableObject.CreateInstance<SearchableListFilterPreset>();
                    
                    obj.SearchString = m_SearchField.value;
                    obj.SortOptionIndex = m_SortOptionsDropDown.index;
                    
                    // Save the object asset.
                    path = string.Format("Assets/{0}", path.Substring(Application.dataPath.Length + 1));
                    AssetDatabase.DeleteAsset(path);
                    AssetDatabase.CreateAsset(obj, path);
                    AssetDatabase.ImportAsset(path);

                    m_FilterPresetField.value = obj;
                }
            };
            filterPresetContainer.Add(m_FilterPresetField);
            filterPresetContainer.Add(newFilterPresetButton);
           
            
            var searchSortContainer = new VisualElement();
            searchSortContainer.AddToClassList(CommonStyles.s_SearchList_SearchSortContainer);
            m_SearchField = new ToolbarSearchField();
            m_SearchField.style.flexShrink = 1;
            searchSortContainer.Add(m_SearchField);
            m_SearchField.RegisterValueChangedCallback(evt =>
            {
                SearchAndSort(evt.newValue);
            });

            m_SortOptionsDropDown = new SortOptionsDropDown(sortOptions);
            m_SortOptionsDropDown.RegisterValueChangedCallback(evt =>
            {
                SortList();
                SearchSortRefresh(m_PreviousSearch);
            });
            searchSortContainer.Add(m_SortOptionsDropDown);

            m_ReorderableList = new ReorderableList(m_ItemSourceCopy, makeItem, bindItem, header,
                index =>
                {
                    m_OnSelection?.Invoke(index);
                    m_PreviousSelectedObject = (index < 0 || index >= m_ItemSourceCopy.Count) ? null : m_ItemSourceCopy[index];
                }, onAdd, onRemove, onReorder);
            //TODO review, I removed this because it was causing issues refreshing while the object was being constructed.
            //m_ReorderableList.Refresh(m_ItemSourceCopy);
            Add(m_ReorderableList);

            //m_ReorderableList.Add(filterPresetContainer);
            //filterPresetContainer.PlaceBehind(m_ReorderableList.Q("body"));
            
            searchSortContainer.Add(filterPresetContainer);
            
            m_ReorderableList.Add(searchSortContainer);
            searchSortContainer.PlaceBehind(m_ReorderableList.Q("body"));

            SetFilterPreset(StoredSearchableListFilterPreset);
            
            //TODO review, I removed this because it was causing issues refreshing while the object was being constructed.
            //Refresh();
        }

        private void SetFilterPreset(SearchableListFilterPreset filterPreset)
        {
            m_FilterPresetField.SetValueWithoutNotify(filterPreset);
            
            if (filterPreset != null) {
                m_SearchField.value = filterPreset.SearchString;
                if (m_SortOptions.Count > 0) {
                    var sortOptionIndex = filterPreset.SortOptionIndex < 0 ? 0 :
                        filterPreset.SortOptionIndex >= m_SortOptions.Count ? m_SortOptions.Count - 1 :
                        filterPreset.SortOptionIndex;
                    m_SortOptionsDropDown.value = m_SortOptions[sortOptionIndex];
                }
            }

            StoredSearchableListFilterPreset = filterPreset;
            SearchSortRefresh(m_PreviousSearch);
        }

        /// <summary>
        /// Clears the search and refreshes.
        /// </summary>
        public void ClearSearch(bool usePreset)
        {
            var FilterPreset = m_FilterPresetField.value as SearchableListFilterPreset;
            if (!usePreset || FilterPreset == null) {
                m_SearchField.SetValueWithoutNotify((string)null);
            } else {
                m_SearchField.SetValueWithoutNotify(FilterPreset.SearchString);
            }
            
            SearchAndSort(null);
        }

        /// <summary>
        /// Search and sort, the order is important since we only sort the filtered list.
        /// </summary>
        /// <param name="searchString">The search text.</param>
        public void SearchAndSort(string searchString)
        {
            m_ItemSourceCopy.Clear();
            Search(searchString);
            SortList();

            SearchSortRefresh(searchString);
        }

        /// <summary>
        /// Refresh but also select the object that was selected before the search sort.
        /// </summary>
        /// <param name="searchString">The search text.</param>
        private void SearchSortRefresh(string searchString)
        {
            //TODO review, I'm removing this because it's causing issues.
            //m_ReorderableList.ScrollView.scrollOffset = Vector2.zero;
        
            var previousSelectedObject = m_PreviousSelectedObject;
            var previousSelectedIndex = m_ReorderableList.SelectedIndex;

            if (selectOnRefresh == false) { m_ReorderableList.SelectedIndex = -1; }

            m_ReorderableList.Refresh(m_ItemSourceCopy);

            if (selectOnRefresh) {
                //Select the object that was selected before the search change if possible.
                if (m_PreviousSearch != searchString || m_PreviousSortOption != m_SortOptionsDropDown.CurrentOption) {
                    if (m_ItemSourceCopy.Contains(previousSelectedObject)) {
                        SelectObject(previousSelectedObject);
                    } else if (m_ItemSourceCopy != null && previousSelectedIndex != -1) {

                        if (m_ItemSourceCopy.Count <= 0) {
                            m_ReorderableList.SelectedIndex = -1;
                            m_OnSelection?.Invoke(-1);
                        } else if (previousSelectedObject != m_ItemSourceCopy[0]) {
                            m_ReorderableList.SelectedIndex = -1;
                            m_OnSelection?.Invoke(-1);
                            m_PreviousSelectedObject = m_ItemSourceCopy[0];
                            m_ReorderableList.SelectedIndex = 0;
                        }
                    }
                } else if (previousSelectedIndex >= m_ItemSourceCopy.Count) {
                    m_ReorderableList.SelectedIndex = m_ItemSourceCopy.Count - 1;
                }
            }

            m_PreviousSearch = searchString;
            m_PreviousSortOption = m_SortOptionsDropDown.CurrentOption;
        }

        /// <summary>
        /// Searches the list.
        /// </summary>
        /// <param name="searchString">The search text.</param>
        protected void Search(string searchString)
        {
            SearchFilter(searchString, m_ItemSourceCopy, m_ItemSource, m_SearchFilter);
        }

        /// <summary>
        /// Sort the sublist.
        /// </summary>
        public void SortList()
        {
            m_SortOptionsDropDown.CurrentOption.Sort?.Invoke(m_ItemSourceCopy);
        }

        /// <summary>
        /// Refresh after assigning a new itemSource.
        /// </summary>
        /// <param name="itemSource">The item source.</param>
        public void Refresh(IList<T> itemSource)
        {
            m_ItemSource = itemSource;
            Refresh();
        }

        /// <summary>
        /// Refresh the list.
        /// </summary>
        public void Refresh()
        {
            var previousSelected = SelectedObject;
            SearchAndSort(m_SearchField.value);
            SelectObject(previousSelected);
        }

        /// <summary>
        /// Select an object in the list.
        /// </summary>
        /// <param name="obj">The object to select.</param>
        public virtual void SelectObject(T obj)
        {
            if (obj == null) { return; }

            var index = -1;

            for (int i = 0; i < m_ItemSourceCopy.Count; i++) {
                if (ReferenceEquals(obj, m_ItemSourceCopy[i]) == false) { continue; }

                index = i;
                break;
            }

            m_ReorderableList.HighlightSelectedItem = true;
            if (index == -1 || (index == SelectedIndex && obj == SelectedObject)) { return; }
            m_ReorderableList.SelectedIndex = index;
        }

        /// <summary>
        /// Function used to filter a list. Used by the search options
        /// </summary>
        /// <param name="searchValue">The search string.</param>
        /// <param name="subSource">The filtered list.</param>
        /// <param name="fullSource">The full lists of objects.</param>
        /// <param name="searchFilter">The filter function.</param>
        protected void SearchFilter(string searchValue, IList subSource, IList<T> fullSource, Func<IList<T>, string, IList<T>> searchFilter)
        {
            if (string.IsNullOrWhiteSpace(searchValue) == false && searchFilter != null) {
                var filteredList = searchFilter.Invoke(fullSource, searchValue);
                FilterWithPreset(subSource, filteredList, m_FilterPresetField.value as SearchableListFilterPreset);
            } else if (fullSource != null) {
                FilterWithPreset(subSource, fullSource, m_FilterPresetField.value as SearchableListFilterPreset);
            }
        }

        /// <summary>
        /// Function used to filter a list using the preset.
        /// </summary>
        /// <param name="subSource">he filtered list.</param>
        /// <param name="list">The full lists of objects.</param>
        /// <param name="filterPreset">The filter preset object.</param>
        protected void FilterWithPreset(IList subSource, IList<T> list, SearchableListFilterPreset filterPreset)
        {
            if (filterPreset == null) {
                for (int i = 0; i < list.Count; ++i) {
                    subSource.Add(list[i]);
                }
            } else {
                for (int i = 0; i < list.Count; ++i) {
                    if (filterPreset.IsValid(list[i])) {
                        subSource.Add(list[i]);
                    }
                }
            }
            
        }

        /// <summary>
        /// Focus the search field
        /// </summary>
        public void FocusSearchField()
        {
            m_SearchField.Focus();
            m_SearchField.Q("unity-text-input").Focus();
        }
    }
}
