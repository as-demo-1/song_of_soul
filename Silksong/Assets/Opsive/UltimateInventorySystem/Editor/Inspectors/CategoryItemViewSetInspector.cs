/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Inspectors
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.Storage;
    using Opsive.UltimateInventorySystem.UI.DataContainers;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// The inspector for the categories Item View uis.
    /// </summary>
    [CustomEditor(typeof(CategoryItemViewSet), true)]
    public class CategoryItemViewSetInspector : DatabaseInspectorBase
    {
        protected const string c_CategoriesItemViews = "m_CategoriesItemViews";

        protected override List<string> ExcludedFields => new List<string>() { c_CategoriesItemViews };

        protected CategoryItemViewSet m_CategoryItemViewSet;
        protected List<CategoryItemViews> m_List;
        protected ReorderableList m_ReorderableList;

        protected VisualElement m_Container;

        /// <summary>
        /// Initialize the inspector when it is first selected.
        /// </summary>
        protected override void InitializeInspector()
        {
            m_CategoryItemViewSet = target as CategoryItemViewSet;

            if (m_CategoryItemViewSet.CategoriesItemViews == null) {
                m_CategoryItemViewSet.CategoriesItemViews = new CategoryItemViews[0];
            }

            base.InitializeInspector();
        }

        /// <summary>
        /// Create the inspector.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {
            m_Container = container;

            m_List = new List<CategoryItemViews>(m_CategoryItemViewSet.CategoriesItemViews);
            Refresh();
        }

        /// <summary>
        /// Refresh the inspector.
        /// </summary>
        protected void Refresh()
        {
            if (m_Container == null) { return; }
            if (m_ReorderableList != null) {
                if (m_Container.Contains(m_ReorderableList)) {
                    m_Container.Remove(m_ReorderableList);
                }
            }

            m_ReorderableList = new ReorderableList(
                m_List,
                (parent, index) =>
                {
                    var listElement = new ListElement(m_DatabaseField.value as InventorySystemDatabase);
                    listElement.Index = index;
                    listElement.OnValueChanged += OnValueChanged;
                    parent.Add(listElement);
                }, (parent, index) =>
                {
                    var listElement = parent.ElementAt(0) as ListElement;
                    listElement.Index = index;
                    listElement.Refresh((CategoryItemViews)m_ReorderableList.ItemsSource[index]);
                }, (parent) =>
                {
                    parent.Add(new Label("Category Item Views"));
                }, (index) => { return 43f; },
                (index) =>
                {
                    // Not implemented.
                }, () =>
                {
                    m_List.Add(default);

                    m_CategoryItemViewSet.CategoriesItemViews = m_List.ToArray();
                    m_ReorderableList.Refresh(m_List);
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_CategoryItemViewSet);
                }, (index) =>
                {
                    if (index < 0 || index >= m_List.Count) { return; }

                    m_List.RemoveAt(index);

                    m_CategoryItemViewSet.CategoriesItemViews = m_List.ToArray();
                    m_ReorderableList.Refresh(m_List);
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_CategoryItemViewSet);
                }, (i1, i2) =>
                {
                    var element1 = m_ReorderableList.ListItems[i1].ItemContents.ElementAt(0) as ListElement;
                    element1.Index = i1;
                    var element2 = m_ReorderableList.ListItems[i2].ItemContents.ElementAt(0) as ListElement;
                    element2.Index = i2;
                    m_CategoryItemViewSet.CategoriesItemViews = m_List.ToArray();
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_CategoryItemViewSet);
                });

            m_Container.Add(m_ReorderableList);
        }

        /// <summary>
        /// Serialise when a value changes.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        private void OnValueChanged(int index, CategoryItemViews value)
        {
            m_List[index] = value;
            m_CategoryItemViewSet.CategoriesItemViews = m_List.ToArray();
            m_ReorderableList.Refresh(m_List);
            Shared.Editor.Utility.EditorUtility.SetDirty(m_CategoryItemViewSet);
        }

        /// <summary>
        /// The visual element for the object in the list.
        /// </summary>
        public class ListElement : VisualElement
        {
            public event Action<int, CategoryItemViews> OnValueChanged;
            public int Index { get; set; }

            protected ItemCategoryField m_CategoryObjectField;
            protected ObjectField m_ObjectField;

            /// <summary>
            /// The constructor.
            /// </summary>
            /// <param name="database">The inventory database.</param>
            public ListElement(InventorySystemDatabase database)
            {
                m_CategoryObjectField = new ItemCategoryField(
                    "Category",
                    database, new (string, Action<ItemCategory>)[]
                    {
                        ("Set Category", (x) =>
                        {
                            OnValueChanged?.Invoke(Index,new CategoryItemViews(x,m_ObjectField.value as GameObject));
                        })
                    },
                    (x) => true);
                Add(m_CategoryObjectField);

                m_ObjectField = new ObjectField();
                m_ObjectField.objectType = typeof(GameObject);
                m_ObjectField.label = "Item View Prefab";
                m_ObjectField.RegisterValueChangedCallback(evt =>
                {
                    OnValueChanged?.Invoke(Index, new CategoryItemViews(m_CategoryObjectField.Value, evt.newValue as GameObject));
                });
                Add(m_ObjectField);
            }

            /// <summary>
            /// Refresh the component.
            /// </summary>
            /// <param name="value">The new category Item Viewes.</param>
            public void Refresh(CategoryItemViews value)
            {
                m_CategoryObjectField.Refresh(value.Category);
                m_ObjectField.value = value.ItemViewPrefab;
            }
        }
    }
}