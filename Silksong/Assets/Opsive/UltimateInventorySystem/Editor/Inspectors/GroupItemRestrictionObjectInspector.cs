/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Inspectors
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;

    /// <summary>
    /// The custom inspector for the group item restriction object.
    /// </summary>
    [CustomEditor(typeof(GroupItemRestrictionObject), true)]
    public class GroupItemRestrictionObjectInspector : DatabaseInspectorBase
    {
        protected override List<string> ExcludedFields => new List<string> { "m_Restriction" };

        protected GroupItemRestrictionObject m_Target;
        protected GroupItemRestriction m_GroupItemRestriction;
        protected ItemCollectionRestrictions m_Restrictions;

        protected VisualElement m_RestrictionsContainer;

        protected IntegerField m_FullSize;

        protected VisualElement m_CategorySizeContainer;
        protected IntegerField m_CategorySize;
        protected UnicodeTextField m_CategorySizeAttributeName;

        protected VisualElement m_DefinitionSizeContainer;
        protected IntegerField m_DefinitionSize;
        protected UnicodeTextField m_DefinitionSizeAttributeName;

        protected VisualElement m_ItemSizeContainer;
        protected IntegerField m_ItemSize;
        protected UnicodeTextField m_ItemSizeAttributeName;

        protected VisualElement m_RejectCategoriesContainer;
        protected Toggle m_RejectCategories;
        protected List<ItemCategory> m_List;
        protected ReorderableList m_ReorderableList;

        /// <summary>
        /// Initialize the inspector when it is first selected.
        /// </summary>
        protected override void InitializeInspector()
        {
            m_Target = target as GroupItemRestrictionObject;
            m_GroupItemRestriction = m_Target.OriginalRestriction as GroupItemRestriction;

            m_List = m_GroupItemRestriction?.ItemCategories == null ?
                new List<ItemCategory>() :
                new List<ItemCategory>(m_GroupItemRestriction.ItemCategories);

            base.InitializeInspector();
        }

        /// <summary>
        /// Create the inspector.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {

            var customRestriction = new VisualElement();

            var fieldInfo = m_Target.OriginalRestriction.GetType().GetField("m_Restriction", BindingFlags.NonPublic | BindingFlags.Default | BindingFlags.Instance);
            FieldInspectorView.AddField(
                m_Target,
                m_Target.OriginalRestriction, fieldInfo,
                -1, m_Target.OriginalRestriction.GetType(),
                "Restriction", string.Empty, true,
                m_Target.OriginalRestriction,
                customRestriction,
                (object obj) =>
                {
                    FieldChanged();

                }, null, false, null, new HashSet<string>()
                {
                    //"m_ItemCollectionRestrictions",
                    "m_FullSizeLimit",
                    "m_DefaultCategorySizeLimit",
                    "m_LimitPerCategoryAttributeName",
                    "m_DefaultDefinitionSizeLimit",
                    "m_LimitPerDefinitionAttributeName",
                    "m_DefaultItemSizeLimit",
                    "m_ItemSizeLimitAttributeName",
                    "m_RejectCategories",
                    "m_ItemCategories",
                });

            container.Add(customRestriction);

            m_RestrictionsContainer = new VisualElement();
            container.Add(m_RestrictionsContainer);

            //The Full size limit.
            m_FullSize = new IntegerField("Full Size Limit");
            m_FullSize.value = m_GroupItemRestriction.FullSizeLimit;
            m_FullSize.RegisterValueChangedCallback(evt =>
            {
                m_GroupItemRestriction.FullSizeLimit = evt.newValue;
                Shared.Editor.Utility.EditorUtility.SetDirty(m_Target);
            });

            //The Category size limit.
            m_CategorySizeContainer = new VisualElement();

            m_CategorySize = new IntegerField("Category Default Size Limit");
            m_CategorySize.value = m_GroupItemRestriction.DefaultCategorySizeLimit;
            m_CategorySize.RegisterValueChangedCallback(evt =>
            {
                m_GroupItemRestriction.DefaultCategorySizeLimit = evt.newValue;
                Shared.Editor.Utility.EditorUtility.SetDirty(m_Target);
            });
            m_CategorySizeContainer.Add(m_CategorySize);

            m_CategorySizeAttributeName = new UnicodeTextField("Category Size Limit Attribute Name");
            m_CategorySizeAttributeName.value = m_GroupItemRestriction.LimitPerCategoryAttributeName;
            m_CategorySizeAttributeName.RegisterValueChangedCallback(evt =>
            {
                m_GroupItemRestriction.LimitPerCategoryAttributeName = evt.newValue;
                Shared.Editor.Utility.EditorUtility.SetDirty(m_Target);
            });
            m_CategorySizeContainer.Add(m_CategorySizeAttributeName);

            //The Definition size limit.
            m_DefinitionSizeContainer = new VisualElement();

            m_DefinitionSize = new IntegerField("Definition Default Size Limit");
            m_DefinitionSize.value = m_GroupItemRestriction.DefaultDefinitionSizeLimit;
            m_DefinitionSize.RegisterValueChangedCallback(evt =>
            {
                m_GroupItemRestriction.DefaultDefinitionSizeLimit = evt.newValue;
                Shared.Editor.Utility.EditorUtility.SetDirty(m_Target);
            });
            m_DefinitionSizeContainer.Add(m_DefinitionSize);

            m_DefinitionSizeAttributeName = new UnicodeTextField("Definition Size Limit Attribute Name");
            m_DefinitionSizeAttributeName.value = m_GroupItemRestriction.LimitPerDefinitionAttributeName;
            m_DefinitionSizeAttributeName.RegisterValueChangedCallback(evt =>
            {
                m_GroupItemRestriction.LimitPerDefinitionAttributeName = evt.newValue;
                Shared.Editor.Utility.EditorUtility.SetDirty(m_Target);
            });
            m_DefinitionSizeContainer.Add(m_DefinitionSizeAttributeName);

            //The Item size limit.
            m_ItemSizeContainer = new VisualElement();

            m_ItemSize = new IntegerField("Item Default Size Limit");
            m_ItemSize.value = m_GroupItemRestriction.DefaultItemSizeLimit;
            m_ItemSize.RegisterValueChangedCallback(evt =>
            {
                m_GroupItemRestriction.DefaultItemSizeLimit = evt.newValue;
                Shared.Editor.Utility.EditorUtility.SetDirty(m_Target);
            });
            m_ItemSizeContainer.Add(m_ItemSize);

            m_ItemSizeAttributeName = new UnicodeTextField("Item Size Limit Attribute Name");
            m_ItemSizeAttributeName.value = m_GroupItemRestriction.ItemSizeLimitAttributeName;
            m_ItemSizeAttributeName.RegisterValueChangedCallback(evt =>
            {
                m_GroupItemRestriction.ItemSizeLimitAttributeName = evt.newValue;
                Shared.Editor.Utility.EditorUtility.SetDirty(m_Target);
            });
            m_ItemSizeContainer.Add(m_ItemSizeAttributeName);

            //ItemCategories to Reject.
            m_RejectCategoriesContainer = new VisualElement();

            m_RejectCategories = new Toggle("Reject ItemCategories");
            m_RejectCategories.value = m_GroupItemRestriction.RejectCategories;
            m_RejectCategories.RegisterValueChangedCallback(evt =>
            {
                m_GroupItemRestriction.RejectCategories = evt.newValue;
                Shared.Editor.Utility.EditorUtility.SetDirty(m_Target);
            });
            m_RejectCategoriesContainer.Add(m_RejectCategories);

            FieldChanged();
            Refresh();
        }

        /// <summary>
        /// update the inspector to show only the restriction which will be applied.
        /// </summary>
        protected virtual void FieldChanged()
        {
            Shared.Editor.Utility.EditorUtility.SetDirty(m_Target);

            var newRestrictions = m_GroupItemRestriction.ItemCollectionRestrictions;
            if (m_Restrictions == newRestrictions) { return; }

            m_RestrictionsContainer.Clear();
            m_Restrictions = newRestrictions;

            if ((m_Restrictions & ItemCollectionRestrictions.FullSize) != 0) {
                m_RestrictionsContainer.Add(m_FullSize);
            }

            if ((m_Restrictions & ItemCollectionRestrictions.CategorySize) != 0) {
                m_RestrictionsContainer.Add(m_CategorySizeContainer);
            }

            if ((m_Restrictions & ItemCollectionRestrictions.DefinitionSize) != 0) {
                m_RestrictionsContainer.Add(m_DefinitionSizeContainer);
            }

            if ((m_Restrictions & ItemCollectionRestrictions.ItemSize) != 0) {
                m_RestrictionsContainer.Add(m_ItemSizeContainer);
            }

            if ((m_Restrictions & ItemCollectionRestrictions.CategoryRestriction) != 0) {
                m_RestrictionsContainer.Add(m_RejectCategoriesContainer);
            }
        }

        /// <summary>
        /// Refresh the visual elements when the database is changed.
        /// </summary>
        protected void Refresh()
        {
            m_RejectCategoriesContainer.Clear();
            m_RejectCategoriesContainer.Add(m_RejectCategories);

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
                    listElement.Refresh(m_ReorderableList.ItemsSource[index] as ItemCategory);
                }, (parent) =>
                {
                    parent.Add(new Label("Tab Item Categories"));
                }, (index) => { return 25f; },
                (index) =>
                {
                    // Intentionally left empty.
                }, () =>
                {
                    m_List.Add(default);

                    m_GroupItemRestriction.ItemCategories = m_List.ToArray();
                    m_ReorderableList.Refresh(m_List);
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_Target);
                }, (index) =>
                {
                    if (index < 0 || index >= m_List.Count) { return; }

                    m_List.RemoveAt(index);

                    m_GroupItemRestriction.ItemCategories = m_List.ToArray();
                    m_ReorderableList.Refresh(m_List);
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_Target);
                }, (i1, i2) =>
                {
                    var element1 = m_ReorderableList.ListItems[i1].ItemContents.ElementAt(0) as ListElement;
                    element1.Index = i1;
                    var element2 = m_ReorderableList.ListItems[i2].ItemContents.ElementAt(0) as ListElement;
                    element2.Index = i2;
                    m_GroupItemRestriction.ItemCategories = m_List.ToArray();
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_Target);
                });

            m_RejectCategoriesContainer.Add(m_ReorderableList);
        }

        /// <summary>
        /// Serialize and update the visuals.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        private void OnValueChanged(int index, ItemCategory value)
        {
            m_List[index] = value;
            m_GroupItemRestriction.ItemCategories = m_List.ToArray();
            m_ReorderableList.Refresh(m_List);
            Shared.Editor.Utility.EditorUtility.SetDirty(m_Target);
        }

        // <summary>
        /// The list element for the tabs.
        /// </summary>
        public class ListElement : VisualElement
        {
            public event Action<int, ItemCategory> OnValueChanged;
            public int Index { get; set; }

            protected ItemCategoryField m_CategoryObjectField;

            /// <summary>
            /// The list element.
            /// </summary>
            public ListElement(InventorySystemDatabase database)
            {
                m_CategoryObjectField = new ItemCategoryField(
                    "",
                    database, new (string, Action<ItemCategory>)[]
                    {
                        ("Set Category", (x) =>
                        {
                            OnValueChanged?.Invoke(Index,x);
                        })
                    },
                    (x) => true);
                Add(m_CategoryObjectField);
            }

            /// <summary>
            /// Update the visuals.
            /// </summary>
            /// <param name="value">The new value.</param>
            public void Refresh(ItemCategory value)
            {
                m_CategoryObjectField.Refresh(value);
            }
        }
    }
}