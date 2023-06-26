/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements.ControlTypes
{
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Exchange;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using System.Reflection;
    using UnityEngine.UIElements;

    /// <summary>
    /// Implements TypeControlBase for the CurrencyAmounts ControlType.
    /// </summary>
    [ControlType(typeof(CurrencyAmounts))]
    public class CurrencyAmountsControl : ControlWithInventoryDatabase
    {
        /// <summary>
        /// Create the CurrencyAmountsView.
        /// </summary>
        /// <param name="value">The default value.</param>
        /// <param name="onChangeEvent">The on change event function.</param>
        /// <param name="field"></param>
        /// <returns>The currencyAmountsView.</returns>
        public override VisualElement CreateCustomControlVisualElement(object value, Func<object, bool> onChangeEvent,
            FieldInfo field)
        {
            return new CurrencyAmountsView(value as CurrencyAmounts, m_Database, onChangeEvent);
        }
    }

    /// <summary>
    /// Currency Amounts view from ObjectAmountsBasView
    /// </summary>
    public class CurrencyAmountsView : InventoryObjectAmountsView<CurrencyAmounts, CurrencyAmount, Currency>
    {

        /// <summary>
        /// Requires base constructor.
        /// </summary>
        /// <param name="objectAmounts">The object amounts.</param>
        /// <param name="database">The inventory system database.</param>
        /// <param name="onChangeEvent">The onChangeEvent function.</param>
        public CurrencyAmountsView(CurrencyAmounts objectAmounts, InventorySystemDatabase database, Func<object, bool> onChangeEvent) :
            base("Currency Amounts", objectAmounts, database, onChangeEvent)
        {
            m_SearchableListWindow = new CurrencySearchableListWindow(database, AddObjectAmount, null, false);
        }

        /// <summary>
        /// Create the ObjectAmountView.
        /// </summary>
        /// <returns>The ObjectAmountView.</returns>
        protected override InventoryObjectAmountView<Currency> CreateObjectAmountView()
        {
            return new CurrencyAmountView(m_Database);
        }

        /// <summary>
        /// Create a default CurrencyAmount.
        /// </summary>
        /// <returns>The default CurrencyAmount.</returns>
        protected override CurrencyAmount CreateObjectAmount(Currency currency)
        {
            return new CurrencyAmount(1, currency);
        }

        /// <summary>
        /// Create a default CurrencyAmounts
        /// </summary>
        /// <returns>The default CurrencyAmounts.</returns>
        protected override CurrencyAmounts CreateObjectAmounts()
        {
            return new CurrencyAmounts();
        }
    }

    /// <summary>
    /// CurrencyAmounts View from ObjectAmountBaseView
    /// </summary>
    public class CurrencyAmountView : InventoryObjectAmountView<Currency>
    {
        protected CurrencyField m_CurrencyField;

        protected override Currency ObjectFieldValue {
            get => m_CurrencyField?.Value as Currency;
            set => m_CurrencyField.Refresh(value);
        }

        /// <summary>
        /// Create a currency amount view.
        /// </summary>
        /// <param name="database">The database.</param>
        public CurrencyAmountView(InventorySystemDatabase database) : base(database)
        {
            m_CurrencyField = new CurrencyField("", m_Database, new (string, Action<Currency>)[]
            {
                ("Set Currency", (x) =>InvokeOnValueChanged(CreateNewObjectAmount(x,m_IntegerField.value)))
            }, (x) => true);
            Add(m_CurrencyField);
            m_CurrencyField.Refresh();
        }

        /// <summary>
        /// Refresh the object Icon.
        /// </summary>
        public override void RefreshInternal()
        {
            m_CurrencyField.Refresh(m_ObjectAmount.Object);
        }

        /// <summary>
        /// Create an objectAmount.
        /// </summary>
        /// <param name="obj">The new Object.</param>
        /// <param name="amount">The new Amount.</param>
        /// <returns>The ObjectAmount.</returns>
        public override IObjectAmount<Currency> CreateNewObjectAmount(Currency obj, int amount)
        {
            return new CurrencyAmount(obj, amount);
        }

        /// <summary>
        /// Set if the Amount View is interactable
        /// </summary>
        /// <param name="interactable">true if interactable.</param>
        public override void SetInteractable(bool interactable)
        {
            m_Interactable = interactable;

            m_IntegerField.SetEnabled(m_Interactable);
            m_CurrencyField.SetInteractable(m_Interactable);
        }

        /// <summary>
        /// Show the Type in the name
        /// </summary>
        /// <param name="showType">Should the type be shown?</param>
        public override void SetShowType(bool showType)
        {
            m_ShowType = showType;
            m_CurrencyField.ViewName.SetShowType(showType);
        }
    }
}