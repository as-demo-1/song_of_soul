/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Exchange
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Currency exchange rate.
    /// </summary>
    [Serializable]
    public struct CurrencyExchangeRate
    {
        [Tooltip("The currency.")]
        [SerializeField] private Currency m_Currency;
        [Tooltip("The exchange rate.")]
        [SerializeField] private double m_ExchangeRate;

        public Currency Currency => m_Currency;
        public double ExchangeRate => m_ExchangeRate;

        public CurrencyExchangeRate(Currency currency, double exchangeRate)
        {
            m_Currency = currency;
            m_ExchangeRate = exchangeRate;
        }

        public static implicit operator CurrencyExchangeRate((double, Currency) x)
            => new CurrencyExchangeRate(x.Item2, x.Item1);
        public static implicit operator CurrencyExchangeRate((Currency, double) x)
            => new CurrencyExchangeRate(x.Item1, x.Item2);
    }
}