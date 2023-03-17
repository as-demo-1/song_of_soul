/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Exchange
{
    using Opsive.Shared.Events;
    using Opsive.UltimateInventorySystem.Core;
    using UnityEngine;

    /// <summary>
    /// The interface for a currency owner.
    /// </summary>
    public interface ICurrencyOwner
    {
        /// <summary>
        /// Get the game object attached to the currency owner.
        /// </summary>
        GameObject gameObject { get; }
    }

    /// <summary>
    /// Generic interface for a currency owner.
    /// </summary>
    /// <typeparam name="T">The currency type.</typeparam>
    public interface ICurrencyOwner<T> : ICurrencyOwner
    {
        /// <summary>
        /// Get the currency amount.
        /// </summary>
        T CurrencyAmount { get; }

        /// <summary>
        /// Add currency.
        /// </summary>
        /// <param name="amount">The amount to add.</param>
        /// <returns>True if added correctly.</returns>
        bool AddCurrency(T amount);

        /// <summary>
        /// Remove currency.
        /// </summary>
        /// <param name="amount">The amount to remove.</param>
        /// <returns>True if removed correctly.</returns>
        bool RemoveCurrency(T amount);

        /// <summary>
        /// Set the currency amount.
        /// </summary>
        /// <param name="amount">The amount to set.</param>
        /// <returns>True if set correctly.</returns>
        bool SetCurrency(T amount);

        /// <summary>
        /// Check if the holder has that amount of currency.
        /// </summary>
        /// <param name="amount">amount of currency.</param>
        /// <returns>True if the holder has at least that amount.</returns>
        bool HasCurrency(T amount);

        /// <summary>
        /// Computes the number of times the currency amount provided could fit in the CurrencyOwner amount.
        /// Think of it as a division that is not processed but the result is returned as a quotient instead.
        /// This function is useful to display how many times an item could be bought for example.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <returns>The quotient.</returns>
        int PotentialQuotientFor(T amount);

        /// <summary>
        /// Set the currency without notification.
        /// </summary>
        /// <param name="newAmount">The amount to set.</param>
        void SetCurrencyWithoutNotify(T newAmount);
    }

    /// <summary>
    /// Base class for currency owner. Used as a container of currency.
    /// </summary>
    public abstract class CurrencyOwnerBase : MonoBehaviour, ICurrencyOwner
    { }

    /// <summary>
    /// Generic class for currency owner. Used as a container of any type of currency.
    /// </summary>
    /// <typeparam name="CurrencyT">The currency type.</typeparam>
    public abstract class CurrencyOwnerGeneric<CurrencyT> : CurrencyOwnerBase, ICurrencyOwner<CurrencyT>
    {
        [Tooltip("The amount of currency.")]
        [SerializeField] protected CurrencyT m_CurrencyAmount;

        public virtual CurrencyT CurrencyAmount => m_CurrencyAmount;

        /// <summary>
        /// Add currency.
        /// </summary>
        /// <param name="amount">The amount to add.</param>
        /// <returns>True if added correctly.</returns>
        public virtual bool AddCurrency(CurrencyT amount)
        {
            var result = AddCurrencyInternal(amount);
            NotifyChange();
            return result;
        }

        /// <summary>
        /// Remove currency.
        /// </summary>
        /// <param name="amount">The amount to remove.</param>
        /// <returns>True if removed correctly.</returns>
        public virtual bool RemoveCurrency(CurrencyT amount)
        {
            var result = RemoveCurrencyInternal(amount);
            NotifyChange();
            return result;
        }

        /// <summary>
        /// Set the currency amount.
        /// </summary>
        /// <param name="amount">The amount to set.</param>
        /// <returns>True if set correctly.</returns>
        public virtual bool SetCurrency(CurrencyT amount)
        {
            var result = SetCurrencyInternal(amount);
            NotifyChange();
            return result;
        }

        /// <summary>
        /// Notify that a change occured.
        /// </summary>
        public void NotifyChange()
        {
            EventHandler.ExecuteEvent(this, EventNames.c_CurrencyOwner_OnUpdate);
        }

        /// <summary>
        /// Add currency.
        /// </summary>
        /// <param name="amount">The amount to add.</param>
        /// <returns>True if added correctly.</returns>
        protected abstract bool AddCurrencyInternal(CurrencyT amount);

        /// <summary>
        /// Remove currency.
        /// </summary>
        /// <param name="amount">The amount to remove.</param>
        /// <returns>True if removed correctly.</returns>
        protected abstract bool RemoveCurrencyInternal(CurrencyT amount);

        /// <summary>
        /// Set the currency amount.
        /// </summary>
        /// <param name="amount">The amount to set.</param>
        /// <returns>True if set correctly.</returns>
        protected abstract bool SetCurrencyInternal(CurrencyT amount);

        /// <summary>
        /// Computes the number of times the currency amount provided could fit in the Currency Owner amount.
        /// Think of it as a division that is not processed but the result is returned as a quotient instead.
        /// This function is useful to display how many times an item could be bought for example.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <returns>The quotient.</returns>
        public abstract int PotentialQuotientFor(CurrencyT amount);

        /// <summary>
        /// Check if the holder has that amount of currency.
        /// </summary>
        /// <param name="amount">amount of currency.</param>
        /// <returns>True if the holder has at least that amount.</returns>
        public abstract bool HasCurrency(CurrencyT amount);

        /// <summary>
        /// Set the currency without notification.
        /// </summary>
        /// <param name="newAmount">The amount to set.</param>
        public virtual void SetCurrencyWithoutNotify(CurrencyT newAmount)
        {
            SetCurrencyInternal(newAmount);
        }

    }
}
