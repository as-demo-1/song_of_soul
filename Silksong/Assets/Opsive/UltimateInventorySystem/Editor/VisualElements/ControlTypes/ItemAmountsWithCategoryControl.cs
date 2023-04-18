/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements.ControlTypes
{
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using System.Reflection;
    using UnityEngine.UIElements;

    /// <summary>
    /// Implements TypeControlBase for the ObjectAmounts ControlType.
    /// </summary>
    [ControlType(typeof(ItemAmountsWithCategory))]
    public class ItemAmountsWithCategoryControl : ControlWithInventoryDatabase
    {
        /// <summary>
        /// Create the ObjectAmountsView.
        /// </summary>
        /// <param name="value">The default value.</param>
        /// <param name="onChangeEvent">The on change event function.</param>
        /// <param name="field"></param>
        /// <returns>The ObjectAmountsView.</returns>
        public override VisualElement CreateCustomControlVisualElement(object value, Func<object, bool> onChangeEvent,
            FieldInfo field)
        {
            return new ItemAmountsWithCategoryView(value as ItemAmountsWithCategory, m_Database, onChangeEvent);
        }
    }

    /// <summary>
    /// Object Amounts view from ObjectAmountsBasView
    /// </summary>
    public class ItemAmountsWithCategoryView : ItemAmountsView
    {
        protected Toggle m_Inherently;
        protected ItemCategoryField m_ItemCategoryField;

        /// <summary>
        /// Requires base constructor.
        /// </summary>
        /// <param name="objectAmounts">The object amounts.</param>
        /// <param name="database">The inventory system database.</param>
        /// <param name="onChangeEvent">The onChangeEvent function.</param>
        public ItemAmountsWithCategoryView(ItemAmountsWithCategory objectAmounts, InventorySystemDatabase database, Func<object, bool> onChangeEvent) :
            base(objectAmounts, database, onChangeEvent)
        {
            objectAmounts = m_ObjectAmounts as ItemAmountsWithCategory;

            bool PrefilterCondition(ItemDefinition itemDefinition)
            {
                return (m_ObjectAmounts as ItemAmountsWithCategory)?.Condition(itemDefinition) ?? false;
            }

            m_Label.text = "Item Amounts With Category";

            m_Inherently = new Toggle("Inherently");
            m_Inherently.value = objectAmounts.Inherently;
            m_Inherently.RegisterValueChangedCallback(evt =>
            {
                objectAmounts.Inherently = evt.newValue;
                Refresh();
            });
            Add(m_Inherently);
            m_Inherently.SendToBack();

            m_ItemCategoryField = new ItemCategoryField("Item Category", database,
                category =>
                {
                    objectAmounts.ItemCategory = category;
                    m_ItemCategoryField.Refresh(category);
                    m_ItemField.SetPreFilter(PrefilterCondition);
                    Refresh();
                },
                category => true);
            m_ItemCategoryField.Refresh(objectAmounts.ItemCategory);
            Add(m_ItemCategoryField);
            m_ItemCategoryField.SendToBack();

            m_ItemField.SetPreFilter(PrefilterCondition);
            m_ItemDefinitionSearchableListWindow?.SetPreFilter(PrefilterCondition);
        }

        /// <summary>
        /// Create a default ObjectAmounts
        /// </summary>
        /// <returns>The default ObjectAmounts.</returns>
        protected override ItemAmounts CreateObjectAmounts()
        {
            return new ItemAmountsWithCategory(null, true);
        }

        /// <summary>
        /// Redraw the list.
        /// </summary>
        public override void Refresh()
        {
            (m_ObjectAmounts as ItemAmountsWithCategory)?.Refresh();
            base.Refresh();

            m_ItemFieldContainer.Clear();
            var index = m_ReorderableList.SelectedIndex;
            if (index < 0 || index >= m_ReorderableList.ItemsSource.Count) { return; }
            m_ItemFieldContainer.Add(m_ItemField);
            var itemAmount = (ItemAmount)m_ReorderableList.ItemsSource[index];
            if (m_ItemField.Value == itemAmount.Item) { return; }
            m_ItemField.Refresh(itemAmount.Item);
        }
    }
}