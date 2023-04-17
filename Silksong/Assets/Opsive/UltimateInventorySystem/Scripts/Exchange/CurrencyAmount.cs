/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Exchange
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using System;
    using UnityEngine;

    /// <summary>
    /// Amount of Currency.
    /// </summary>
    [Serializable]
    public struct CurrencyAmount : IEquatable<CurrencyAmount>, IObjectAmount<Currency>
    {
        [Tooltip("The amount.")]
        [SerializeField] private int m_Amount;
        [Tooltip("The currency.")]
        [SerializeField] private Currency m_Currency;

        public int Amount => m_Amount;
        public Currency Currency => m_Currency;
        public Currency Object => m_Currency;

        public static CurrencyAmount None => new CurrencyAmount(null, 0);

        /// <summary>
        /// The currency amount constructor.
        /// </summary>
        /// <param name="currency">The currency.</param>
        /// <param name="amount">The amount.</param>
        public CurrencyAmount(Currency currency, int amount)
        {
            m_Amount = amount;
            m_Currency = currency;
        }
        
        /// <summary>
        /// The currency amount constructor.
        /// </summary>
        /// <param name="currency">The currency.</param>
        /// <param name="amount">The amount.</param>
        public CurrencyAmount(int amount, Currency currency)
        {
            m_Amount = amount;
            m_Currency = currency;
        }

        /// <summary>
        /// The currency constructor.
        /// </summary>
        /// <param name="other">The currency amount to copy.</param>
        public CurrencyAmount(CurrencyAmount other)
        {
            m_Amount = other.Amount;
            m_Currency = other.Currency;
        }

        public static implicit operator CurrencyAmount((int, Currency) x)
            => new CurrencyAmount(x.Item2, x.Item1);
        public static implicit operator CurrencyAmount((Currency, int) x)
            => new CurrencyAmount(x.Item1, x.Item2);

        /// <summary>
        /// Returns a readable format for the object.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return string.Format("{0} {1}", m_Amount, m_Currency);
        }

        /// <summary>
        /// Do an Equivalence check.
        /// </summary>
        /// <param name="lhs">Left hand side object.</param>
        /// <param name="rhs">Right hand side object.</param>
        /// <returns>True if equivalent.</returns>
        public static bool operator ==(CurrencyAmount i1, CurrencyAmount i2)
        {
            return i1.Equals(i2);
        }

        /// <summary>
        /// Do an Equivalence check.
        /// </summary>
        /// <param name="lhs">Left hand side object.</param>
        /// <param name="rhs">Right hand side object.</param>
        /// <returns>True if not equivalent.</returns>
        public static bool operator !=(CurrencyAmount i1, CurrencyAmount i2)
        {
            return !i1.Equals(i2);
        }

        /// <summary>
        /// Checks if the objects are equivalent, by comparing values and parameters.
        /// </summary>
        /// <param name="other">The other object.</param>
        /// <returns>Returns true if equivalent.</returns>
        public bool Equals(CurrencyAmount other)
        {
            return m_Amount == other.m_Amount && Equals(m_Currency, other.m_Currency);
        }

        /// <summary>
        /// Checks if the object are equivalent.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>Returns true if equivalent.</returns>
        public override bool Equals(object obj)
        {
            return obj is CurrencyAmount other && Equals(other);
        }

        /// <summary>
        /// Returns the hashcode for the object, computed from the value and parameters.
        /// </summary>
        /// <returns>The computed hashcode.</returns>
        public override int GetHashCode()
        {
            unchecked { return (m_Amount * 397) ^ (m_Currency != null ? m_Currency.GetHashCode() : 0); }
        }
    }
}