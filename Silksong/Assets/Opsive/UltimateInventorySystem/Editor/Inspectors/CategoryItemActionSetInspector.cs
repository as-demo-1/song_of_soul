/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Inspectors
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.ItemActions;
    using Opsive.UltimateInventorySystem.UI.DataContainers;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;

    /// <summary>
    /// The inspector for the categories item actions scriptable object.
    /// </summary>
    [CustomEditor(typeof(CategoryItemActionSet), true)]
    public class CategoryItemActionSetInspector : InspectorBase
    {
        protected const string c_CategoriesItemActions = "m_CategoriesItemActions";

        protected override List<string> ExcludedFields => new List<string>() { c_CategoriesItemActions };

        protected CategoryItemActionSet m_CategoryItemActionSet;
        protected List<ItemActionSet> m_List;
        protected ReorderableList m_ReorderableList;

        protected NestedScriptableObjectInspector<ItemActionSet, ItemActionSetInspector> m_SelectedCategoryItemActions;

        /// <summary>
        /// Create the inspector.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {
            m_CategoryItemActionSet = target as CategoryItemActionSet;

            if (m_CategoryItemActionSet.CategoryItemActionsArray == null) {
                m_CategoryItemActionSet.CategoryItemActionsArray = new ItemActionSet[0];
            }

            m_List = new List<ItemActionSet>(m_CategoryItemActionSet.CategoryItemActionsArray);
            m_ReorderableList = new ReorderableList(
                m_List,
                (parent, index) =>
                {
                    var listElement = new ListElement<ItemActionSet>();
                    listElement.Index = index;
                    listElement.OnValueChanged += OnValueChanged;
                    parent.Add(listElement);
                }, (parent, index) =>
                {
                    var listElement = parent.ElementAt(0) as ListElement<ItemActionSet>;
                    listElement.Index = index;
                    listElement.label = $"Element {index}";
                    listElement.value = m_ReorderableList.ItemsSource[index] as ItemActionSet;
                }, (parent) =>
                {
                    parent.Add(new Label("Category Item Actions"));
                }, (index) =>
                {
                    return 20f;
                },
                UpdateSelection,
                () =>
                {
                    m_List.Add(null);

                    m_CategoryItemActionSet.CategoryItemActionsArray = m_List.ToArray();
                    m_ReorderableList.Refresh(m_List);
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_CategoryItemActionSet);
                }, (index) =>
                {
                    if (index < 0 || index >= m_List.Count) { return; }

                    m_List.RemoveAt(index);

                    m_CategoryItemActionSet.CategoryItemActionsArray = m_List.ToArray();
                    m_ReorderableList.Refresh(m_List);
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_CategoryItemActionSet);

                    if (m_List.Count == 0) { UpdateSelection(-1); }

                }, (i1, i2) =>
                {
                    m_CategoryItemActionSet.CategoryItemActionsArray = m_List.ToArray();
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_CategoryItemActionSet);
                });

            container.Add(m_ReorderableList);


            m_SelectedCategoryItemActions = new NestedScriptableObjectInspector<ItemActionSet, ItemActionSetInspector>(
                (createObject) =>
                {
                    var selectedIndex = m_ReorderableList.SelectedIndex;
                    if (selectedIndex < 0 || selectedIndex >= m_List.Count) { return; }

                    m_List[selectedIndex] = createObject;
                    m_CategoryItemActionSet.CategoryItemActionsArray = m_List.ToArray();
                    m_ReorderableList.Refresh(m_List);
                }
                );

            container.Add(m_SelectedCategoryItemActions);
        }

        /// <summary>
        /// Update the selection visual element.
        /// </summary>
        /// <param name="index">The index selected.</param>
        private void UpdateSelection(int index)
        {
            m_SelectedCategoryItemActions.Clear();

            if (index < 0 || index >= m_List.Count) { return; }

            var selectedObject = m_List[index];

            m_SelectedCategoryItemActions.Refresh(selectedObject);
        }

        /// <summary>
        /// Serialize and send On value changed.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        private void OnValueChanged(int index, ItemActionSet value)
        {
            m_List[index] = value;
            m_CategoryItemActionSet.CategoryItemActionsArray = m_List.ToArray();
            m_ReorderableList.Refresh(m_List);
            Shared.Editor.Utility.EditorUtility.SetDirty(m_CategoryItemActionSet);

            UpdateSelection(m_ReorderableList.SelectedIndex);
        }

        /// <summary>
        /// The visual element in the list.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        public class ListElement<T> : VisualElement where T : UnityEngine.Object
        {
            public event Action<int, T> OnValueChanged;
            public int Index { get; set; }
            public string label { get => m_ObjectField.label; set => m_ObjectField.label = value; }
            public T value {
                get => m_ObjectField.value as T;
                set => m_ObjectField.value = value;
            }

            protected ObjectField m_ObjectField;

            /// <summary>
            /// Constructor.
            /// </summary>
            public ListElement()
            {
                m_ObjectField = new ObjectField();
                m_ObjectField.objectType = typeof(T);
                m_ObjectField.RegisterValueChangedCallback(evt =>
                {
                    OnValueChanged?.Invoke(Index, evt.newValue as T);
                });
                Add(m_ObjectField);
            }
        }
    }
}