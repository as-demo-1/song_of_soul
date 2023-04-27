/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Inspectors
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.Exchange;
    using Opsive.UltimateInventorySystem.Storage;
    using Opsive.UltimateInventorySystem.UI.Currency;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;

    /// <summary>
    /// Custom editor to display the category item actions.
    /// </summary>
    [CustomEditor(typeof(MultiCurrencyView), true)]
    public class MultiCurrencyViewInspector : DatabaseInspectorBase
    {
        protected override List<string> ExcludedFields => new List<string>() { "m_CurrenciesWithViews" };

        protected MultiCurrencyView m_MultiCurrencyView;

        protected List<CurrencyWithView> m_List;
        protected ReorderableList m_ReorderableList;

        /// <summary>
        /// Initialize the inspector when it is first selected.
        /// </summary>
        protected override void InitializeInspector()
        {
            m_MultiCurrencyView = target as MultiCurrencyView;

            if (m_MultiCurrencyView.m_CurrenciesWithViews == null) {
                m_MultiCurrencyView.m_CurrenciesWithViews = new CurrencyWithView[0];
                return;
            }

            base.InitializeInspector();
        }

        /// <summary>
        /// Create the inspector.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {

            m_List = new List<CurrencyWithView>(m_MultiCurrencyView.m_CurrenciesWithViews);
            m_ReorderableList = new ReorderableList(
                m_List,
                (parent, index) =>
                {
                    var listElement = new ListElement(m_Database);
                    listElement.Index = index;
                    listElement.OnValueChanged += OnValueChanged;
                    parent.Add(listElement);
                }, (parent, index) =>
                {
                    var listElement = parent.ElementAt(0) as ListElement;

                    listElement.Index = index;
                    listElement.Refresh(m_List[index]);
                }, (parent) =>
                {
                    parent.Add(new Label("Currencies With Views"));
                }, (index) => { return 50f; },
                (index) =>
                {
                    //nothing
                }, () =>
                {
                    m_List.Add(default);

                    UpdateOriginalArray();
                    m_ReorderableList.Refresh(m_List);
                }, (index) =>
                {
                    if (index < 0 || index >= m_List.Count) { return; }

                    m_List.RemoveAt(index);

                    UpdateOriginalArray();
                    m_ReorderableList.Refresh(m_List);
                }, (i1, i2) =>
                {
                    var element1 = m_ReorderableList.ListItems[i1].ItemContents.ElementAt(0) as ListElement;
                    element1.Index = i1;
                    var element2 = m_ReorderableList.ListItems[i2].ItemContents.ElementAt(0) as ListElement;
                    element2.Index = i2;
                    UpdateOriginalArray();
                });
            container.Add(m_ReorderableList);
        }

        private void UpdateOriginalArray()
        {
            m_MultiCurrencyView.m_CurrenciesWithViews = m_List.ToArray();
            Shared.Editor.Utility.EditorUtility.SetDirty(m_MultiCurrencyView);
        }

        private void OnValueChanged(int index, CurrencyWithView value)
        {
            m_List[index] = value;
            UpdateOriginalArray();
            m_ReorderableList.Refresh(m_List);
        }

        public class ListElement : VisualElement
        {
            public event Action<int, CurrencyWithView> OnValueChanged;
            public int Index { get; set; }

            protected CurrencyField m_CurrencyObjectField;
            protected ObjectField m_CurrencyView;

            /// <summary>
            /// The list element.
            /// </summary>
            public ListElement(InventorySystemDatabase database)
            {
                m_CurrencyObjectField = new CurrencyField(
                    "Currency",
                    database, new (string, Action<Currency>)[]
                    {
                        ("Set Currency", (x) =>
                        {
                            OnValueChanged?.Invoke(Index,new CurrencyWithView(x,m_CurrencyView.value as CurrencyView));
                        })
                    },
                    (x) => true);
                Add(m_CurrencyObjectField);

                m_CurrencyView = new ObjectField("View");
                m_CurrencyView.objectType = typeof(CurrencyView);
                m_CurrencyView.RegisterValueChangedCallback(evt =>
                {
                    OnValueChanged?.Invoke(Index, new CurrencyWithView(m_CurrencyObjectField.Value, evt.newValue as CurrencyView));
                });
                Add(m_CurrencyView);
            }

            /// <summary>
            /// Update the visuals.
            /// </summary>
            /// <param name="value">The new value.</param>
            public void Refresh(CurrencyWithView value)
            {
                m_CurrencyObjectField.Refresh(value.Currency);
                m_CurrencyView.SetValueWithoutNotify(value.View);
            }
        }
    }
}