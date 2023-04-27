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
    [ControlType(typeof(ItemDefinitionAmountsWithCategory))]
    public class ItemDefinitionAmountsWithCategoryControl : ControlWithInventoryDatabase
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
            return new ItemDefinitionAmountsWithCategoryView(value as ItemDefinitionAmountsWithCategory, m_Database, onChangeEvent);
        }
    }

    /// <summary>
    /// Object Amounts view from ObjectAmountsBasView
    /// </summary>
    public class ItemDefinitionAmountsWithCategoryView : ItemDefinitionAmountsView
    {
        protected Toggle m_Inherently;
        protected ItemCategoryField m_ItemCategoryField;

        /// <summary>
        /// Requires base constructor.
        /// </summary>
        /// <param name="objectAmounts">The object amounts.</param>
        /// <param name="database">The inventory system database.</param>
        /// <param name="onChangeEvent">The onChangeEvent function.</param>
        public ItemDefinitionAmountsWithCategoryView(ItemDefinitionAmountsWithCategory objectAmounts, InventorySystemDatabase database, Func<object, bool> onChangeEvent) :
            base(objectAmounts, database, onChangeEvent)
        {
            objectAmounts = m_ObjectAmounts as ItemDefinitionAmountsWithCategory;

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
                    (m_InventoryObjectAmountView as ItemDefinitionAmountView)?.ItemDefinitionField.SetPreFilter(PrefilterCondition);
                    Refresh();
                },
                category => true);
            m_ItemCategoryField.Refresh(objectAmounts.ItemCategory);
            Add(m_ItemCategoryField);
            m_ItemCategoryField.SendToBack();

            (m_InventoryObjectAmountView as ItemDefinitionAmountView)?.ItemDefinitionField.SetPreFilter(PrefilterCondition);
            m_SearchableListWindow?.SetPreFilter(PrefilterCondition);
        }

        /// <summary>
        /// The pre filter function.
        /// </summary>
        /// <param name="itemDefinition">The item definition.</param>
        /// <returns>True if it can be added.</returns>
        bool PrefilterCondition(ItemDefinition itemDefinition)
        {
            return (m_ObjectAmounts as ItemDefinitionAmountsWithCategory)?.Condition(itemDefinition) ?? false;
        }

        /// <summary>
        /// Create the ObjectAmountView.
        /// </summary>
        /// <returns>The ObjectAmountView.</returns>
        protected override InventoryObjectAmountView<ItemDefinition> CreateObjectAmountView()
        {
            var newItemDefinitionAmountView = new ItemDefinitionAmountView(m_Database);
            newItemDefinitionAmountView.ItemDefinitionField.SetPreFilter(PrefilterCondition);
            return newItemDefinitionAmountView;
        }

        /// <summary>
        /// Create a default ObjectAmounts
        /// </summary>
        /// <returns>The default ObjectAmounts.</returns>
        protected override ItemDefinitionAmounts CreateObjectAmounts()
        {
            return new ItemDefinitionAmountsWithCategory(null, true);
        }

        /// <summary>
        /// Redraw the list.
        /// </summary>
        public override void Refresh()
        {
            (m_ObjectAmounts as ItemDefinitionAmountsWithCategory)?.Refresh();
            base.Refresh();
        }
    }
}