/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Exchange
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Storage;
    using EventHandler = Shared.Events.EventHandler;

    /// <summary>
    /// Currency owner of a currency collection.
    /// </summary>
    public class CurrencyOwner : CurrencyOwnerGeneric<CurrencyCollection>, IDatabaseSwitcher
    {
        /// <summary>
        /// Initialize on awake.
        /// </summary>
        private void Awake()
        {
            if (m_CurrencyAmount == null) {
                m_CurrencyAmount = new CurrencyCollection();
            }
            m_CurrencyAmount.Initialize(null, true);

            m_CurrencyAmount.OnAdd += (amountSlice) =>
            {
                EventHandler.ExecuteEvent<ListSlice<CurrencyAmount>>(gameObject, EventNames.c_CurrencyOwnerGameObject_OnAdd_CurrencyAmountListSlice, amountSlice);
            };
            m_CurrencyAmount.OnRemove += (amountSlice) =>
            {
                EventHandler.ExecuteEvent<ListSlice<CurrencyAmount>>(gameObject, EventNames.c_CurrencyOwnerGameObject_OnRemove_CurrencyAmountListSlice, amountSlice);
            };

            EventHandler.RegisterEvent(m_CurrencyAmount, EventNames.c_CurrencyCollection_OnUpdate, NotifyChange);

            NotifyChange();
        }

        /// <summary>
        /// Add currency.
        /// </summary>
        /// <param name="amount">The amount to add.</param>
        /// <returns>True if added correctly.</returns>
        protected override bool AddCurrencyInternal(CurrencyCollection amount)
        {
            return m_CurrencyAmount.AddCurrency(amount);
        }

        /// <summary>
        /// Add currency to the currency collection.
        /// </summary>
        /// <param name="currency">The currency to add.</param>
        /// <param name="amount">The amount to add.</param>
        /// <returns>True if added correctly.</returns>
        public virtual bool AddCurrency(Currency currency, double amount)
        {
            var result = m_CurrencyAmount.AddCurrency(currency, amount);
            if (result) { NotifyChange(); }
            return result;
        }
        
        /// <summary>
        /// Add currency to the currency collection.
        /// </summary>
        /// <param name="currencyName">The currency to add.</param>
        /// <param name="amount">The amount to add.</param>
        /// <returns>True if added correctly.</returns>
        public virtual bool AddCurrency(string currencyName, double amount)
        {
            var result = m_CurrencyAmount.AddCurrency(InventorySystemManager.GetCurrency(currencyName), amount);
            if (result) { NotifyChange(); }
            return result;
        }

        /// <summary>
        /// Remove currency.
        /// </summary>
        /// <param name="amount">The amount to remove.</param>
        /// <returns>True if removed correctly.</returns>
        protected override bool RemoveCurrencyInternal(CurrencyCollection amount)
        {
            return m_CurrencyAmount.RemoveCurrency(amount);
        }

        /// <summary>
        /// Remove currency from collection.
        /// </summary>
        /// <param name="currency">Currency to remove.</param>
        /// <param name="amount">Amount to remove.</param>
        /// <returns>True if removed correctly.</returns>
        public virtual bool RemoveCurrency(Currency currency, double amount)
        {
            var result = m_CurrencyAmount.RemoveCurrency(currency, amount);
            if (result) { NotifyChange(); }
            return result;
        }
        
        /// <summary>
        /// Remove currency from collection.
        /// </summary>
        /// <param name="currencyName">Currency to remove.</param>
        /// <param name="amount">Amount to remove.</param>
        /// <returns>True if removed correctly.</returns>
        public virtual bool RemoveCurrency(string currencyName, double amount)
        {
            var result = m_CurrencyAmount.RemoveCurrency(InventorySystemManager.GetCurrency(currencyName), amount);
            if (result) { NotifyChange(); }
            return result;
        }

        /// <summary>
        /// Set the currency amount.
        /// </summary>
        /// <param name="amount">The amount to set.</param>
        /// <returns>True if set correctly.</returns>
        protected override bool SetCurrencyInternal(CurrencyCollection amount)
        {
            return m_CurrencyAmount.SetCurrency(amount);
        }

        /// <summary>
        /// Computes the number of times the currency amount provided could fit in the CurrencyOwner amount.
        /// Think of it as a division that is not processed but the result is returned as a quotient instead.
        /// This function is useful to display how many times an item could be bought for example.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <returns>The quotient.</returns>
        public override int PotentialQuotientFor(CurrencyCollection amount)
        {
            return m_CurrencyAmount.PotentialQuotientFor(amount);
        }

        /// <summary>
        /// Check if the holder has that amount of currency.
        /// </summary>
        /// <param name="amount">amount of currency.</param>
        /// <returns>True if the holder has at least that amount.</returns>
        public override bool HasCurrency(CurrencyCollection amount)
        {
            return m_CurrencyAmount.HasCurrency(amount);
        }

        /// <summary>
        /// Unregister listener to events.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent(
                m_CurrencyAmount, EventNames.c_CurrencyCollection_OnUpdate, NotifyChange);
        }

        /// <summary>
        /// Check if the object contained by this component are part of the database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>True if all the objects in the component are part of that database.</returns>
        bool IDatabaseSwitcher.IsComponentValidForDatabase(InventorySystemDatabase database)
        {
            if (database == null) { return false; }

            CurrencyAmount.Initialize(null, false);

            var currencyAmounts = CurrencyAmount.GetCurrencyAmounts();

            for (int i = 0; i < currencyAmounts.Count; i++) {
                if (database.Contains(currencyAmounts[i].Currency)) { continue; }

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

            m_CurrencyAmount.Initialize(null, false);

            bool dirty = false;
            var currencyAmounts = m_CurrencyAmount.GetCurrencyAmounts().ToArray();

            for (int i = 0; i < currencyAmounts.Length; i++) {
                if (database.Contains(currencyAmounts[i].Currency)) {
                    continue;
                }

                dirty = true;
                var currency = database.FindSimilar(currencyAmounts[i].Currency);
                currencyAmounts[i] = (currencyAmounts[i].Amount, currency);
            }

            if (!dirty) { return null; }

            m_CurrencyAmount.SetCurrency(currencyAmounts);
            m_CurrencyAmount.Serialize();

            return null;
        }
    }
}
