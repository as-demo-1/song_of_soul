/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Currency
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Exchange;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using UnityEngine;

    /// <summary>
    /// A struct that maps a currency to a currency view.
    /// </summary>
    [Serializable]
    public struct CurrencyWithView
    {
        [Tooltip("The currency.")]
        [SerializeField] private DynamicCurrency m_Currency;
        [Tooltip("The currency view.")]
        [SerializeField] private CurrencyView m_View;

        public Currency Currency => m_Currency;
        public CurrencyView View => m_View;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="currency">The currency.</param>
        /// <param name="view">The view.</param>
        public CurrencyWithView(Currency currency, CurrencyView view)
        {
            m_Currency = currency;
            m_View = view;
        }
    }

    /// <summary>
    /// Currency UI used to display currency amounts.
    /// </summary>
    public class MultiCurrencyView : MonoBehaviour, IDatabaseSwitcher
    {
        [Tooltip("The currency amount UIs.")]
        [SerializeField] internal CurrencyWithView[] m_CurrenciesWithViews;

        /// <summary>
        /// Draw currency amounts.
        /// </summary>
        /// <param name="currencyCollection">A currency Collection.</param>
        public void DrawCurrency(CurrencyCollection currencyCollection)
        {
            if (currencyCollection == null) {
                Clear();
                return;
            }
            for (int i = 0; i < m_CurrenciesWithViews.Length; i++) {
                var amount = currencyCollection.GetAmountOf(m_CurrenciesWithViews[i].Currency);

                m_CurrenciesWithViews[i].View.SetValue((m_CurrenciesWithViews[i].Currency, amount));
            }
        }

        /// <summary>
        /// Set the amount of currency to display.
        /// </summary>
        /// <param name="currencyAmounts">A currency amounts to display.</param>
        public void DrawCurrency(Exchange.CurrencyAmount[] currencyAmounts)
        {
            if (currencyAmounts == null) {
                Clear();
                return;
            }

            for (int i = 0; i < m_CurrenciesWithViews.Length; i++) {
                var currencyWithView = m_CurrenciesWithViews[i];

                var foundMatch = false;
                for (int j = 0; j < currencyAmounts.Length; j++) {
                    var currencyAmount = currencyAmounts[j];
                    if (currencyAmount.Currency != currencyWithView.Currency) { continue; }

                    currencyWithView.View.SetValue(currencyAmount);
                    foundMatch = true;
                    break;
                }

                if (foundMatch == false) {
                    currencyWithView.View.SetValue(new CurrencyAmount(currencyWithView.Currency, 0));
                }
            }
        }

        /// <summary>
        /// Clear the currency views.
        /// </summary>
        private void Clear()
        {
            for (int i = 0; i < m_CurrenciesWithViews.Length; i++) {
                m_CurrenciesWithViews[i].View.SetValue(CurrencyAmount.None);
            }
        }

        /// <summary>
        /// Draw Empty.
        /// </summary>
        public void DrawEmptyCurrency()
        {
            for (int i = 0; i < m_CurrenciesWithViews.Length; i++) {
                m_CurrenciesWithViews[i].View.Clear();
            }
        }

        /// <summary>
        /// Set the text color.
        /// </summary>
        /// <param name="color">The color to set.</param>
        public void SetTextColor(Color color)
        {
            for (int i = 0; i < m_CurrenciesWithViews.Length; i++) {
                m_CurrenciesWithViews[i].View.Text.SetColor(color);
            }
        }

        /// <summary>
        /// Get the text color of the first currency text. 
        /// </summary>
        /// <returns>The text color.</returns>
        public Color GetTextColor()
        {
            return m_CurrenciesWithViews[0].View.Text.color;
        }



        /// <summary>
        /// Check if the object contained by this component are part of the database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>True if all the objects in the component are part of that database.</returns>
        bool IDatabaseSwitcher.IsComponentValidForDatabase(InventorySystemDatabase database)
        {
            if (database == null) { return false; }

            for (int i = 0; i < m_CurrenciesWithViews.Length; i++) {
                if (database.Contains(m_CurrenciesWithViews[i].Currency)) { continue; }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Replace any object that is not in the database by an equivalent object in the specified database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>The objects that have been changed.</returns>
        ModifiedObjectWithDatabaseObjects IDatabaseSwitcher.ReplaceInventoryObjectsBySelectedDatabaseEquivalents(InventorySystemDatabase database)
        {
            if (database == null) { return null; }

            for (int i = 0; i < m_CurrenciesWithViews.Length; i++) {
                if (database.Contains(m_CurrenciesWithViews[i].Currency)) { continue; }

                m_CurrenciesWithViews[i] = new CurrencyWithView(
                    database.FindSimilar(m_CurrenciesWithViews[i].Currency),
                    m_CurrenciesWithViews[i].View);
            }

            return null;
        }
    }
}