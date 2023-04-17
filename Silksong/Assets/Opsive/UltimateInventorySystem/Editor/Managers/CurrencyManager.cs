/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Managers;
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using Opsive.UltimateInventorySystem.Editor.Utility;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ControlTypes;
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames;
    using Opsive.UltimateInventorySystem.Exchange;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// The currency manager.
    /// </summary>
    [OrderedEditorItem("Currency", 60)]
    [RequireDatabase]
    [System.Serializable]
    public class CurrencyManager : InventorySystemObjectBaseManager<Currency>
    {
        protected CurrencyField m_BaseCurrencyField;
        protected DoubleField m_ExchangeRateToParentField;
        protected IntegerField m_MaxAmountField;
        protected UnityObjectFieldWithPreview m_IconField;
        protected CurrencyField m_OverflowCurrencyField;
        protected CurrencyField m_FractionCurrencyField;
        protected TabToolbar m_RelationshipsTabToolbar;
        protected ReorderableList m_RelationshipsReorderableList;

        /// <summary>
        /// Adds the visual elements to the ManagerContentContainer visual element. 
        /// </summary>
        public override void BuildVisualElements()
        {
            var list = GetList();
            if (list != null) {
                for (int i = 0; i < list.Count; i++) { list[i].Initialize(false); }
            }

            base.BuildVisualElements();

            m_IconField = new UnityObjectFieldWithPreview();
            m_IconField.label = "Icon";
            m_IconField.tooltip = "The currency icon which can be accessed in game.";
            m_IconField.objectType = typeof(Sprite);
            m_IconField.RegisterValueChangedCallback(evt =>
            {
                CurrencyEditorUtility.SetIcon(SelectedObject, evt.newValue as Sprite);
                Refresh();
            });
            m_ContentPanel.Add(m_IconField);

            m_BaseCurrencyField = new CurrencyField(
                "Base Currency",
                m_InventoryMainWindow.Database,
                new (string, Action<Currency>)[]
                {
                    ("Set Base Currency",c => { CurrencyEditorUtility.SetParent(SelectedObject, c); }),
                },
                (currency) => SelectedObject?.SetParentCondition(currency) ?? true);

            m_BaseCurrencyField.OnClose += () =>
            {
                Refresh();
            };
            m_BaseCurrencyField.tooltip = "The base currency which will define the currency family and the exchange rates.";
            m_ContentPanel.Add(m_BaseCurrencyField);

            m_ExchangeRateToParentField = new DoubleField("Base Exchange Rate");
            m_ExchangeRateToParentField.AddToClassList("flex-grow");
            m_ExchangeRateToParentField.RegisterValueChangedCallback(evt =>
            {
                if (SelectedObject.SetParentExchangeRateCondition(evt.newValue)) {
                    CurrencyEditorUtility.SetExchangeRateToParent(SelectedObject, evt.newValue);
                    Refresh();
                } else { m_ExchangeRateToParentField.value = evt.previousValue; }

            });
            m_ExchangeRateToParentField.tooltip = "The exchange with the base currency (must be positive).";
            m_ContentPanel.Add(m_ExchangeRateToParentField);

            m_MaxAmountField = new IntegerField("Max Amount");
            m_MaxAmountField.AddToClassList("flex-grow");
            m_MaxAmountField.RegisterValueChangedCallback(evt =>
            {
                if (SelectedObject.SetMaxAmountCondition(evt.newValue)) {
                    CurrencyEditorUtility.SetMaxAmount(SelectedObject, evt.newValue);
                } else { m_ExchangeRateToParentField.value = evt.previousValue; }

            });
            m_MaxAmountField.tooltip = "The max amount of that currency that can be hold in a currencyCollection.";
            m_ContentPanel.Add(m_MaxAmountField);

            m_OverflowCurrencyField = new CurrencyField(
                "Overflow Currency",
                m_InventoryMainWindow.Database,
                new (string, Action<Currency>)[]
                {
                    ("Set Overflow Currency",c => {CurrencyEditorUtility.SetOverflowCurrency(SelectedObject, c); })
                },
                (currency) => SelectedObject?.SetOverflowCurrencyCondition(currency) ?? true);

            m_OverflowCurrencyField.OnClose += () =>
            {
                Refresh();
            };
            m_OverflowCurrencyField.tooltip =
                "The currency which this currency will overflow to when going above its max amount.";
            m_ContentPanel.Add(m_OverflowCurrencyField);

            m_FractionCurrencyField = new CurrencyField(
                "Fraction Currency",
                m_InventoryMainWindow.Database,
                new (string, Action<Currency>)[]
                {
                    ("Set Fraction Currency",c => {CurrencyEditorUtility.SetFractionCurrency(SelectedObject, c); })
                },
                (currency) => SelectedObject?.SetFractionCurrencyCondition(currency) ?? true);

            m_FractionCurrencyField.OnClose += () =>
            {
                Refresh();
            };
            m_FractionCurrencyField.tooltip = "The currency to fraction to when subtracted by a smaller valued currency.";
            m_ContentPanel.Add(m_FractionCurrencyField);

            var attributesBox = new VisualElement();
            attributesBox.name = "box";
            attributesBox.AddToClassList(ManagerStyles.BoxBackground);

            var relationshipBox = new VisualElement();
            relationshipBox.name = "box";
            relationshipBox.AddToClassList(ManagerStyles.BoxBackground);

            m_RelationshipsTabToolbar = new TabToolbar(new string[]
            {
                "Exchange Rates"
            }, 0, ShowExchangeRatesTab);
            relationshipBox.Add(m_RelationshipsTabToolbar);

            m_RelationshipsReorderableList = new ReorderableList(null, (VisualElement parent, int index) =>
            {
                parent.Add(new CurrencyFamilyItemView());
            }, (VisualElement parent, int index) =>
            {
                var currencyFamilyItemView = parent.ElementAt(0) as CurrencyFamilyItemView;
                var otherCurrency = m_RelationshipsReorderableList.ItemsSource[index] as Currency;
                if (otherCurrency == null) { return; }

                currencyFamilyItemView.Refresh(SelectedObject, otherCurrency);
            }, null, null, null, null, null);
            relationshipBox.Add(m_RelationshipsReorderableList);
            m_ContentPanel.Add(relationshipBox);

            m_ListPanel.Refresh(-1);
        }

        /// <summary>
        /// Returns true if the definition name is unique.
        /// </summary>
        /// <param name="name">The possible name of the definition.</param>
        /// <returns>True if the definition name is unique.</returns>
        public override bool IsObjectNameValidAndUnique(string name)
        {
            for (int i = 0; i < m_InventoryMainWindow.Database.Currencies.Length; ++i) {
                if (m_InventoryMainWindow.Database.Currencies[i].name == name) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Refreshes the content for the current database.
        /// </summary>
        public override void Refresh()
        {
            if (m_SelectedIndex == -1) {
                m_SelectedIndex = 0;
            }
            OnSelected(m_SelectedIndex);
        }

        /// <summary>
        /// Returns the list that the ReorderableList should use.
        /// </summary>
        /// <returns>The list that the ReorderableList should use.</returns>
        public override IList<Currency> GetList()
        {
            return m_InventoryMainWindow.Database?.Currencies;
        }

        /// <summary>
        /// Update the visual elements to reflect the specified category.
        /// </summary>
        /// <param name="currency">The currency that is being displayed.</param>
        protected override void UpdateElements(Currency currency)
        {
            if (currency == null) { return; }
            currency.Initialize(false);

            m_Name.SetValueWithoutNotify(currency.name);
            m_IconField.SetValueWithoutNotify(currency.Icon);
            m_IconField.Refresh();
            m_BaseCurrencyField.Refresh(currency.Parent);
            m_ExchangeRateToParentField.SetValueWithoutNotify(currency.ExchangeRateToParent);
            m_MaxAmountField.SetValueWithoutNotify(currency.MaxAmount);
            m_OverflowCurrencyField.Refresh(currency.OverflowCurrency);
            m_FractionCurrencyField.Refresh(currency.FractionCurrency);

            if (m_RelationshipsTabToolbar.Selected < 0) { m_RelationshipsTabToolbar.Selected = 0; }
            ShowExchangeRatesTab(m_RelationshipsTabToolbar.Selected);
        }

        /// <summary>
        /// Shows the exchange rates that correspond to the tab at the specified index.
        /// </summary>
        /// <param name="index">The tab index.</param>
        private void ShowExchangeRatesTab(int index)
        {
            var currency = SelectedObject;

            var currencies = GenericObjectPool.Get<Currency[]>();
            var count = currency.GetAllFamily(ref currencies);
            var family = new List<Currency>();
            for (int i = 0; i < count; i++) {
                family.Add(currencies[i]);
            }
            family.Sort((x, y) =>
            {
                if (x == null) { return -1; }
                if (y == null) { return 1; }
                return x.GetRootExchangeRate().ExchangeRate > y.GetRootExchangeRate().ExchangeRate ? -1 : 1;
            });
            GenericObjectPool.Return(currencies);

            IList list;
            switch (index) {
                case 0: list = family; break; // Family relations.
                default: list = null; break;
            }

            m_RelationshipsReorderableList.Refresh(list);
        }

        /// <summary>
        /// Get the currency family with their relationships.
        /// </summary>
        /// <param name="currency">The currency.</param>
        /// <param name="currencies">The related currencies.</param>
        /// <param name="includeThis">Include the currency.</param>
        /// <returns>A list of relationships.</returns>
        public static List<(string, Currency)> GetCurrencyFamilyWithRelationships(Currency currency, IList<Currency> currencies, bool includeThis)
        {
            var family = new List<(string, Currency)>();
            var rootCurrency = currency.GetRoot();
            var baseCurrency = currency.Parent != null ? currency.Parent : currency;

            if (includeThis) {
                family.Add(("This", currency));
            }

            family.Add(("Root", rootCurrency));

            family.Add(("Base", baseCurrency));

            for (var i = 0; i < currencies.Count; i++) {
                var otherCurrency = currencies[i];
                if (otherCurrency == currency
                    || otherCurrency == rootCurrency
                    || otherCurrency == baseCurrency) { continue; }
                if (rootCurrency == otherCurrency.GetRoot()) { family.Add(("C" + i, otherCurrency)); }
            }

            return family;
        }

        /// <summary>
        /// The add button has been pressed.
        /// </summary>
        public override Currency OnAdd(string name)
        {
            return CurrencyEditorUtility.AddCurrency(name,
                m_InventoryMainWindow.Database,
                m_InventoryMainWindow.GetDatabaseDirectory());
        }

        /// <summary>
        /// The remove button has been pressed.
        /// </summary>
        /// <param name="index">The index of the selected object.</param>
        public override void OnRemove(int index)
        {
            // No more work needs to be performed if the object is empty.
            if (SelectedIndexOutOfRange) { return; }

            string warningMessage;
            if (SelectedObject.Children.Count == 0) {
                warningMessage = $"Are you sure you want to remove the Currency '{SelectedObject}'? This action cannot be undone.";
            } else if (SelectedObject.Children.Count == 1) {
                warningMessage = $"Are you sure you want to remove the Currency '{SelectedObject}' with 1 child? This action cannot be undone.";
            } else {
                warningMessage = $"Are you sure you want to remove the Currency '{SelectedObject}' with {SelectedObject.Children.Count} children? " +
                                 "This action cannot be undone.";
            }

            if (EditorUtility.DisplayDialog("Remove Currency?", warningMessage, "Yes", "No")) {
                CurrencyEditorUtility.RemoveCurrency(SelectedObject, m_InventoryMainWindow.Database);
                OnSelected(-1);
            }
        }

        /// <summary>
        /// Build the contextual menu when right-clicking a definition.
        /// </summary>
        /// <param name="evt">The event context.</param>
        void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Duplicate", DuplicateObject, DropdownMenuAction.AlwaysEnabled, evt.target);
        }

        /// <summary>
        /// Duplicate the item definition using the right click.
        /// </summary>
        /// <param name="action">The drop down menu action.</param>
        void DuplicateObject(DropdownMenuAction action)
        {
            var viewName = action.userData as CurrencyViewName;
            var objectToDuplicate = viewName?.Currency;

            var newObject = CurrencyEditorUtility.DuplicateCurrency(objectToDuplicate,
                m_InventoryMainWindow.Database,
                m_InventoryMainWindow.GetDatabaseDirectory());

            Refresh();
            Select(newObject);
        }

        /// <summary>
        /// Creates the new ReorderableList item.
        /// </summary>
        /// <param name="parent">The parent ReorderableList item.</param>
        /// <param name="index">The index of the item.</param>
        public override void MakeListItem(VisualElement parent, int index)
        {
            var viewName = new CurrencyViewName();
            viewName.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
            parent.Add(viewName);
        }

        /// <summary>
        /// Binds the ReorderableList item to the specified index.
        /// </summary>
        /// <param name="parent">The ReorderableList item that is being bound.</param>
        /// <param name="index">The index of the item.</param>
        public override void BindListItem(VisualElement parent, int index)
        {
            var currencyViewName = parent.ElementAt(0) as CurrencyViewName;
            if (index < 0 || index >= m_ListPanel.SearchableList.ItemList.Count) { return; }
            var currency = m_ListPanel.SearchableList.ItemList[index];
            currencyViewName.Refresh(currency);
        }

        /// <summary>
        /// Get the sort options for the currency.
        /// </summary>
        /// <returns>A list of sort options.</returns>
        public override IList<SortOption> GetSortOptions()
        {
            return CurrencyEditorUtility.SortOptions();
        }

        /// <summary>
        /// Search filter for the ItemDefinition list
        /// </summary>
        /// <param name="list">The list to search.</param>
        /// <param name="searchValue">The search string.</param>
        /// <returns>A new filtered list.</returns>
        public override IList<Currency> GetSearchFilter(IList<Currency> list, string searchValue)
        {
            return CurrencyEditorUtility.SearchFilter(list, searchValue);
        }
    }

    /// <summary>
    /// The family view of a currency.
    /// </summary>
    public class CurrencyFamilyItemView : VisualElement
    {
        protected Currency m_Currency1;
        protected Currency m_Currency2;

        protected CurrencyViewName m_Currency1ViewName;
        protected CurrencyViewName m_Currency2ViewName;

        protected Label m_ExchangeRateLabel;
        protected Label m_Currency2SpecialRelationsLabel;

        /// <summary>
        /// Create teh family item view.
        /// </summary>
        public CurrencyFamilyItemView()
        {
            AddToClassList(InventoryManagerStyles.CurrencyFamilyItemView);

            var leftSideContainer = new VisualElement();
            leftSideContainer.AddToClassList(InventoryManagerStyles.CurrencyFamilyItemView_LeftSide);

            var label1 = new Label("1");
            leftSideContainer.Add(label1);

            m_Currency1ViewName = new CurrencyViewName();
            leftSideContainer.Add(m_Currency1ViewName);

            m_ExchangeRateLabel = new Label();
            leftSideContainer.Add(m_ExchangeRateLabel);

            Add(leftSideContainer);

            var rightSideContainer = new VisualElement();
            rightSideContainer.AddToClassList(InventoryManagerStyles.CurrencyFamilyItemView_RightSide);

            m_Currency2ViewName = new CurrencyViewName();
            rightSideContainer.Add(m_Currency2ViewName);

            m_Currency2SpecialRelationsLabel = new Label();
            rightSideContainer.Add(m_Currency2SpecialRelationsLabel);

            Add(rightSideContainer);
        }

        /// <summary>
        /// Change the related currency.
        /// </summary>
        /// <param name="c1">The viewed currency.</param>
        /// <param name="c2">The related currency.</param>
        public void Refresh(Currency c1, Currency c2)
        {
            m_Currency1 = c1;
            m_Currency2 = c2;
            Refresh();
        }

        /// <summary>
        /// Redraw the view.
        /// </summary>
        public void Refresh()
        {
            m_Currency1ViewName.Refresh(m_Currency1);
            m_Currency2ViewName.Refresh(m_Currency2);

            if (m_Currency1.TryGetExchangeRateTo(m_Currency2, out var exchangeRate)) {
                m_ExchangeRateLabel.text = string.Format("= {0} ", exchangeRate);
            } else { m_ExchangeRateLabel.text = "= ? "; }

            var specialRelations = "";
            if (m_Currency2 == m_Currency1.GetRoot()) { specialRelations += "Root | "; }
            if (m_Currency2 == m_Currency1.Parent) { specialRelations += "Base | "; }
            if (m_Currency2 == m_Currency1.OverflowCurrency) { specialRelations += "Overflow | "; }
            if (m_Currency2 == m_Currency1.FractionCurrency) { specialRelations += "Fraction | "; }

            if (string.IsNullOrEmpty(specialRelations)) {
                m_Currency2SpecialRelationsLabel.text = "";
            } else {
                m_Currency2SpecialRelationsLabel.text = $"({specialRelations.Remove(specialRelations.Length - 3)})";
            }
        }
    }
}