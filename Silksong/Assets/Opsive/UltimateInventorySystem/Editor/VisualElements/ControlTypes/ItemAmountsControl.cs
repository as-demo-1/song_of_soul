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
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using System.Reflection;
    using UnityEngine.UIElements;

    /// <summary>
    /// Implements TypeControlBase for the ObjectAmounts ControlType.
    /// </summary>
    [ControlType(typeof(ItemAmounts))]
    public class ItemAmountsControl : ControlWithInventoryDatabase
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
            return new ItemAmountsView(value as ItemAmounts, m_Database, onChangeEvent);
        }
    }

    /// <summary>
    /// Object Amounts view from ObjectAmountsBasView
    /// </summary>
    public class ItemAmountsView : InventoryObjectAmountsView<ItemAmounts, ItemAmount, Item>
    {
        protected VisualElement m_ItemFieldContainer;
        protected ItemField m_ItemField;
        protected int m_SelectedIndex;
        protected ItemDefinitionSearchableListWindow m_ItemDefinitionSearchableListWindow;

        /// <summary>
        /// Requires base constructor.
        /// </summary>
        /// <param name="objectAmounts">The object amounts.</param>
        /// <param name="database">The inventory system database.</param>
        /// <param name="onChangeEvent">The onChangeEvent function.</param>
        public ItemAmountsView(ItemAmounts objectAmounts, InventorySystemDatabase database, Func<object, bool> onChangeEvent) :
            base("Item Amounts", objectAmounts, database, onChangeEvent)
        {
            m_ItemFieldContainer = new VisualElement();
            Add(m_ItemFieldContainer);

            m_ItemDefinitionSearchableListWindow = new ItemDefinitionSearchableListWindow(database, QuickAddItem, null, false);

            m_ItemField = new ItemField(m_Database);
            m_ItemField.OnValueChanged += () =>
            {
                var index = m_ReorderableList.SelectedIndex;
                if (index < 0 || index >= m_ReorderableList.ItemsSource.Count) { return; }

                var previousValue = (ItemAmount)m_ReorderableList.ItemsSource[index];
                ChangeValue(index, (previousValue.Amount, m_ItemField.Value));

                if (index < 0 || index >= m_ReorderableList.ListItems.Count) { return; }

                var itemAmountView = m_ReorderableList.ListItems[index].ItemContents.ElementAt(0) as ItemAmountView;
                itemAmountView?.Refresh(false);

            };

            OnSelection += HandleSelection;
        }

        /// <summary>
        /// Add an item using the popup search field window.
        /// </summary>
        /// <param name="definition"></param>
        private void QuickAddItem(ItemDefinition definition)
        {
            var item = definition == null ? null : Item.Create(definition);
            m_ObjectAmounts.Add(new ItemAmount(item,1));
            ValueChanged();
            m_ReorderableList.SelectedIndex = m_ReorderableList.ItemsSource.Count - 1;
        }

        /// <summary>
        /// When the add button is pressed open the searchable list.
        /// </summary>
        protected override void Add()
        {
            var buttonWorldBound = m_ReorderableList.AddButton.worldBound;
            m_ItemDefinitionSearchableListWindow.OpenPopUpWindow(buttonWorldBound.position,buttonWorldBound.size);
        }

        /// <summary>
        /// Remove the element at the index.
        /// </summary>
        /// <param name="index">The index to remove at.</param>
        protected override void Remove(int index)
        {
            base.Remove(index);
            m_ItemFieldContainer.Clear();
        }

        /// <summary>
        /// Show the fields for the selection index.
        /// </summary>
        /// <param name="index">The index selected.</param>
        protected void HandleSelection(int index)
        {
            m_ItemFieldContainer.Clear();
            if (index < 0 || index >= m_ReorderableList.ItemsSource.Count) { return; }

            m_SelectedIndex = index;
            m_ItemFieldContainer.Add(m_ItemField);
            var itemAmount = (ItemAmount)m_ReorderableList.ItemsSource[index];
            m_ItemField.Refresh(itemAmount.Item);
        }

        /// <summary>
        /// Create the ObjectAmountView.
        /// </summary>
        /// <returns>The ObjectAmountView.</returns>
        protected override InventoryObjectAmountView<Item> CreateObjectAmountView()
        {
            return new ItemAmountView(m_Database);
        }

        /// <summary>
        /// Create a default ObjectAmount.
        /// </summary>
        /// <returns>The default ObjectAmount.</returns>
        protected override ItemAmount CreateObjectAmount(Item item)
        {
            return new ItemAmount(1, item);
        }

        /// <summary>
        /// Create a default ObjectAmounts
        /// </summary>
        /// <returns>The default ObjectAmounts.</returns>
        protected override ItemAmounts CreateObjectAmounts()
        {
            return new ItemAmounts();
        }
    }

    /// <summary>
    /// ObjectAmounts View from ObjectAmountBaseView
    /// </summary>
    public class ItemAmountView : InventoryObjectAmountView<Item>
    {
        protected ItemViewName m_ItemViewName;

        protected override Item ObjectFieldValue {
            get => m_ItemViewName?.Item;
            set => m_ItemViewName.Refresh(value);
        }

        /// <summary>
        /// The item amount view.
        /// </summary>
        /// <param name="database">The database.</param>
        public ItemAmountView(InventorySystemDatabase database) : base(database)
        {
            m_ItemViewName = new ItemViewName();
            Add(m_ItemViewName);
            m_ItemViewName.Refresh();
        }

        /// <summary>
        /// Refresh the view.
        /// </summary>
        /// <param name="notify">Send an event that the value changed.</param>
        public override void Refresh(bool notify)
        {
            base.Refresh(notify);
            m_ItemViewName.Refresh(m_ObjectAmount.Object);
        }

        /// <summary>
        /// Refresh the object Icon.
        /// </summary>
        public override void RefreshInternal()
        {
            m_ItemViewName.Refresh(m_ObjectAmount.Object);
        }

        /// <summary>
        /// Create an objectAmount.
        /// </summary>
        /// <param name="obj">The new Object.</param>
        /// <param name="amount">The new Amount.</param>
        /// <returns>The ObjectAmount.</returns>
        public override IObjectAmount<Item> CreateNewObjectAmount(Item obj, int amount)
        {
            return new ItemAmount(obj, amount);
        }

        /// <summary>
        /// Show the Type in the name
        /// </summary>
        /// <param name="showType">show or hide type.</param>
        public override void SetShowType(bool showType)
        {
            m_ShowType = showType;
            m_ItemViewName.SetShowType(showType);
        }
    }
}