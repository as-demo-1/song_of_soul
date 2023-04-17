/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Inspectors
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Managers;
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Editor.Managers;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// The view for category attribute bindings.
    /// </summary>
    public class ItemCategoryAttributeBindingView : VisualElement
    {
        public event Action<ItemCategory> OnItemCategoryChanged;
        public event Action<List<AttributeBinding>> OnAttributeBindingsChanged;

        protected InventorySystemDatabase m_Database;

        protected VisualElement m_SelectedContainer;

        protected ItemCategory m_ItemCategory;
        protected ItemCategoryField m_ItemCategoryField;

        protected List<AttributeBinding> m_AttributeBindings;

        protected TabToolbar m_AttributeCollectionsTabToolbar;
        protected ReorderableList m_AttributesReorderableList;
        protected VisualElement m_AttributesBox;

        protected Type m_ObjectType;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="objectType">The object type to bind.</param>
        public ItemCategoryAttributeBindingView(InventorySystemDatabase database, Type objectType)
        {
            m_ObjectType = objectType;
            InventoryMainWindow.OnLostFocusEvent += Refresh;

            m_Database = database;
            m_SelectedContainer = new VisualElement();
            Add(m_SelectedContainer);

            m_ItemCategoryField = new ItemCategoryField(
                "Item Category",
                m_Database,
                new (string, Action<ItemCategory>)[]
                {
                    ("Set Category", (x) =>
                    {
                        SetItemCategory(x);
                        OnItemCategoryChanged?.Invoke(x);
                        Refresh();
                        AttributeBindingViewChanged();
                    })
                },
                (x) => true);

            m_AttributesBox = new VisualElement();
            m_AttributesBox.name = "box";
            m_AttributesBox.AddToClassList(ManagerStyles.BoxBackground);

            m_AttributeCollectionsTabToolbar = new TabToolbar(new string[]
            {
                "Item Attributes",
                "Item Definition Attributes",
                "Category Attributes",
            }, 0, ShowAttributeTab);
            m_AttributesBox.Add(m_AttributeCollectionsTabToolbar);

            m_AttributesReorderableList = new ReorderableList(null, MakeItem, BindItem, null, null, null, null, null);
            m_AttributesBox.Add(m_AttributesReorderableList);

            Refresh();
            ShowAttributeTab(0);
        }

        /// <summary>
        /// Update the view to show the latest values.
        /// </summary>
        protected void Refresh()
        {
            m_SelectedContainer.Clear();

            if (m_Database == null) {
                m_SelectedContainer.Add(new Label("The database is null."));
                return;
            }
            m_Database.Initialize(false);

            m_ItemCategoryField.Refresh(m_ItemCategory);
            UpdateAttributeBindings();
            m_SelectedContainer.Add(m_ItemCategoryField);

            if (m_ItemCategoryField.Value == null) {
                m_SelectedContainer.Add(new Label("Assign an Item Category to see the attribute binding options."));
                return;
            }

            m_SelectedContainer.Add(m_AttributesBox);

            ShowAttributeTab(m_AttributeCollectionsTabToolbar.Selected);
        }

        /// <summary>
        /// Bind the item to a list index.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="index">The index.</param>
        protected virtual void BindItem(VisualElement parent, int index)
        {
            var attributeBindingView = parent.ElementAt(0) as AttributeBindingView;
            var attribute = ((AttributeBase, AttributeBinding))m_AttributesReorderableList.ItemsSource[index];
            attributeBindingView.Setup(attribute.Item1, attribute.Item2);
        }

        /// <summary>
        /// Make an item in the list.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="index">The index.</param>
        protected virtual void MakeItem(VisualElement parent, int index)
        {
            var attributeBindingView = new AttributeBindingView(m_ObjectType);
            attributeBindingView.OnValueChange += AttributeBindingViewChanged;
            if (Application.isPlaying) { attributeBindingView.SetEnabled(false); }
            parent.Add(attributeBindingView);
        }

        /// <summary>
        /// Send an event that the Attribute Bindings changed.
        /// </summary>
        protected void AttributeBindingViewChanged()
        {
            OnAttributeBindingsChanged?.Invoke(m_AttributeBindings);
        }

        /// <summary>
        /// Set the item category.
        /// </summary>
        /// <param name="itemCategory">The item category.</param>
        public void SetItemCategory(ItemCategory itemCategory)
        {
            m_ItemCategory = itemCategory;
            Refresh();
        }

        /// <summary>
        /// Set the attribute bindings.
        /// </summary>
        /// <param name="attributeBindings">The attribute bindings.</param>
        public void SetAttributeBindings(IList<AttributeBinding> attributeBindings)
        {
            m_AttributeBindings = attributeBindings != null ?
                new List<AttributeBinding>(attributeBindings) :
                new List<AttributeBinding>();
            Refresh();
        }

        /// <summary>
        /// Update the attribute bindings.
        /// </summary>
        protected virtual void UpdateAttributeBindings()
        {
            if (Application.isPlaying) { return; }
            if (m_ItemCategory == null) { return; }

            if (m_AttributeBindings == null) {
                m_AttributeBindings = new List<AttributeBinding>();
            }

            var attributeBindings = m_AttributeBindings;
            var allAttributeCount = m_ItemCategory.GetAttributesCount();
            var newBindings = new AttributeBinding[allAttributeCount];
            for (int i = 0; i < allAttributeCount; i++) {
                var attribute = m_ItemCategory.GetAttributesAt(i);
                newBindings[i] = FindMatchOrCreate(attributeBindings, attribute);
            }

            m_AttributeBindings = new List<AttributeBinding>(newBindings);
        }

        /// <summary>
        /// Find a match or create an attribute binding for the attribute provided.
        /// </summary>
        /// <param name="attributeBindings">The attribute bindings.</param>
        /// <param name="attribute">The attribute.</param>
        /// <returns>The attribute binding.</returns>
        protected virtual AttributeBinding FindMatchOrCreate(List<AttributeBinding> attributeBindings, AttributeBase attribute)
        {
            for (int j = 0; j < attributeBindings.Count; j++) {
                var attributeBinding = attributeBindings[j];
                if (attributeBinding == null) { continue; }

                if (attribute.Name != attributeBinding.AttributeName ||
                    attributeBinding.GetType().GetGenericArguments()[0] != attribute.GetValueType()) { continue; }

                return attributeBinding;
            }

            var newAttributeBinding =
                Activator.CreateInstance(
                    typeof(AttributeBinding<>).MakeGenericType(attribute.GetValueType())) as AttributeBinding;
            newAttributeBinding.AttributeName = attribute.Name;
            return newAttributeBinding;
        }

        /// <summary>
        /// Shows the attributes that correspond to the tab at the specified index.
        /// </summary>
        /// <param name="index">The attribute tab index.</param>
        protected void ShowAttributeTab(int index)
        {
            m_AttributesReorderableList?.Refresh(GetAttributeCollection(index));
        }

        /// <summary>
        /// Returns the AttributeCollection at the specified index.
        /// </summary>
        /// <param name="index">The index to retrieve the AttributeCollection at.</param>
        /// <returns>The AttributeCollection at the specified index.</returns>
        protected IList GetAttributeCollection(int index)
        {
            var category = m_ItemCategoryField.Value;
            if (category == null) { return null; }
            if (m_AttributeBindings == null) { return null; }

            var list = new List<(AttributeBase, AttributeBinding)>();

            var offset = 0;
            ResizableArray<AttributeBase> attributes;

            if (index == 0) { // Item Attributes.
                offset = category.ItemCategoryAttributeCollection.Count + category.ItemDefinitionAttributeCollection.Count;
                attributes = category.ItemAttributeCollection.Attributes;
            } else if (index == 1) { // Item Definition Attributes.
                offset = category.ItemCategoryAttributeCollection.Count;
                attributes = category.ItemDefinitionAttributeCollection.Attributes;
            } else { // Item Category Attributes.
                offset = 0;
                attributes = category.ItemCategoryAttributeCollection.Attributes;
            }

            for (int i = 0; i < attributes.Count; i++) {
                //The attributes in the binding may not exist
                if (i + offset >= m_AttributeBindings.Count) { continue; }
                list.Add((attributes[i], m_AttributeBindings[i + offset]));
            }

            return list;
        }
    }

    /// <summary>
    /// Attribute Name Binding view.
    /// </summary>
    public class ItemCategoryAttributeNameBindingView : ItemCategoryAttributeBindingView
    {
        protected bool m_HidePropertyPath = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="objectType">The object type to bind.</param>
        public ItemCategoryAttributeNameBindingView(InventorySystemDatabase database, Type objectType) : base(database, objectType)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="hidePropertyPath">The properties to hide.</param>
        public ItemCategoryAttributeNameBindingView(InventorySystemDatabase database, Type objectType, bool hidePropertyPath) :
            base(database, objectType)
        {
            m_HidePropertyPath = hidePropertyPath;
        }

        /// <summary>
        /// Find a match or create an attribute binding for the attribute provided.
        /// </summary>
        /// <param name="attributeBindings">The attribute bindings.</param>
        /// <param name="attribute">The attribute.</param>
        /// <returns>The attribute binding.</returns>
        protected override AttributeBinding FindMatchOrCreate(List<AttributeBinding> attributeBindings, AttributeBase attribute)
        {
            for (int j = 0; j < attributeBindings.Count; j++) {
                var attributeBinding = attributeBindings[j];
                if (attributeBinding == null) { continue; }

                if (attribute.Name != attributeBinding.AttributeName) { continue; }

                return attributeBinding;
            }

            var newAttributeBinding =
                Activator.CreateInstance(typeof(AttributeNameBinding)) as AttributeBinding;
            newAttributeBinding.AttributeName = attribute.Name;
            return newAttributeBinding;
        }

        /// <summary>
        /// Make an item in the list.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="index">The index.</param>
        protected override void MakeItem(VisualElement parent, int index)
        {
            var attributeBindingView = new AttributeNameBindingView(m_ObjectType);
            attributeBindingView.HidePropertyPath(m_HidePropertyPath);
            attributeBindingView.OnValueChange += AttributeBindingViewChanged;
            if (Application.isPlaying) { attributeBindingView.SetEnabled(false); }
            parent.Add(attributeBindingView);
        }

        /// <summary>
        /// Bind the item to a list index.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="index">The index.</param>
        protected override void BindItem(VisualElement parent, int index)
        {
            var attributeBindingView = parent.ElementAt(0) as AttributeNameBindingView;
            var attribute = ((AttributeBase, AttributeBinding))m_AttributesReorderableList.ItemsSource[index];
            attributeBindingView.Setup(attribute.Item1, attribute.Item2);
        }
    }
}