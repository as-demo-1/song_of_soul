/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements.ControlTypes
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using UnityEngine.UIElements;

    /// <summary>
    /// Base class for the Object Amounts Views
    /// </summary>
    /// <typeparam name="Tamounts">The Type of ObjectAmounts.</typeparam>
    /// <typeparam name="Tamount">The Type of ObjectAmount.</typeparam>
    /// <typeparam name="T">The Object Type.</typeparam>
    public abstract class InventoryObjectAmountsView<Tamounts, Tamount, T> : VisualElement
        where Tamounts : IObjectAmounts<Tamount>
        where Tamount : IObjectAmount<T>
        where T : class
    {
        public event Action<int> OnSelection;

        protected Label m_Label;
        protected Tamounts m_ObjectAmounts;
        protected Func<object, bool> m_OnChangeEvent;
        protected ReorderableList m_ReorderableList;
        protected InventoryObjectAmountView<T> m_InventoryObjectAmountView;
        protected InventorySystemDatabase m_Database;
        protected InventoryObjectSearchableListWindow<T> m_SearchableListWindow;

        public InventoryObjectAmountView<T> InventoryObjectAmountView => m_InventoryObjectAmountView;
        public InventoryObjectSearchableListWindow<T> SearchableListWindow
        {
            get { return m_SearchableListWindow; }
            set => m_SearchableListWindow = value;
        }

        /// <summary>
        /// Constructor to setup the view.
        /// </summary>
        /// <param name="label">The label for the field.</param>
        /// <param name="objectAmounts">The default value.</param>
        /// <param name="database">The inventory system database.</param>
        /// <param name="onChangeEvent">The onChangeEvent function.</param>
        public InventoryObjectAmountsView(string label, Tamounts objectAmounts, InventorySystemDatabase database, Func<object, bool> onChangeEvent)
        {
            m_ObjectAmounts = objectAmounts;
            m_Database = database;
            m_OnChangeEvent = onChangeEvent;
            if (m_ObjectAmounts == null) { m_ObjectAmounts = CreateObjectAmounts(); }

            m_Label = new Label(label);

            m_ReorderableList = new ReorderableList(m_ObjectAmounts.Array, (VisualElement parent, int index) =>
            {
                m_InventoryObjectAmountView = CreateObjectAmountView();
                m_InventoryObjectAmountView.OnValueChanged += objectAmount =>
                {
                    ChangeValue(index, (Tamount)objectAmount);
                };
                parent.Add(m_InventoryObjectAmountView);
            }, (VisualElement parent, int index) =>
            {
                if (index < 0 || index >= m_ObjectAmounts.Array.Length) {
                    (parent.ElementAt(0) as InventoryObjectAmountView<T>).Refresh(CreateObjectAmount(null));
                } else {
                    (parent.ElementAt(0) as InventoryObjectAmountView<T>).Refresh((m_ObjectAmounts.Array[index]));
                }

            }, (parent) =>
            {
                parent.Add(m_Label);
            }, (index) => OnSelection?.Invoke(index),
            Add, Remove,
            (int i1, int i2) =>
            {
                ValueChanged();
            });
            Add(m_ReorderableList);
        }

        /// <summary>
        /// Add an object amount to the list.
        /// </summary>
        /// <param name="obj">The object to add.</param>
        protected virtual void AddObjectAmount(T obj)
        {
            m_ObjectAmounts.Add(CreateObjectAmount(obj));
            ValueChanged();
            m_ReorderableList.SelectedIndex = m_ReorderableList.ItemsSource.Count - 1;
        }

        /// <summary>
        /// Add an the element.
        /// </summary>
        protected virtual void Add()
        {
            if (SearchableListWindow == null) {
                AddObjectAmount(null);
            } else {
                var buttonWorldBound = m_ReorderableList.AddButton.worldBound;
                m_SearchableListWindow.OpenPopUpWindow(buttonWorldBound.position,buttonWorldBound.size);
            }
        }

        /// <summary>
        /// Remove the element at the index.
        /// </summary>
        /// <param name="index">The index to remove at.</param>
        protected virtual void Remove(int index)
        {
            m_ObjectAmounts.RemoveAt(index);
            ValueChanged();
        }

        /// <summary>
        /// Change th value of an element in the list.
        /// </summary>
        /// <param name="index">The element index.</param>
        /// <param name="objectAmount">The new object amount.</param>
        public void ChangeValue(int index, Tamount objectAmount)
        {
            if (index < 0 || index >= m_ObjectAmounts.Array.Length) {
                ValueChanged();
                return;
            }

            m_ObjectAmounts.Array[index] = objectAmount;
            ValueChanged();
        }

        /// <summary>
        /// Value Changed.
        /// </summary>
        protected void ValueChanged()
        {
            Refresh();
            m_OnChangeEvent(m_ObjectAmounts);
        }

        /// <summary>
        /// Change the object amounts.
        /// </summary>
        /// <param name="objectAmounts">The object amounts.</param>
        public void Refresh(Tamounts objectAmounts)
        {
            m_ObjectAmounts = objectAmounts;
            Refresh();
        }

        /// <summary>
        /// Redraw the list.
        /// </summary>
        public virtual void Refresh()
        {
            m_ReorderableList.Refresh(m_ObjectAmounts.Array);
        }

        /// <summary>
        /// Create the ObjectAmountView.
        /// </summary>
        /// <returns>The ObjectAmountView.</returns>
        protected abstract InventoryObjectAmountView<T> CreateObjectAmountView();

        /// <summary>
        /// Create a default CurrencyAmount.
        /// </summary>
        /// <returns>The default CurrencyAmount.</returns>
        protected abstract Tamount CreateObjectAmount(T obj);

        /// <summary>
        /// Create a default CurrencyAmounts
        /// </summary>
        /// <returns>The default CurrencyAmounts.</returns>
        protected abstract Tamounts CreateObjectAmounts();
    }
}