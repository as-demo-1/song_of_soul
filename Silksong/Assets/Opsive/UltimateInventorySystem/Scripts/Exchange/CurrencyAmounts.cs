/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Exchange
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;

    /// <summary>
    /// An array of currency amounts.
    /// </summary>
    [System.Serializable]
    public class CurrencyAmounts : ObjectAmounts<Currency, CurrencyAmount>
    {
        public CurrencyAmounts() : base()
        { }

        public CurrencyAmounts(CurrencyAmount[] array) : base(array)
        { }

        public static implicit operator CurrencyAmount[](CurrencyAmounts x) => x?.m_Array;
        public static implicit operator CurrencyAmounts(CurrencyAmount[] x) => new CurrencyAmounts(x);
    }
}