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
    [ControlType(typeof(ItemAmountsWithDefinition))]
    public class ItemAmountsWithDefinitionControl : ControlWithInventoryDatabase
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
            return new ItemAmountsWithDefinitionView(value as ItemAmountsWithDefinition, m_Database, onChangeEvent);
        }
    }

    /// <summary>
    /// Object Amounts view from ObjectAmountsBasView
    /// </summary>
    public class ItemAmountsWithDefinitionView : ItemAmountsView
    {
        protected Toggle m_Inherently;
        protected ItemDefinitionField m_ItemDefinitionField;

        /// <summary>
        /// Requires base constructor.
        /// </summary>
        /// <param name="objectAmounts">The object amounts.</param>
        /// <param name="database">The inventory system database.</param>
        /// <param name="onChangeEvent">The onChangeEvent function.</param>
        public ItemAmountsWithDefinitionView(ItemAmountsWithDefinition objectAmounts, InventorySystemDatabase database, Func<object, bool> onChangeEvent) :
            base(objectAmounts, database, onChangeEvent)
        {
            objectAmounts = m_ObjectAmounts as ItemAmountsWithDefinition;

            bool PrefilterCondition(ItemDefinition itemDefinition)
            {
                return (m_ObjectAmounts as ItemAmountsWithDefinition)?.Condition(itemDefinition) ?? false;
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

            m_ItemDefinitionField = new ItemDefinitionField("Item Category", database,
                definition =>
                {
                    objectAmounts.ItemDefinition = definition;
                    m_ItemDefinitionField.Refresh(definition);
                    m_ItemField.SetPreFilter(PrefilterCondition);
                    Refresh();
                },
                definition => true);
            m_ItemDefinitionField.Refresh(objectAmounts.ItemDefinition);
            Add(m_ItemDefinitionField);
            m_ItemDefinitionField.SendToBack();

            m_ItemField.SetPreFilter(PrefilterCondition);
            m_ItemDefinitionSearchableListWindow?.SetPreFilter(PrefilterCondition);
        }

        /// <summary>
        /// Create a default ObjectAmounts
        /// </summary>
        /// <returns>The default ObjectAmounts.</returns>
        protected override ItemAmounts CreateObjectAmounts()
        {
            return new ItemAmountsWithDefinition(null, true);
        }

        /// <summary>
        /// Redraw the list.
        /// </summary>
        public override void Refresh()
        {
            (m_ObjectAmounts as ItemAmountsWithDefinition)?.Refresh();
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