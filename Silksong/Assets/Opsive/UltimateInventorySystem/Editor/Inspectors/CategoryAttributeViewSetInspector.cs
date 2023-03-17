/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Inspectors
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames;
    using Opsive.UltimateInventorySystem.Storage;
    using Opsive.UltimateInventorySystem.UI.DataContainers;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// The inspector for categories attribute uis.
    /// </summary>
    [CustomEditor(typeof(CategoryAttributeViewSet), true)]
    public class CategoryAttributeViewSetInspector : DatabaseInspectorBase
    {
        protected const string c_CategoriesAttributeUIs = "m_CategoriesAttributeViews";

        protected override List<string> ExcludedFields => new List<string>() { c_CategoriesAttributeUIs };

        protected CategoryAttributeViewSet m_CategoryAttributeViewSet;
        protected List<CategoryAttributeViews> m_List;
        protected ReorderableList m_ReorderableList;
        protected VisualElement m_Selection;

        protected ItemCategoryAttributeNameBindingView m_ItemCategoryAttributeBindingView;

        /// <summary>
        /// Initialize the inspector when it is first selected.
        /// </summary>
        protected override void InitializeInspector()
        {
            m_CategoryAttributeViewSet = target as CategoryAttributeViewSet;

            if (m_CategoryAttributeViewSet.CategoriesAttributeBoxArray == null) {
                m_CategoryAttributeViewSet.CategoriesAttributeBoxArray = new CategoryAttributeViews[0];
            }

            base.InitializeInspector();
        }

        /// <summary>
        /// Create the inspector.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {
            m_CategoryAttributeViewSet = target as CategoryAttributeViewSet;

            if (m_CategoryAttributeViewSet.CategoriesAttributeBoxArray == null) {
                m_CategoryAttributeViewSet.CategoriesAttributeBoxArray = new CategoryAttributeViews[0];
            }

            m_List = new List<CategoryAttributeViews>(m_CategoryAttributeViewSet.CategoriesAttributeBoxArray);
            m_ReorderableList = new ReorderableList(
                m_List,
                (parent, index) =>
                {
                    var listElement = new ItemCategoryViewName();

                    parent.Add(listElement);
                }, (parent, index) =>
                {
                    var listElement = parent.ElementAt(0) as ItemCategoryViewName;
                    var categoryAttributeUIs = m_List[index];
                    listElement.Refresh(categoryAttributeUIs.Category);
                }, (parent) =>
                {
                    parent.Add(new Label("Categories Attribute Boxes"));
                },
                (index) => { Select(index); },
                () =>
                {
                    m_List.Add(default);

                    m_CategoryAttributeViewSet.CategoriesAttributeBoxArray = m_List.ToArray();
                    m_ReorderableList.Refresh(m_List);
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_CategoryAttributeViewSet);
                    Select(m_List.Count - 1);
                }, (index) =>
                {
                    if (index < 0 || index >= m_List.Count) { return; }

                    m_List.RemoveAt(index);

                    m_CategoryAttributeViewSet.CategoriesAttributeBoxArray = m_List.ToArray();
                    m_ReorderableList.Refresh(m_List);
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_CategoryAttributeViewSet);
                    if (index >= m_List.Count) { index = m_List.Count - 1; }
                    Select(index);
                }, (i1, i2) =>
                {
                    m_CategoryAttributeViewSet.CategoriesAttributeBoxArray = m_List.ToArray();
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_CategoryAttributeViewSet);
                });

            container.Add(m_ReorderableList);

            m_Selection = new VisualElement();
            container.Add(m_Selection);

            Refresh();
        }

        /// <summary>
        /// Refresh the inspector when the database changes.
        /// </summary>
        protected void Refresh()
        {
            m_Selection.Clear();
            m_ItemCategoryAttributeBindingView = new ItemCategoryAttributeNameBindingView(
                m_DatabaseField.value as InventorySystemDatabase, typeof(GameObject), true);
            m_Selection.Add(m_ItemCategoryAttributeBindingView);

            m_ItemCategoryAttributeBindingView.OnItemCategoryChanged +=
                (x) =>
                {
                    OnValueChanged(m_ReorderableList.SelectedIndex,
                        new CategoryAttributeViews(x, m_List[m_ReorderableList.SelectedIndex].AttributeViews));
                };

            m_ItemCategoryAttributeBindingView.OnAttributeBindingsChanged +=
                (bindings) =>
                {
                    var attributeUIs = new List<StringKeyGameObject>();
                    for (int i = 0; i < bindings.Count; i++) {
                        var attributeBinding = bindings[i];
                        if (attributeBinding.BoundComponent == null) { continue; }
                        attributeUIs.Add(new StringKeyGameObject(
                            attributeBinding.AttributeName, attributeBinding.BoundComponent as GameObject));
                    }

                    OnValueChanged(m_ReorderableList.SelectedIndex,
                        new CategoryAttributeViews(m_List[m_ReorderableList.SelectedIndex].Category, attributeUIs.ToArray()));
                };

            Select(m_ReorderableList.SelectedIndex);
        }

        /// <summary>
        /// Select the index.
        /// </summary>
        /// <param name="index">The index.</param>
        private void Select(int index)
        {
            m_ReorderableList.SelectedIndex = index;
            m_Selection.Clear();
            if (index < 0 || index >= m_List.Count) {
                m_Selection.Add(new Label("No item is selected"));
                return;
            }
            m_Selection.Add(m_ItemCategoryAttributeBindingView);
            m_ItemCategoryAttributeBindingView.SetItemCategory(m_List[index].Category);

            var attributeBindings = new List<AttributeBinding>();

            if (m_List[index].AttributeViews != null) {
                for (int i = 0; i < m_List[index].AttributeViews.Length; i++) {
                    var attributeUI = m_List[index].AttributeViews[i];
                    attributeBindings.Add(new AttributeNameBinding(attributeUI.Name, attributeUI.Object, ""));
                }
            }

            m_ItemCategoryAttributeBindingView.SetAttributeBindings(attributeBindings);
        }

        /// <summary>
        /// Send events when the value is changed.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        private void OnValueChanged(int index, CategoryAttributeViews value)
        {
            m_List[index] = value;
            m_CategoryAttributeViewSet.CategoriesAttributeBoxArray = m_List.ToArray();
            m_ReorderableList.Refresh(m_List);
            Shared.Editor.Utility.EditorUtility.SetDirty(m_CategoryAttributeViewSet);
        }
    }
}