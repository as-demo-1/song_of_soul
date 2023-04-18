/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Managers;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using UnityEngine.UIElements;

    /// <summary>
    /// The item field.
    /// </summary>
    public class ItemField : VisualElement
    {
        public event Action OnValueChanged;

        protected Item m_Item;
        public Item Value => m_Item;

        protected ItemDefinitionField m_ItemDefinitionField;
        protected VisualElement m_ItemInfoContainer;
        protected Foldout m_ItemInfoFoldout;
        protected Label m_ItemName;
        protected Label m_ItemID;
        protected Label m_Mutable;
        protected Label m_Unique;
        protected ItemDefinitionViewName m_ItemDefinition;
        protected ItemCategoryViewName m_ItemCategory;


        protected TabToolbar m_AttributeCollectionsTabToolbar;
        protected AttributeVisualElement m_AttributeVisualElement;
        protected ReorderableList m_AttributesReorderableList;

        public static Item CreateItem(ItemDefinition definition)
        {
            return Item.Create(definition);
        }
        
        /// <summary>
        /// Create the item field.
        /// </summary>
        /// <param name="database">The database.</param>
        public ItemField(InventorySystemDatabase database)
        {
            m_ItemDefinitionField = new ItemDefinitionField("", database,
                new (string, Action<ItemDefinition>)[]
            {
                ("Set Item Definition (default)", x=>
                {
                    var newItem = CreateItem(x);
                    Refresh(newItem);
                }),
                ("Set Item Definition with Attribute Values", x=>
                {
                    var newItem = Item.Create(x,m_Item?.ItemAttributeCollection);
                    Refresh(newItem);
                })
            }, (x) => true);
            Add(m_ItemDefinitionField);

            m_ItemInfoContainer = new VisualElement();
            Add(m_ItemInfoContainer);

            m_ItemInfoFoldout = new Foldout();
            m_ItemInfoFoldout.text = "Item Details";
            m_ItemName = new Label();
            m_ItemInfoFoldout.Add(m_ItemName);
            m_ItemID = new Label();
            m_ItemInfoFoldout.Add(m_ItemID);
            var categoryContainer = new VisualElement();
            categoryContainer.AddToClassList("horizontal-layout");
            categoryContainer.Add(new Label("Item Category: "));
            m_ItemCategory = new ItemCategoryViewName();
            categoryContainer.Add(m_ItemCategory);
            m_ItemInfoFoldout.Add(categoryContainer);
            m_Mutable = new Label();
            m_ItemInfoFoldout.Add(m_Mutable);
            m_Unique = new Label();
            m_ItemInfoFoldout.Add(m_Unique);
            var definitionContainer = new VisualElement();
            definitionContainer.AddToClassList("horizontal-layout");
            definitionContainer.Add(new Label("Item Definition:"));
            m_ItemDefinition = new ItemDefinitionViewName();
            definitionContainer.Add(m_ItemDefinition);
            m_ItemInfoFoldout.Add(definitionContainer);


            var attributesBox = new VisualElement();
            attributesBox.name = "box";
            attributesBox.AddToClassList(ManagerStyles.BoxBackground);

            m_AttributeCollectionsTabToolbar = new TabToolbar(new string[]
            {
                "Item Attributes",
                "Item Definition Attributes",
                "Item Category Attributes",
            }, 0, ShowAttributeTab);
            attributesBox.Add(m_AttributeCollectionsTabToolbar);

            m_AttributeVisualElement = new AttributeVisualElement(database);
            m_AttributeVisualElement.AddToClassList(InventoryManagerStyles.AttributeView_Margin);
            m_AttributesReorderableList = new ReorderableList(null, (VisualElement parent, int index) =>
            {
                parent.Add(new AttributeViewNameAndValue());
            }, (VisualElement parent, int index) =>
            {
                var attributeViewNameAndValue = parent.ElementAt(0) as AttributeViewNameAndValue;
                var attribute = m_AttributesReorderableList.ItemsSource[index] as AttributeBase;
                attributeViewNameAndValue.Refresh(attribute);
            }, null, (int index) =>
            {
                m_AttributeVisualElement.BindAttribute(m_AttributesReorderableList.ItemsSource[index] as AttributeBase);
            }, null, null,
                (i1, i2) =>
            {
                Refresh();
            });
            m_AttributeVisualElement.OnValueChanged += (attribute) =>
            {
                SoftRefresh();
            };
            m_AttributeVisualElement.OnAttributeReplaced += (attribute) =>
            {
                Refresh();
                ShowAttributeTab(m_AttributeCollectionsTabToolbar.Selected);
                m_AttributesReorderableList.SelectedIndex = m_AttributesReorderableList.ItemsSource.Count - 1;
            };
            attributesBox.Add(m_AttributesReorderableList);
            attributesBox.Add(m_AttributeVisualElement);
            Add(attributesBox);
        }

        /// <summary>
        /// Shows the attributes that correspond to the tab at the specified index.
        /// </summary>
        /// <param name="index">The attribute tab index.</param>
        private void ShowAttributeTab(int index)
        {
            m_AttributeVisualElement.ClearBinding();

            var collection = GetAttributeCollection(index);
            if (collection == null) { return; }

            //Only allow to edit Item attributes.
            m_AttributeVisualElement.SetEnabled(index == 0);

            m_AttributesReorderableList.Refresh(collection.Attributes);

            var attributeIndex = m_AttributesReorderableList.SelectedIndex;
            if (attributeIndex >= 0 && attributeIndex < m_AttributesReorderableList.ItemsSource.Count) {
                m_AttributeVisualElement.BindAttribute(m_AttributesReorderableList.ItemsSource[attributeIndex] as AttributeBase);
            }
        }

        /// <summary>
        /// Returns the AttributeCollection at the specified index.
        /// </summary>
        /// <param name="index">The index to retrieve the AttributeCollection at.</param>
        /// <returns>The AttributeCollection at the specified index.</returns>
        private AttributeCollection GetAttributeCollection(int index)
        {
            if (m_Item == null) { return null; }

            if (index == 0) { // Item Attributes.
                return m_Item?.ItemAttributeCollection;
            } else if (index == 1) { // Item Definition Attributes.
                return m_Item?.ItemDefinition?.ItemDefinitionAttributeCollection;
            } else { // Item Category Attributes.
                return m_Item?.Category?.ItemCategoryAttributeCollection;
            }
        }

        /// <summary>
        /// Set the preFilter.
        /// </summary>
        /// <param name="preFilterCondition">The prefilter condition.</param>
        public virtual void SetPreFilter(Func<ItemDefinition, bool> preFilterCondition)
        {
            m_ItemDefinitionField.SetPreFilter(preFilterCondition);
        }

        /// <summary>
        /// Refresh the elements without redrawing them from scratch.
        /// </summary>
        private void SoftRefresh()
        {
            m_Item?.Serialize();
            m_AttributesReorderableList.Refresh(m_Item?.ItemAttributeCollection.Attributes);
            OnValueChanged?.Invoke();
        }

        /// <summary>
        /// Change the item viewed.
        /// </summary>
        /// <param name="newValue">The new item.</param>
        public void Refresh(Item newValue)
        {
            m_Item = newValue;

            Refresh();
        }

        /// <summary>
        /// Redraw the visual elements.
        /// </summary>
        public void Refresh()
        {
            if (m_Item?.IsInitialized == false) {
                if (m_Item?.ItemDefinition != null) {
                    m_Item.Initialize(false);
                }
            }
            m_ItemDefinitionField.Refresh(m_Item?.ItemDefinition);

            m_ItemInfoContainer.Clear();
            if (m_Item != null) {
                m_ItemInfoContainer.Add(m_ItemInfoFoldout);

                m_ItemName.text = $"Name: {m_Item.name}";
                m_ItemID.text = $"ID: {m_Item.ID}";
                m_Mutable.text = $"Mutable: {m_Item.IsMutable}";
                m_Unique.text = $"Unique: {m_Item.IsUnique}";
                m_ItemDefinition.Refresh(m_Item.ItemDefinition);
                m_ItemCategory.Refresh(m_Item.Category);
            }

            m_AttributeVisualElement.ClearBinding();
            m_AttributesReorderableList.Refresh(m_Item?.ItemAttributeCollection?.Attributes);

            var attributeIndex = m_AttributesReorderableList.SelectedIndex;
            if (attributeIndex >= 0 && attributeIndex < m_AttributesReorderableList.ItemsSource.Count) {
                m_AttributeVisualElement.BindAttribute(m_AttributesReorderableList.ItemsSource[attributeIndex] as AttributeBase);
            }

            m_Item?.Serialize();

            OnValueChanged?.Invoke();
        }
    }
}