/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Managers;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using System.Collections.Generic;
    using Opsive.UltimateInventorySystem.Editor.Utility.InventoryDatabaseImportExport;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// base manager for any of the inventory system objects, such as ItemCategory, ItemDefinition, etc.
    /// </summary>
    /// <typeparam name="T">The Object type.</typeparam>
    [System.Serializable]
    public abstract class InventorySystemObjectBaseManager<T> : InventoryManager, IListPanelProvider<T> where T : UnityEngine.Object
    {
        protected ListPanel<T> m_ListPanel;
        protected ScrollView m_ContentPanel;
        protected UnicodeTextField m_Name;

        [SerializeField] protected int m_SelectedIndex = -1;
        public T SelectedObject => SelectedIndexOutOfRange ? null : m_ListPanel.SearchableList.ItemList[m_SelectedIndex];

        public bool SelectedIndexOutOfRange =>
            m_SelectedIndex < 0 || m_SelectedIndex >= m_ListPanel.SearchableList.ItemList.Count;

        /// <summary>
        /// Adds the visual elements to the ManagerContentContainer visual element. 
        /// </summary>
        public override void BuildVisualElements()
        {
            ManagerContentContainer.Clear();

            var horizontalLayout = new VisualElement();
            horizontalLayout.AddToClassList("horizontal-layout");
            ManagerContentContainer.Add(horizontalLayout);

            m_ListPanel = new ListPanel<T>(this);
            horizontalLayout.Add(m_ListPanel);

            m_ContentPanel = new ScrollView(ScrollViewMode.Vertical);
            m_ContentPanel.name = "contentPanel";
            m_ContentPanel.AddToClassList(CommonStyles.s_VerticalLayout);
#if UNITY_2021_1_OR_NEWER
            m_ContentPanel.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
#else
            m_ContentPanel.showVertical = true;
#endif
            horizontalLayout.Add(m_ContentPanel);

            m_Name = new UnicodeTextField("Name");
            m_Name.tooltip = "The object name";
            m_Name.isDelayed = true;
            m_Name.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                // The new name must be unique.
                if (!IsObjectNameValidAndUnique(evt.newValue)) {
                    m_Name.SetValueWithoutNotify(evt.previousValue);
                    return;
                }

                // The name is unique. Update the name and set the object to dirty.
                var obj = SelectedObject;
                Undo.RegisterCompleteObjectUndo(obj, "Rename Object");
                obj.name = evt.newValue;
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(obj), obj.name);
                Shared.Editor.Utility.EditorUtility.SetDirty(obj);

                Refresh();
                m_ListPanel.SearchableList.SelectObject(obj);
            });
            m_ContentPanel.Add(m_Name);

            // The content will become visible when the object is selected.
            m_ContentPanel.style.display = DisplayStyle.None;
        }

        public abstract bool IsObjectNameValidAndUnique(string name);
        public abstract IList<T> GetList();
        public abstract IList<SortOption> GetSortOptions();
        public abstract void MakeListItem(VisualElement parent, int index);
        public abstract void BindListItem(VisualElement parent, int index);

        /// <summary>
        /// The item at the specified index has been selected.
        /// </summary>
        /// <param name="index">The selected index.</param>
        public virtual void OnSelected(int index)
        {
            if (m_ContentPanel == null) { return; }

            m_SelectedIndex = index;
            m_ContentPanel.style.display = m_SelectedIndex != -1 ? DisplayStyle.Flex : DisplayStyle.None;
            OnManagerChange();

            // No more work needs to be performed if the category is empty.
            if (SelectedIndexOutOfRange) {
                m_SelectedIndex = 0;
            }

            if (SelectedObject != null) { UpdateElements(SelectedObject); }

            m_ListPanel.Refresh(m_SelectedIndex);
        }

        /// <summary>
        /// Select the object.
        /// </summary>
        /// <param name="obj">The object to select.</param>
        public void Select(T obj)
        {
            var index = -1;
            m_ListPanel.SearchableList.ClearSearch(false);

            //Refresh required before because the inner list needs to be updated.
            m_ListPanel.Refresh();

            for (int i = 0; i < m_ListPanel.SearchableList.ItemList.Count; i++) {
                if (obj != m_ListPanel.SearchableList.ItemList[i]) { continue; }

                index = i;
                break;
            }

            // The list must be a subset, therefore draw the object instead of selecting it in the list.
            if (index == -1) {
                Debug.LogWarning("The object is not part of the database.");
                UpdateElements(obj);
                return;
            }

            m_SelectedIndex = index;
            m_ListPanel.Refresh(m_SelectedIndex);
        }

        /// <summary>
        /// Update the visual elements to reflect the specified object.
        /// </summary>
        /// <param name="obj">The object that is being displayed.</param>
        protected abstract void UpdateElements(T obj);

        /// <summary>
        /// Add a new object in the list.
        /// </summary>
        /// <param name="name">The new object name.</param>
        /// <returns>The created object.</returns>
        public abstract T OnAdd(string name);

        /// <summary>
        /// Remove an object from the list.
        /// </summary>
        /// <param name="index">The index in which the object lies.</param>
        public abstract void OnRemove(int index);

        /// <summary>
        /// Filter the list using a search string.
        /// </summary>
        /// <param name="list">The original list.</param>
        /// <param name="searchValue">The search expression.</param>
        /// <returns>The filtered list.</returns>
        public abstract IList<T> GetSearchFilter(IList<T> list, string searchValue);

        /// <summary>
        /// Refresh the list and the view whenever the list is modified.
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            m_ListPanel.SearchableList.ClearSearch(true);
            m_ListPanel.Refresh();
            UpdateElements(SelectedObject);
        }
    }
}