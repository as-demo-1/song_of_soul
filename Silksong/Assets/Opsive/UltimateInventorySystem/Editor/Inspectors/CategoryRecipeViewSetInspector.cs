/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Inspectors
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Crafting;
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
    /// The inspector for the categories recipe box UIs.
    /// </summary>
    [CustomEditor(typeof(CategoryRecipeViewSet), true)]
    public class CategoryRecipeViewSetInspector : DatabaseInspectorBase
    {
        protected const string c_CategoriesRecipeViews = "m_CategoriesRecipeViews";

        protected override List<string> ExcludedFields => new List<string>() { c_CategoriesRecipeViews };

        protected CategoryRecipeViewSet m_CategoryRecipeViewSet;
        protected List<CategoryRecipeViews> m_List;
        protected ReorderableList m_ReorderableList;

        protected VisualElement m_Container;

        /// <summary>
        /// Initialize the inspector when it is first selected.
        /// </summary>
        protected override void InitializeInspector()
        {
            m_CategoryRecipeViewSet = target as CategoryRecipeViewSet;

            if (m_CategoryRecipeViewSet.CategoriesRecipeBoxUIArray == null) {
                m_CategoryRecipeViewSet.CategoriesRecipeBoxUIArray = new CategoryRecipeViews[0];
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

            m_List = new List<CategoryRecipeViews>(m_CategoryRecipeViewSet.CategoriesRecipeBoxUIArray);
            Refresh();
        }

        /// <summary>
        /// Refresh the inspector when the database changes.
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
                    listElement.Refresh((CategoryRecipeViews)m_ReorderableList.ItemsSource[index]);
                }, (parent) =>
                {
                    parent.Add(new Label("Categories Recipe Views"));
                }, (index) => { return 43f; },
                (index) =>
                {
                    //nothing
                }, () =>
                {
                    m_List.Add(default);

                    m_CategoryRecipeViewSet.CategoriesRecipeBoxUIArray = m_List.ToArray();
                    m_ReorderableList.Refresh(m_List);
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_CategoryRecipeViewSet);
                }, (index) =>
                {
                    if (index < 0 || index >= m_List.Count) { return; }

                    m_List.RemoveAt(index);

                    m_CategoryRecipeViewSet.CategoriesRecipeBoxUIArray = m_List.ToArray();
                    m_ReorderableList.Refresh(m_List);
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_CategoryRecipeViewSet);
                }, (i1, i2) =>
                {
                    m_CategoryRecipeViewSet.CategoriesRecipeBoxUIArray = m_List.ToArray();
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_CategoryRecipeViewSet);
                });

            m_Container.Add(m_ReorderableList);
        }

        /// <summary>
        /// Serialize the object when a value changes.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        private void OnValueChanged(int index, CategoryRecipeViews value)
        {
            m_List[index] = value;
            m_CategoryRecipeViewSet.CategoriesRecipeBoxUIArray = m_List.ToArray();
            m_ReorderableList.Refresh(m_List);
            Shared.Editor.Utility.EditorUtility.SetDirty(m_CategoryRecipeViewSet);
        }

        /// <summary>
        /// The visual element in a list.
        /// </summary>
        public class ListElement : VisualElement
        {
            public event Action<int, CategoryRecipeViews> OnValueChanged;
            public int Index { get; set; }

            protected CraftingCategoryField m_CategoryObjectField;
            protected ObjectField m_ObjectField;

            /// <summary>
            /// The constructor.
            /// </summary>
            /// <param name="database">The inventory database.</param>
            public ListElement(InventorySystemDatabase database)
            {
                m_CategoryObjectField = new CraftingCategoryField(
                    "Category",
                    database, new (string, Action<CraftingCategory>)[]
                    {
                        ("Set Category", (x) =>
                        {
                            OnValueChanged?.Invoke(Index,new CategoryRecipeViews(x,m_ObjectField.value as GameObject));
                        })
                    },
                    (x) => true);
                Add(m_CategoryObjectField);

                m_ObjectField = new ObjectField();
                m_ObjectField.objectType = typeof(GameObject);
                m_ObjectField.label = "Item View Prefab";
                m_ObjectField.RegisterValueChangedCallback(evt =>
                {
                    OnValueChanged?.Invoke(Index, new CategoryRecipeViews(m_CategoryObjectField.Value, evt.newValue as GameObject));
                });
                Add(m_ObjectField);
            }

            /// <summary>
            /// Refresh the visual element.
            /// </summary>
            /// <param name="value">The new category recipe boxes.</param>
            public void Refresh(CategoryRecipeViews value)
            {
                m_CategoryObjectField.Refresh(value.Category);
                m_ObjectField.value = value.RecipeViewPrefab;
            }
        }
    }
}