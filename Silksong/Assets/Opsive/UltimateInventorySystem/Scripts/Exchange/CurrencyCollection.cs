/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Exchange
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    using EventHandler = Shared.Events.EventHandler;

    /// <summary>
    /// Comparer used to sort currencyAmounts by the root the currencies.
    /// </summary>
    public class CurrencyAmountRootComparer : IComparer<CurrencyAmount>
    {

        /// <summary>
        /// Compare the root of the currencies.
        /// </summary>
        /// <param name="x">Currency amount lhs.</param>
        /// <param name="y">Currency amount rhs.</param>
        /// <returns>The comparison value.</returns>
        public int Compare(CurrencyAmount x, CurrencyAmount y)
        {
            if (x.Currency == null && y.Currency == null) { return 0; }
            if (x.Currency == null) { return 1; }
            if (y.Currency == null) { return 1; }
            return x.Currency.GetRoot().name.CompareTo(y.Currency.GetRoot().name);
        }
    }

    /// <summary>
    /// Collection of currency amounts, used to organize currencies amounts using their defined constraints.
    /// This class also contains logic to add, remove, and divide currency amounts.
    /// </summary>
    [System.Serializable]
    public class CurrencyCollection
    {
        public event Action<ListSlice<CurrencyAmount>> OnAdd;
        public event Action<ListSlice<CurrencyAmount>> OnRemove;

        [Tooltip("The serialization data for the currency amounts loaded at the start of the game.")]
        [SerializeField] protected Serialization m_CurrencyAmountData;

        [System.NonSerialized] protected bool m_Initialized = false;
        [System.NonSerialized] protected ResizableArray<CurrencyAmount> m_CurrencyAmounts;

        protected static readonly CurrencyAmountRootComparer s_CurrencyAmountRootComparer = new CurrencyAmountRootComparer();

        /// <summary>
        /// Default constructor used to initialize the object.
        /// </summary>
        public CurrencyCollection()
        {
            var pooledArray = GenericObjectPool.Get<CurrencyAmount[]>();
            m_CurrencyAmounts = new ResizableArray<CurrencyAmount>(pooledArray, false, 0);
        }

        /// <summary>
        /// Initialize the collection using a new start currency amount array.
        /// </summary>
        /// <param name="currencyAmountAtStart">The currency amount to start with.</param>
        /// <param name="force">Force the initialization.</param>
        public virtual void Initialize(CurrencyAmount[] currencyAmountAtStart, bool force)
        {
            if (m_Initialized == true && force == false) {
                return;
            }

            Deserialize();
            m_Initialized = true;

            if (currencyAmountAtStart == null) { return; }
            AddCurrency(currencyAmountAtStart);
        }

        /// <summary>
        /// Initialize the collection array struct and add the start currency amounts.
        /// </summary>
        protected virtual void Deserialize()
        {
            if (m_CurrencyAmountData != null && m_CurrencyAmountData.Values?.Length > 0) {
                m_CurrencyAmounts = (ResizableArray<CurrencyAmount>)m_CurrencyAmountData.DeserializeFields(MemberVisibility.Public);
            }

            if (m_CurrencyAmounts == null || m_CurrencyAmounts.Array == null) {
                var pooledArray = GenericObjectPool.Get<CurrencyAmount[]>();
                m_CurrencyAmounts = new ResizableArray<CurrencyAmount>(pooledArray, false, 0);
            }
        }

        /// <summary>
        /// Serializes the currency amount ArrayStruct to Serialization data.
        /// </summary>
        public void Serialize()
        {
            m_CurrencyAmounts.Truncate();
            m_CurrencyAmountData = Serialization.Serialize(m_CurrencyAmounts);
        }

        #region Static Functions

        /// <summary>
        /// Find the index of the currency amount which contains the currency specified.
        /// </summary>
        /// <param name="currency">The currency that needs to match.</param>
        /// <param name="list">The array struct to search.</param>
        /// <returns>The index of the currency amount, -1 if not found.</returns>
        public static int FindIndexWithCurrency(Currency currency, ListSlice<CurrencyAmount> list)
        {
            for (int i = 0; i < list.Count; i++) {
                if (list[i].Currency == currency) { return i; }
            }

            return -1;
        }

        /// <summary>
        /// Finds the index of the currency amount with the specified currency, if not found a new element is added to the currency amounts.
        /// </summary>
        /// <param name="currency">The currency that needs to match.</param>
        /// <param name="result">The array to search.</param>
        /// <param name="count">The array element count.</param>
        /// <returns>The index of the currency amount.</returns>
        private static int FindOrCreateCurrencyIndex(Currency currency, ref CurrencyAmount[] result, ref int count)
        {
            var index = FindIndexWithCurrency(currency, (result, 0, count));
            if (index != -1) { return index; }

            TypeUtility.ResizeIfNecessary(ref result, count + 1);
            result[count] = new CurrencyAmount(currency, 0);
            count++;

            index = count - 1;

            return index;
        }

        /// <summary>
        /// Safely convert to Decimal without an overflow exception.
        /// </summary>
        /// <param name="x">the value to convert.</param>
        /// <returns>The decimal equivalent.</returns>
        public static decimal ToDecimalSafe(double x)
        {
            if (x < (double)decimal.MinValue)
                return decimal.MinValue;
            else if (x > (double)decimal.MaxValue)
                return decimal.MaxValue;
            else
                return (decimal)x;
        }

        /// <summary>
        /// Safely convert to Int without an overflow exception.
        /// </summary>
        /// <param name="x">the value to convert.</param>
        /// <returns>The int equivalent.</returns>
        public static int ToIntSafe(double x)
        {
            if (x < (double)int.MinValue)
                return int.MinValue;
            else if (x > (double)int.MaxValue)
                return int.MaxValue;
            else
                return (int)x;
        }

        /// <summary>
        /// Safely convert to Int without an overflow exception.
        /// </summary>
        /// <param name="x">the value to convert.</param>
        /// <returns>The int equivalent.</returns>
        public static int ToIntSafe(ulong x)
        {
            if (x <= 0)
                return 0;
            else if (x > (ulong)int.MaxValue)
                return int.MaxValue;
            else
                return (int)x;
        }

        /// <summary>
        /// Convert the currency amounts specified to a discrete format, meaning a format that follows the constraints of the currencies.
        /// </summary>
        /// <param name="currencyAmountsInput">The currency amounts.</param>
        /// <param name="multiplier">A multiplier for the currency amounts.</param>
        /// <param name="result">The array that will store the result.</param>
        /// <returns>The array struct containing the resulting array.</returns>
        public static ListSlice<CurrencyAmount> ConvertToDiscrete_S(ListSlice<CurrencyAmount> currencyAmountsInput, double multiplier, ref CurrencyAmount[] result)
        {
            var pooledArray = GenericObjectPool.Get<CurrencyAmount[]>();
            var pooledArray2 = GenericObjectPool.Get<CurrencyAmount[]>();

            var count = 0;

            for (int i = 0; i < currencyAmountsInput.Count; i++) {
                var chunk = ConvertToDiscrete_S(currencyAmountsInput[i].Currency, currencyAmountsInput[i].Amount * multiplier,
                    ref pooledArray);

                for (int j = 0; j < count; j++) {
                    TypeUtility.ResizeIfNecessary(ref pooledArray2, j);
                    pooledArray2[j] = result[j];
                }

                count = DiscreteAddition_S((pooledArray2, 0, count), chunk, ref result).Count;
            }

            GenericObjectPool.Return(pooledArray);
            GenericObjectPool.Return(pooledArray2);

            return (result, 0, count);
        }

        /// <summary>
        /// Convert a single currency amount to a set of currencyAmounts which are in a discrete format, meaning a format that follows the constraints of the currencies.
        /// </summary>
        /// <param name="currency">The currency.</param>
        /// <param name="amount">The amounts.</param>
        /// <param name="result">The array that will store the result.</param>
        /// <returns>The count of currency amounts.</returns>
        public static ListSlice<CurrencyAmount> ConvertToDiscrete_S(Currency currency, double amount, ref CurrencyAmount[] result)
        {
            var index = 0;

            var currencies = GenericObjectPool.Get<CurrencyAmount[]>();
            var overflowCurrencies = ConvertOverflow_S(currency, amount, ref currencies);

            var maxed = false;
            for (int i = overflowCurrencies.Count - 1; i >= 0; i--) {
                TypeUtility.ResizeIfNecessary(ref result, index + 1);

                if (currency.FractionCurrency == overflowCurrencies[i].Currency) { maxed = true; }

                result[index] = overflowCurrencies[i];
                index++;
            }

            if (!maxed) {
                var fractionCurrencies = ConvertFraction_S(currency, amount, ref currencies);

                for (int i = 0; i < fractionCurrencies.Count; i++) {
                    TypeUtility.ResizeIfNecessary(ref result, index + 1);

                    result[index] = fractionCurrencies[i];
                    index++;
                }
            }

            GenericObjectPool.Return(currencies);

            return (result, 0, index);
        }

        /// <summary>
        /// Converts only the amounts that are full numbers, does not care about the fractional amounts.
        /// </summary>
        /// <param name="currency">The currency.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="result">The array that will store the result.</param>
        /// <returns>The count of currency amounts.</returns>
        public static ListSlice<CurrencyAmount> ConvertOverflow_S(Currency currency, double amount, ref CurrencyAmount[] result)
        {
            var index = 0;

            var nextCurrency = currency;

            amount = Math.Truncate(amount);// ToIntSafe(amount);

            while (nextCurrency != null && amount > 0) {

                // If MaxAmount is int.Max, it still works because % int.min is still the biggest number.
                var mod = amount % (nextCurrency.MaxAmount + 1);

                var intMod = (int)mod;
                if (intMod > 0) {
                    TypeUtility.ResizeIfNecessary(ref result, index + 1);
                    result[index] = (intMod, nextCurrency);
                    index++;
                }

                if (amount - intMod <= 0d) { break; }

                if (nextCurrency.OverflowCurrency == null ||
                    !nextCurrency.TryGetExchangeRateTo(nextCurrency.OverflowCurrency, out var rate)) {
                    return MaxedOutAmount_S(nextCurrency, ref result);
                }

                amount -= mod;
                amount *= rate;
                nextCurrency = nextCurrency.OverflowCurrency;
            }

            return (result, 0, index);
        }

        /// <summary>
        /// Converts only the fractional amounts, does not take into account full number amounts.
        /// </summary>
        /// <param name="currency">The currency.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="result">The array that will store the result.</param>
        /// <returns>The array struct containing the resulting array.</returns>
        public static ListSlice<CurrencyAmount> ConvertFraction_S(Currency currency, double amount, ref CurrencyAmount[] result)
        {
            if (amount % 1d == 0d) { return (result, 0, 0); }

            var index = 0;

            var nextCurrency = currency;
            var decimalAmount = ToDecimalSafe(amount);// (decimal) amount;
            decimalAmount = decimalAmount % 1;

            while (nextCurrency != null && decimalAmount > 0) {

                if (nextCurrency.FractionCurrency == null) { break; }
                if (nextCurrency.TryGetExchangeRateTo(nextCurrency.FractionCurrency, out var rate) == false) { break; }
                nextCurrency = nextCurrency.FractionCurrency;

                decimalAmount *= (decimal)rate;
                var floor = (int)decimalAmount;
                if (floor > 0) {
                    TypeUtility.ResizeIfNecessary(ref result, index + 1);
                    result[index] = (floor, nextCurrency);
                    index++;
                }

                decimalAmount = decimalAmount - floor;

            }

            return (result, 0, index);
        }

        /// <summary>
        /// Discrete addition, assumes that both operands are in a discrete format.
        /// </summary>
        /// <param name="lhs">Left hand side discrete currency amounts.</param>
        /// <param name="rhs">Right hand side discrete currency amounts.</param>
        /// <param name="result">The array that will store the result.</param>
        /// <returns>The array struct containing the resulting array.</returns>
        private static ListSlice<CurrencyAmount> DiscreteAddition_S(
            ListSlice<CurrencyAmount> lhs,
            ListSlice<CurrencyAmount> rhs, ref CurrencyAmount[] result)
        {
            var pooledArray = GenericObjectPool.Get<CurrencyAmount[]>();

            var resultSlice = lhs.CopyTo(ref pooledArray);

            for (int i = 0; i < rhs.Count; i++) {
                if (i % 2 == 0) {
                    resultSlice = DiscreteAddition_S(resultSlice, rhs[i].Currency, rhs[i].Amount, ref result);
                } else {
                    resultSlice = DiscreteAddition_S(resultSlice, rhs[i].Currency, rhs[i].Amount, ref pooledArray);
                }
            }

            resultSlice = resultSlice.CopyTo(ref result);
            GenericObjectPool.Return(pooledArray);

            return resultSlice;
        }

        /// <summary>
        /// Discrete addition, assumes the currency amounts is in a discrete format.
        /// </summary>
        /// <param name="currencyAmounts">The currency amounts.</param>
        /// <param name="currency">The currency to add.</param>
        /// <param name="amount">The amount to add.</param>
        /// <param name="result">The array that will store the result.</param>
        /// <returns>The count of currency amounts.</returns>
        private static ListSlice<CurrencyAmount> DiscreteAddition_S(ListSlice<CurrencyAmount> currencyAmounts,
            Currency currency, int amount, ref CurrencyAmount[] result)
        {
            var count = currencyAmounts.CopyTo(ref result).Count;

            while (true) {
                var index = FindOrCreateCurrencyIndex(currency, ref result, ref count);

                var sum = amount + result[index].Amount;

                var modInt = (int)(sum % (currency.MaxAmount + 1));
                result[index] = (modInt, currency);

                var overflow = sum - modInt;

                if (overflow <= 0) { break; }

                var overflowCurrency = currency.OverflowCurrency;
                if (overflowCurrency == null || !currency.TryGetExchangeRateTo(overflowCurrency, out var rate)) {
                    return SetAllFractionToMax((result, 0, count), currency, ref result);
                }

                var overflowDouble = overflow * rate;
                overflow = (int)Math.Truncate(overflowDouble);

                var diff = overflowDouble - overflow;
                if (diff > 0) {
                    var pooledArray1 = GenericObjectPool.Get<CurrencyAmount[]>();
                    var pooledArray2 = GenericObjectPool.Get<CurrencyAmount[]>();
                    ListSlice.CopyTo(result, ref pooledArray2);

                    var newResult = DiscreteAddition_S(
                        (pooledArray2, 0, count),
                        ConvertToDiscrete_S(overflowCurrency, diff, ref pooledArray1), ref result);
                    GenericObjectPool.Return(pooledArray1);
                    GenericObjectPool.Return(pooledArray2);
                    count = newResult.Count;
                }

                currency = overflowCurrency;
                amount = overflow;
            }

            return (result, 0, count);
        }

        /// <summary>
        /// Discrete subtraction, assumes that both operands are in a discrete format.
        /// </summary>
        /// <param name="lhs">Left hand side discrete currency amounts.</param>
        /// <param name="rhs">Right hand side discrete currency amounts.</param>
        /// <param name="result">The array that will store the result.</param>
        /// <returns>The array struct containing the resulting array.</returns>
        private static ListSlice<CurrencyAmount> DiscreteSubtraction_S(
            ListSlice<CurrencyAmount> lhs,
            ListSlice<CurrencyAmount> rhs, ref CurrencyAmount[] result)
        {
            var pooledArray = GenericObjectPool.Get<CurrencyAmount[]>();

            var resultSlice = lhs.CopyTo(ref pooledArray);

            for (int i = 0; i < rhs.Count; i++) {
                if (i % 2 == 0) {
                    resultSlice = DiscreteSubtraction_S(resultSlice, rhs[i].Currency, rhs[i].Amount, ref result);
                } else {
                    resultSlice = DiscreteSubtraction_S(resultSlice, rhs[i].Currency, rhs[i].Amount, ref pooledArray);
                }
            }

            resultSlice = resultSlice.CopyTo(ref result);
            GenericObjectPool.Return(pooledArray);

            return resultSlice;
        }

        /// <summary>
        /// Discrete subtraction, assumes that the currencyAmount is in discrete format.
        /// </summary>
        /// <param name="currencyAmounts">The currency amounts.</param>
        /// <param name="currency">The currency to subtract.</param>
        /// <param name="amount">The amount to subtract.</param>
        /// <param name="result">The array that will store the result.</param>
        /// <returns>The count of currency amounts.</returns>
        private static ListSlice<CurrencyAmount> DiscreteSubtraction_S(ListSlice<CurrencyAmount> currencyAmounts,
            Currency currency, int amount, ref CurrencyAmount[] result)
        {
            var count = currencyAmounts.CopyTo(ref result).Count;

            while (true) {

                var index = FindOrCreateCurrencyIndex(currency, ref result, ref count);

                var diff = result[index].Amount - amount;

                if (diff >= 0) {
                    result[index] = (diff, currency);
                    break;
                }

                var foundCurrencyToFraction = FindCurrencyThatFractionTo((result, 0, count), currency, out var overflowCurrency);

                if (!foundCurrencyToFraction || !overflowCurrency.TryGetExchangeRateTo(currency, out var rate)) {
                    //Subtracting more currency than had before, only counting inherently auto conversion currencies.
                    var currentAmounts = (result, 0, count);

                    var extractedFractionsPooledArray = GenericObjectPool.Get<CurrencyAmount[]>();
                    var extractedFractions = ExtractFraction(currentAmounts, currency.FractionCurrency, ref extractedFractionsPooledArray);

                    var possibleResult = SetAllFractionToZero(currencyAmounts, currency, ref result);

                    //Check if some currency share the same root.
                    for (int i = 0; i < possibleResult.Count; i++) {
                        if (possibleResult[i].Amount == 0) { continue; }
                        if (!currency.TryGetExchangeRateTo(possibleResult[i].Currency, out var exchangeRate)) { continue; }

                        var fractionsRootAmount = GetFullAmountAsRootCurrency(extractedFractions, currency);
                        var bigAmount = (Mathf.Abs(diff) * exchangeRate);
                        var smallAmount = fractionsRootAmount / (possibleResult[i].Currency.GetRootExchangeRate().ExchangeRate);
                        var amountToRemove = bigAmount - smallAmount;

                        if (amountToRemove <= 0) { break; }

                        GenericObjectPool.Return(extractedFractionsPooledArray);

                        var pooledArray1 = GenericObjectPool.Get<CurrencyAmount[]>();
                        var pooledArray2 = GenericObjectPool.Get<CurrencyAmount[]>();

                        var copyPossibleResult = possibleResult.CopyTo(ref pooledArray1);
                        var otherCurrencyTry = ConvertToDiscrete_S(possibleResult[i].Currency, amountToRemove, ref pooledArray2);
                        var newResult = DiscreteSubtraction_S(copyPossibleResult, otherCurrencyTry, ref result);

                        GenericObjectPool.Return(pooledArray1);
                        GenericObjectPool.Return(pooledArray2);

                        return newResult;
                    }

                    //Not found same root currency
                    GenericObjectPool.Return(extractedFractionsPooledArray);
                    return possibleResult;
                }

                var absDiff = Mathf.Abs(diff);

                var mod = (int)(absDiff % rate);
                var div = (int)(absDiff / rate);
                var remainder = 1 + div;

                var newAmount = Mathf.Clamp(0, (int)(rate - mod), currency.MaxAmount);
                result[index] = (newAmount, currency);

                currency = overflowCurrency;
                amount = remainder;
            }

            return (result, 0, count);
        }

        /// <summary>
        /// Find a currency that fractions to the currency specified
        /// </summary>
        /// <param name="resizableArrayToSearch">The array to search for the currencies.</param>
        /// <param name="fractionCurrency">The fractionCurrency.</param>
        /// <param name="currency">The result currency, if found.</param>
        /// <returns>Returns true if found.</returns>
        private static bool FindCurrencyThatFractionTo(ListSlice<CurrencyAmount> resizableArrayToSearch, Currency fractionCurrency, out Currency currency)
        {
            if (fractionCurrency.OverflowCurrency != null) {
                currency = fractionCurrency.OverflowCurrency;
                return true;
            }

            for (int i = 0; i < resizableArrayToSearch.Count; i++) {
                if (resizableArrayToSearch[i].Amount <= 0) { continue; }
                currency = resizableArrayToSearch[i].Currency;

                if (currency.GetRoot() != fractionCurrency.GetRoot()) { continue; }

                while (true) {
                    if (currency.FractionCurrency == null) { break; }

                    if (currency.FractionCurrency == fractionCurrency) {
                        return true;
                    }

                    currency = currency.FractionCurrency;
                }
            }

            currency = null;
            return false;
        }

        /// <summary>
        /// Discrete division, assumes both operands are in a discrete format.
        /// </summary>
        /// <param name="lhs">Left hand side discrete currency amounts.</param>
        /// <param name="rhs">Right hand side discrete currency amounts.</param>
        /// <param name="result">The array that will store the result remainder.</param>
        /// <returns>Tuple of The quotient as int, and The array struct containing the resulting remainder array.</returns>
        private static (int, ListSlice<CurrencyAmount>) DiscreteDivision_S(
            ListSlice<CurrencyAmount> lhs,
            ListSlice<CurrencyAmount> rhs, ref CurrencyAmount[] result)
        {
            var quotient = DiscreteQuotient_S(lhs, rhs);
            var remainder = Subtraction_S(lhs, 1, rhs, quotient, ref result);

            return (quotient, remainder);
        }

        /// <summary>
        /// Discrete quotient, assumes both operands are in a discrete format. The number of times rhs fits inside lhs.
        /// </summary>
        /// <param name="lhs">Left hand side discrete currency amounts.</param>
        /// <param name="rhs">Right hand side discrete currency amounts.</param>
        /// <returns>The quotient.</returns>
        private static int DiscreteQuotient_S(ListSlice<CurrencyAmount> lhs, ListSlice<CurrencyAmount> rhs)
        {
            return DiscreteQuotientEquivalent_S(lhs, rhs, out var equivalent);
        }

        /// <summary>
        /// Discrete quotient, assumes both operands are in discrete format. The number of times rhs fits inside lhs.
        /// Also checks if rhs and lhs are equivalent.
        /// </summary>
        /// <param name="lhs">Left hand side discrete currency amounts.</param>
        /// <param name="rhs">Right hand side discrete currency amounts.</param>
        /// <param name="equivalent">Outputs if lhs and rhs are equivalent.</param>
        /// <returns>The quotient.</returns>
        private static int DiscreteQuotientEquivalent_S(ListSlice<CurrencyAmount> lhs, ListSlice<CurrencyAmount> rhs, out bool equivalent)
        {
            equivalent = true;

            //Required because the outer loop might not reach the inner loop.
            if (rhs.Count == 0) {
                if (lhs.Count == 0) {
                    equivalent = true;
                    return 1;
                }

                equivalent = false;
                return int.MaxValue;
            }

            //Make a copy to reorder without causing outside trouble.
            var pooledArray = GenericObjectPool.Get<CurrencyAmount[]>();
            var rhsCopy = rhs.CopyTo(ref pooledArray);
            Array.Sort(pooledArray, 0, rhsCopy.Count, s_CurrencyAmountRootComparer);

            var quotient = int.MaxValue;
            var count = 0d;
            var fullAmountAsRoot = 0ul;
            Currency previousRootCurrency = null;
            for (int i = 0; i < rhsCopy.Count; i++) {
                var otherCurrencyAmount = rhsCopy[i];
                if (otherCurrencyAmount.Currency == null) {
                    continue;
                }
                var rootExchangeRate = otherCurrencyAmount.Currency.GetRootExchangeRate();
                var rootCurrency = rootExchangeRate.Currency;
                if (rootCurrency != previousRootCurrency) {

                    if (count != 0) {
                        if (fullAmountAsRoot > count) { equivalent = false; }
                        quotient = Mathf.Min(quotient, ToIntSafe(fullAmountAsRoot / (ulong)count));
                    }

                    previousRootCurrency = rootCurrency;
                    fullAmountAsRoot = GetFullAmountAsRootCurrency(lhs, rootCurrency);
                    count = 0d;
                }

                count += otherCurrencyAmount.Amount * rootExchangeRate.ExchangeRate;
                if (fullAmountAsRoot < count) {
                    //Do not have enough currency
                    equivalent = false;
                    return 0;
                }
            }

            if (fullAmountAsRoot > count) { equivalent = false; }
            return count == 0 ? int.MaxValue : Mathf.Min(quotient, ToIntSafe(fullAmountAsRoot / (ulong)count));
        }

        /// <summary>
        /// Get full amount of the currencies in the the currency amounts as the root currency of the currency specified.
        /// </summary>
        /// <param name="currencyAmounts">The currency amounts.</param>
        /// <param name="currency">The currency used to find the root.</param>
        /// <returns>The unsigned long amount of root currency.</returns>
        public static ulong GetFullAmountAsRootCurrency(ListSlice<CurrencyAmount> currencyAmounts, Currency currency)
        {
            if (currency == null) { return 0; }
            var rootCurrency = currency.GetRoot();
            var totalAmount = 0ul;
            for (int i = 0; i < currencyAmounts.Count; i++) {
                var rootCurrencyExchange = currencyAmounts[i].Currency.GetRootExchangeRate();
                if (rootCurrencyExchange.Currency == rootCurrency) {
                    totalAmount += (ulong)(currencyAmounts[i].Amount * rootCurrencyExchange.ExchangeRate);
                }
            }

            return totalAmount;
        }

        /// <summary>
        /// Sets all the currency amounts with the currencies that are inherently fractional currencies of the currency specified.
        /// </summary>
        /// <param name="currency">The currency.</param>
        /// <param name="result">The array that will store the result.</param>
        /// <returns>The array struct containing the resulting array.</returns>
        public static ListSlice<CurrencyAmount> MaxedOutAmount_S(Currency currency, ref CurrencyAmount[] result)
        {
            var index = 0;
            while (true) {
                if (currency == null) {
                    return (result, 0, index);
                }

                TypeUtility.ResizeIfNecessary(ref result, index + 1);

                result[index] = (currency, currency.MaxAmount);
                index++;

                currency = currency.FractionCurrency;
            }
        }

        /// <summary>
        /// Sets all the fractions of the currency to the max amount.
        /// </summary>
        /// <param name="currencyAmounts">The currency amounts.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="result">The array that will store the result.</param>
        /// <returns>The array struct containing the resulting array.</returns>
        private static ListSlice<CurrencyAmount> SetAllFractionToMax(ListSlice<CurrencyAmount> currencyAmounts, Currency currency, ref CurrencyAmount[] result)
        {
            var count = currencyAmounts.CopyTo(ref result).Count;

            while (true) {
                if (currency == null) { return (result, 0, count); }

                var index = FindOrCreateCurrencyIndex(currency, ref result, ref count);

                result[index] = (currency.MaxAmount, currency);

                currency = currency.FractionCurrency;
            }
        }

        /// <summary>
        /// Sets all the fractions of the currency to zero amount.
        /// </summary>
        /// <param name="currencyAmounts">The currency amounts.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="result">The array that will store the result.</param>
        /// <returns>The count of currency amounts.</returns>
        private static ListSlice<CurrencyAmount> SetAllFractionToZero(ListSlice<CurrencyAmount> currencyAmounts, Currency currency, ref CurrencyAmount[] result)
        {
            var count = currencyAmounts.CopyTo(ref result).Count;

            while (true) {
                if (currency == null) { return (result, 0, count); }

                var index = FindOrCreateCurrencyIndex(currency, ref result, ref count);

                result[index] = (0, currency);

                currency = currency.FractionCurrency;
            }
        }

        /// <summary>
        /// Extract the subset currency amounts that are fraction currencies of the currency specified.
        /// </summary>
        /// <param name="currencyAmounts">The currency amounts.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="result">The array that will store the result.</param>
        /// <returns>The array struct containing the resulting array.</returns>
        private static ListSlice<CurrencyAmount> ExtractFraction(ListSlice<CurrencyAmount> currencyAmounts, Currency currency, ref CurrencyAmount[] result)
        {
            var count = 0;

            while (true) {
                if (currency == null) { return (result, 0, count); }

                var resultIndex = FindOrCreateCurrencyIndex(currency, ref result, ref count);
                var currencyAmountsIndex = FindIndexWithCurrency(currency, currencyAmounts);

                if (currencyAmountsIndex != -1) {
                    result[resultIndex] = (currencyAmounts[currencyAmountsIndex].Amount, currency);
                }

                currency = currency.FractionCurrency;
            }
        }

        /// <summary>
        /// Subtraction of a currency amounts. result = lhs*lm - rhs*rm
        /// </summary>
        /// <param name="lhs">Left hand side currency amounts.</param>
        /// <param name="lm">Left hand side multiplier.</param>
        /// <param name="rhs">Right hand side currency amounts.</param>
        /// <param name="rm">Right hand side multiplier.</param>
        /// <param name="result">The array that will store the result.</param>
        /// <returns>The array struct containing the resulting array.</returns>
        public static ListSlice<CurrencyAmount> Subtraction_S(ListSlice<CurrencyAmount> lhs, double lm, ListSlice<CurrencyAmount> rhs, double rm, ref CurrencyAmount[] result)
        {
            var pooledArray1 = GenericObjectPool.Get<CurrencyAmount[]>();
            var pooledArray2 = GenericObjectPool.Get<CurrencyAmount[]>();

            var lhsConverted = ConvertToDiscrete_S(lhs, lm, ref pooledArray1);
            var rhsConverted = ConvertToDiscrete_S(rhs, rm, ref pooledArray2);

            var resultArrayStruct = DiscreteSubtraction_S(lhsConverted, rhsConverted, ref result);

            GenericObjectPool.Return(pooledArray1);
            GenericObjectPool.Return(pooledArray2);

            return resultArrayStruct;
        }

        /// <summary>
        /// Addition of a currency amounts. result = lhs*lm + rhs*rm
        /// </summary>
        /// <param name="lhs">Left hand side currency amounts.</param>
        /// <param name="lm">Left hand side multiplier.</param>
        /// <param name="rhs">Right hand side currency amounts.</param>
        /// <param name="rm">Right hand side multiplier.</param>
        /// <param name="result">The array that will store the result.</param>
        /// <returns>The array struct containing the resulting array.</returns>
        public static ListSlice<CurrencyAmount> Addition_S(ListSlice<CurrencyAmount> lhs, double lm, ListSlice<CurrencyAmount> rhs, double rm, ref CurrencyAmount[] result)
        {
            var pooledArray1 = GenericObjectPool.Get<CurrencyAmount[]>();
            var pooledArray2 = GenericObjectPool.Get<CurrencyAmount[]>();

            var lhsConverted = ConvertToDiscrete_S(lhs, lm, ref pooledArray1);
            var rhsConverted = ConvertToDiscrete_S(rhs, rm, ref pooledArray2);

            var resultArrayStruct = DiscreteAddition_S(lhsConverted, rhsConverted, ref result);

            GenericObjectPool.Return(pooledArray1);
            GenericObjectPool.Return(pooledArray2);

            return resultArrayStruct;
        }

        /// <summary>
        /// Division of a currency amounts. result quotient = lhs*lm / rhs*rm, result remainder = lhs*lm % rhs*rm
        /// </summary>
        /// <param name="lhs">Left hand side currency amounts.</param>
        /// <param name="lm">Left hand side multiplier.</param>
        /// <param name="rhs">Right hand side currency amounts.</param>
        /// <param name="rm">Right hand side multiplier.</param>
        /// <param name="result">The array that will store the resulting remainder.</param>
        /// <returns>Tuple of the quotient, and the array struct containing the resulting remainder array.</returns>
        public static (int, ListSlice<CurrencyAmount>) Division_S(ListSlice<CurrencyAmount> lhs, double lm, ListSlice<CurrencyAmount> rhs, double rm, ref CurrencyAmount[] result)
        {
            var pooledArray1 = GenericObjectPool.Get<CurrencyAmount[]>();
            var pooledArray2 = GenericObjectPool.Get<CurrencyAmount[]>();

            var lhsConverted = ConvertToDiscrete_S(lhs, lm, ref pooledArray1);
            var rhsConverted = ConvertToDiscrete_S(rhs, rm, ref pooledArray2);

            var quotient_remainder = DiscreteDivision_S(lhsConverted, rhsConverted, ref result);

            GenericObjectPool.Return(pooledArray1);
            GenericObjectPool.Return(pooledArray2);

            return quotient_remainder;
        }

        /// <summary>
        /// Returns true if lhs*lm >= rhs*rm. Note that it is NOT equivalent to !(lhs*lm < rhs*rm.) because some currencies may not convert from one to another.
        /// </summary>
        /// <param name="lhs">Left hand side currency amounts.</param>
        /// <param name="lm">Left hand side multiplier.</param>
        /// <param name="rhs">Right hand side currency amounts.</param>
        /// <param name="rm">Right hand side multiplier.</param>
        /// <returns>True if greater or equal to.</returns>
        public static bool GreaterThanOrEqualTo(ListSlice<CurrencyAmount> lhs, double lm,
            ListSlice<CurrencyAmount> rhs, double rm)
        {
            var pooledArray1 = GenericObjectPool.Get<CurrencyAmount[]>();
            var pooledArray2 = GenericObjectPool.Get<CurrencyAmount[]>();

            var lhsDiscrete = ConvertToDiscrete_S(lhs, lm, ref pooledArray1);
            var rhsDiscrete = ConvertToDiscrete_S(rhs, rm, ref pooledArray2);

            var quotient = DiscreteQuotient_S(lhsDiscrete, rhsDiscrete);

            GenericObjectPool.Return(pooledArray1);
            GenericObjectPool.Return(pooledArray2);

            return quotient >= 1;
        }

        /// <summary>
        /// Returns true if lhs*lm == rhs*rm.
        /// </summary>
        /// <param name="lhs">Left hand side currency amounts.</param>
        /// <param name="lm">Left hand side multiplier.</param>
        /// <param name="rhs">Right hand side currency amounts.</param>
        /// <param name="rm">Right hand side multiplier.</param>
        /// <returns>True if equivalent.</returns>
        public static bool AreEquivalent(ListSlice<CurrencyAmount> lhs, double lm,
            ListSlice<CurrencyAmount> rhs, double rm)
        {
            var pooledArray1 = GenericObjectPool.Get<CurrencyAmount[]>();
            var pooledArray2 = GenericObjectPool.Get<CurrencyAmount[]>();

            var lhsDiscrete = ConvertToDiscrete_S(lhs, lm, ref pooledArray1);
            var rhsDiscrete = ConvertToDiscrete_S(rhs, rm, ref pooledArray2);

            DiscreteQuotientEquivalent_S(lhsDiscrete, rhsDiscrete, out var equivalent);

            GenericObjectPool.Return(pooledArray1);
            GenericObjectPool.Return(pooledArray2);

            return equivalent;
        }

        /// <summary>
        /// Currency amounts to string.
        /// </summary>
        /// <param name="currencyAmounts">Currency amounts.</param>
        /// <returns>The string representation.</returns>
        public static string CurrencyAmountsToString(ListSlice<CurrencyAmount> currencyAmounts)
        {
            var result = string.Empty;
            for (int i = 0; i < currencyAmounts.Count; i++) {
                result += $" {currencyAmounts[i]}";
            }

            return result;
        }

        #endregion

        #region Set

        /// <summary>
        /// Set the currency amounts of the collection.
        /// </summary>
        /// <param name="currencyAmounts">Currency amounts.</param>
        /// <param name="multiplier">Multiplier.</param>
        /// <returns>True if added correctly.</returns>
        public bool SetCurrency(CurrencyAmount[] currencyAmounts, float multiplier = 1)
        {
            m_CurrencyAmounts.Clear();
            return AddCurrency(currencyAmounts, multiplier);
        }

        /// <summary>
        /// Set the currency amounts of the collection.
        /// </summary>
        /// <param name="currencyAmounts">Currency amounts.</param>
        /// <param name="multiplier">Multiplier.</param>
        /// <returns>True if added correctly.</returns>
        public bool SetCurrency(ListSlice<CurrencyAmount> currencyAmounts, float multiplier = 1)
        {
            m_CurrencyAmounts.Clear();
            return AddCurrency(currencyAmounts, multiplier);
        }

        /// <summary>
        /// Set the currency amounts of the collection.
        /// </summary>
        /// <param name="currencyCollection">The currency collection to set.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <returns>True if added correctly.</returns>
        public bool SetCurrency(CurrencyCollection currencyCollection, float multiplier = 1)
        {
            m_CurrencyAmounts.Clear();
            return AddCurrency(currencyCollection, multiplier);
        }

        #endregion

        #region Add

        /// <summary>
        /// Addition of a currency amounts. result = lhs*lm + rhs*rm
        /// </summary>
        /// <param name="lhs">Left hand side currency amounts.</param>
        /// <param name="lm">Left hand side multiplier.</param>
        /// <param name="rhs">Right hand side currency amounts.</param>
        /// <param name="rm">Right hand side multiplier.</param>
        /// <param name="result">The array that will store the result.</param>
        /// <returns>The array struct containing the resulting array.</returns>
        public virtual ListSlice<CurrencyAmount> Addition(ListSlice<CurrencyAmount> lhs, double lm,
            ListSlice<CurrencyAmount> rhs, double rm, ref CurrencyAmount[] result)
        {
            return Addition_S(lhs, lm, rhs, rm, ref result);
        }

        /// <summary>
        /// Discrete addition, assumes that both operands are in a discrete format.
        /// </summary>
        /// <param name="lhs">Left hand side discrete currency amounts.</param>
        /// <param name="rhs">Right hand side discrete currency amounts.</param>
        /// <param name="result">The array that will store the result.</param>
        /// <returns>The array struct containing the resulting array.</returns>
        protected virtual ListSlice<CurrencyAmount> DiscreteAddition(
            ListSlice<CurrencyAmount> lhs,
            ListSlice<CurrencyAmount> rhs, ref CurrencyAmount[] result)
        {
            return DiscreteAddition_S(lhs, rhs, ref result);
        }

        /// <summary>
        /// Add currency to the currency collection.
        /// </summary>
        /// <param name="currency">The currency to add.</param>
        /// <param name="amount">The amount to add.</param>
        /// <param name="notify">Notify that currency was added.</param>
        /// <returns>True if added correctly.</returns>
        public virtual bool AddCurrency(Currency currency, double amount, bool notify = true)
        {
            var result = AddCurrencyInternal(currency, amount, notify);
            return result;
        }

        /// <summary>
        /// Add currency to the currency collection.
        /// </summary>
        /// <param name="currencyAmounts">The currency amounts to add.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <param name="notify">Notify that currency was added.</param>
        /// <returns>True if added correctly.</returns>
        public virtual bool AddCurrency(ListSlice<CurrencyAmount> currencyAmounts, double multiplier = 1, bool notify = true)
        {
            var result = AddCurrencyInternal(currencyAmounts, multiplier, notify);

            return result;
        }

        /// <summary>
        /// Add currency to the currency collection.
        /// </summary>
        /// <param name="currencyCollection">The currency amounts to add.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <param name="notify">Notify that currency was added.</param>
        /// <returns>True if added correctly.</returns>
        public virtual bool AddCurrency(CurrencyCollection currencyCollection, double multiplier = 1, bool notify = true)
        {
            return AddCurrency(currencyCollection.m_CurrencyAmounts, multiplier, notify);
        }

        /// <summary>
        /// Send an event that currency was added to the collection.
        /// </summary>
        protected virtual void NotifyAdd(ListSlice<CurrencyAmount> amount)
        {
            OnAdd?.Invoke(amount);
            EventHandler.ExecuteEvent(this, EventNames.c_CurrencyCollection_OnUpdate);
        }

        /// <summary>
        /// Add currency to the collection.
        /// </summary>
        /// <param name="currency">Currency to add.</param>
        /// <param name="amount">The amount to add.</param>
        /// <returns>True if added correctly.</returns>
        protected virtual bool AddCurrencyInternal(Currency currency, double amount, bool notify)
        {

            var pooledArray1 = GenericObjectPool.Get<CurrencyAmount[]>();
            var discreteCurrencyAmounts = ConvertToDiscrete(currency, amount, ref pooledArray1);

            var pooledArray2 = GenericObjectPool.Get<CurrencyAmount[]>();
            var currencyAmountCopy = ((ListSlice<CurrencyAmount>)m_CurrencyAmounts).CopyTo(ref pooledArray2);

            //m_CurrencyAmounts is about to be overwritten so return the array to the pool
            GenericObjectPool.Return(m_CurrencyAmounts.Array);
            var pooledArray3 = GenericObjectPool.Get<CurrencyAmount[]>();
            var added = DiscreteAddition(currencyAmountCopy, discreteCurrencyAmounts, ref pooledArray3);
            m_CurrencyAmounts.Initialize(pooledArray3, false, added.Count);

            if (notify) { NotifyAdd(discreteCurrencyAmounts); }

            GenericObjectPool.Return(pooledArray1);
            GenericObjectPool.Return(pooledArray2);
            //Don't return pooledArray3 because it is now used by m_CurrencyAmounts

            return true;
        }

        /// <summary>
        /// Add currency to the collection.
        /// </summary>
        /// <param name="currencyAmounts">The currency amounts to add.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <returns>True if added correctly.</returns>
        protected virtual bool AddCurrencyInternal(ListSlice<CurrencyAmount> currencyAmounts, double multiplier, bool notify)
        {
            var pooledArray = GenericObjectPool.Get<CurrencyAmount[]>();
            var lhsCurrencyAmountCopy = ListSlice.CopyTo(m_CurrencyAmounts, ref pooledArray);

            //m_CurrencyAmounts is about to be overwritten so return the array to the pool
            GenericObjectPool.Return(m_CurrencyAmounts.Array);
            var pooledArray2 = GenericObjectPool.Get<CurrencyAmount[]>();
            var added = Addition(lhsCurrencyAmountCopy, 1d, currencyAmounts, multiplier, ref pooledArray2);
            m_CurrencyAmounts.Initialize(pooledArray2, false, added.Count);

            if (notify) {
                var addedAmountPooledArray = GenericObjectPool.Get<CurrencyAmount[]>();
                var discreteAddedAmount = ConvertToDiscrete(currencyAmounts, multiplier, ref addedAmountPooledArray);
                
                NotifyAdd(discreteAddedAmount);
                GenericObjectPool.Return(addedAmountPooledArray);
            }

            GenericObjectPool.Return(pooledArray);
            //Don't return pooledArray2 because it is now used by m_CurrencyAmounts

            return true;
        }

        #endregion

        #region Remove

        /// <summary>
        /// Subtraction of a currency amounts. result = lhs*lm - rhs*rm
        /// </summary>
        /// <param name="lhs">Left hand side currency amounts.</param>
        /// <param name="lm">Left hand side multiplier.</param>
        /// <param name="rhs">Right hand side currency amounts.</param>
        /// <param name="rm">Right hand side multiplier.</param>
        /// <param name="result">The array that will store the result.</param>
        /// <returns>The array struct containing the resulting array.</returns>
        public virtual ListSlice<CurrencyAmount> Subtraction(ListSlice<CurrencyAmount> lhs, double lm,
            ListSlice<CurrencyAmount> rhs, double rm, ref CurrencyAmount[] result)
        {
            return Subtraction_S(lhs, lm, rhs, rm, ref result);
        }

        /// <summary>
        /// Discrete subtraction, assumes that both operands are in a discrete format.
        /// </summary>
        /// <param name="lhs">Left hand side discrete currency amounts.</param>
        /// <param name="rhs">Right hand side discrete currency amounts.</param>
        /// <param name="result">The array that will store the result.</param>
        /// <returns>The array struct containing the resulting array.</returns>
        protected virtual ListSlice<CurrencyAmount> DiscreteSubtraction(
            ListSlice<CurrencyAmount> lhs,
            ListSlice<CurrencyAmount> rhs, ref CurrencyAmount[] result)
        {
            return DiscreteSubtraction_S(lhs, rhs, ref result);
        }

        /// <summary>
        /// Remove currency from collection.
        /// </summary>
        /// <param name="currency">Currency to remove.</param>
        /// <param name="amount">Amount to remove.</param>
        /// <param name="notify">Notify if currency was removed.</param>
        /// <returns>True if removed correctly.</returns>
        public virtual bool RemoveCurrency(Currency currency, double amount, bool notify = true)
        {
            var result = RemoveCurrencyInternal(currency, amount, notify);
            return result;
        }

        /// <summary>
        /// Remove currency from collection.
        /// </summary>
        /// <param name="currencyAmounts">Currency amounts to remove.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <param name="notify">Notify if currency was removed.</param>
        /// <returns>True if removed correctly.</returns>
        public virtual bool RemoveCurrency(ListSlice<CurrencyAmount> currencyAmounts, double multiplier = 1, bool notify = true)
        {
            var result = RemoveCurrencyInternal(currencyAmounts, multiplier, notify);
            return result;
        }

        /// <summary>
        /// Remove currency from collection.
        /// </summary>
        /// <param name="currencyAmounts">Currency amounts to remove.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <param name="notify">Notify if currency was removed.</param>
        /// <returns>True if removed correctly.</returns>
        public virtual bool RemoveCurrency(CurrencyAmount[] currencyAmounts, double multiplier = 1, bool notify = true)
        {
            if (currencyAmounts == null) { return false; }

            var currencyAmountsStruct = (currencyAmounts, 0, currencyAmounts.Length);
            return RemoveCurrency(currencyAmountsStruct, multiplier, notify);
        }

        /// <summary>
        /// Remove currency from collection.
        /// </summary>
        /// <param name="currencyCollection">Currency amounts to remove.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <param name="notify">Notify if currency was removed.</param>
        /// <returns>True if removed correctly.</returns>
        public virtual bool RemoveCurrency(CurrencyCollection currencyCollection, double multiplier = 1, bool notify = true)
        {
            return RemoveCurrency(currencyCollection.m_CurrencyAmounts, multiplier, notify);
        }

        /// <summary>
        /// Send an event that currency was removed from the collection.
        /// </summary>
        protected virtual void NotifyRemove(ListSlice<CurrencyAmount> amount)
        {
            OnRemove?.Invoke(amount);
            EventHandler.ExecuteEvent(this, EventNames.c_CurrencyCollection_OnUpdate);
        }

        /// <summary>
        /// Remove currency from collection.
        /// </summary>
        /// <param name="currency">Currency to remove.</param>
        /// <param name="amount">Amount to remove.</param>
        /// <returns>True if removed correctly.</returns>
        protected virtual bool RemoveCurrencyInternal(Currency currency, double amount, bool notify)
        {
            var pooledArray1 = GenericObjectPool.Get<CurrencyAmount[]>();
            var discreteCurrencyAmounts = ConvertToDiscrete(currency, amount, ref pooledArray1);

            var pooledArray2 = GenericObjectPool.Get<CurrencyAmount[]>();
            var currencyAmountCopy = ListSlice.CopyTo(m_CurrencyAmounts, ref pooledArray2);

            //m_CurrencyAmounts is about to be overwritten so return the array to the pool
            GenericObjectPool.Return(m_CurrencyAmounts.Array);
            var pooledArray3 = GenericObjectPool.Get<CurrencyAmount[]>();
            var subtracted = DiscreteSubtraction(currencyAmountCopy, discreteCurrencyAmounts, ref pooledArray3);
            m_CurrencyAmounts.Initialize(pooledArray3, false, subtracted.Count);

            if (notify) { NotifyRemove(discreteCurrencyAmounts); }

            GenericObjectPool.Return(pooledArray1);
            GenericObjectPool.Return(pooledArray2);
            //Don't return pooledArray3 because it is now used by m_CurrencyAmounts

            return true;
        }

        /// <summary>
        /// Remove currency from collection.
        /// </summary>
        /// <param name="currencyAmounts">Currency amounts to remove.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <returns>True if removed correctly.</returns>
        protected virtual bool RemoveCurrencyInternal(ListSlice<CurrencyAmount> currencyAmounts, double multiplier, bool notify)
        {
            var pooledArray = GenericObjectPool.Get<CurrencyAmount[]>();
            var lhsCurrencyAmountCopy = ListSlice.CopyTo(m_CurrencyAmounts, ref pooledArray);

            //m_CurrencyAmounts is about to be overwritten so return the array to the pool
            GenericObjectPool.Return(m_CurrencyAmounts.Array);
            var pooledArray2 = GenericObjectPool.Get<CurrencyAmount[]>();
            var subtracted = Subtraction(lhsCurrencyAmountCopy, 1d, currencyAmounts, multiplier, ref pooledArray2);
            m_CurrencyAmounts.Initialize(pooledArray2, false, subtracted.Count);

            if (notify) {
                var removedAmountPooledArray = GenericObjectPool.Get<CurrencyAmount[]>();
                var discreteAmountRemoved = ConvertToDiscrete(currencyAmounts, multiplier, ref removedAmountPooledArray);

                NotifyRemove(discreteAmountRemoved);
                GenericObjectPool.Return(removedAmountPooledArray);
            }

            GenericObjectPool.Return(pooledArray);
            //Don't return pooledArray2 because it is now used by m_CurrencyAmounts

            return true;
        }

        /// <summary>
        /// Remove all the currency amounts from this collection.
        /// </summary>
        public void RemoveAll()
        {
            var removedAmountPooledArray = GenericObjectPool.Get<CurrencyAmount[]>();
            ListSlice.CopyTo(m_CurrencyAmounts, ref removedAmountPooledArray);

            m_CurrencyAmounts.Clear();

            NotifyRemove(removedAmountPooledArray);
            GenericObjectPool.Return(removedAmountPooledArray);
        }

        #endregion

        #region Getters

        /// <summary>
        /// Get the amount of the currency specified.
        /// </summary>
        /// <param name="currency">The currency.</param>
        /// <returns>The amounts.</returns>
        public int GetAmountOf(Currency currency)
        {
            var index = FindIndexWithCurrency(currency, m_CurrencyAmounts);
            if (index == -1) { return 0; }

            return m_CurrencyAmounts[index].Amount;
        }

        /// <summary>
        /// Get how many times the currency amounts specified fits within the amounts of the collection.
        /// </summary>
        /// <param name="currencyAmounts">The currency amounts.</param>
        /// <returns>The quotient.</returns>
        public int PotentialQuotientFor(ListSlice<CurrencyAmount> currencyAmounts)
        {
            return DiscreteQuotient_S(m_CurrencyAmounts, currencyAmounts);
        }

        /// <summary>
        /// Get how many times the currency amounts specified fits within the amounts of the collection.
        /// </summary>
        /// <param name="currencyAmounts">The currency amounts.</param>
        /// <returns>The quotient.</returns>
        public int PotentialQuotientFor(CurrencyAmount[] currencyAmounts)
        {
            if (currencyAmounts == null) { return 0; }

            var currencyAmountsStruct = (currencyAmounts, 0, currencyAmounts.Length);
            return DiscreteQuotient_S(m_CurrencyAmounts, currencyAmountsStruct);
        }

        /// <summary>
        /// Get how many times the currency amounts specified fits within the amounts of the collection.
        /// </summary>
        /// <param name="other">The currency amounts.</param>
        /// <returns>The quotient.</returns>
        public int PotentialQuotientFor(CurrencyCollection other)
        {
            if (other == null) {
                return 0;
            }
            return PotentialQuotientFor(other.m_CurrencyAmounts);
        }

        /// <summary>
        /// Get the full amount of currency for the root of the currency specified.
        /// </summary>
        /// <param name="currency">The currency.</param>
        /// <returns>The full amount.</returns>
        public virtual ulong GetFullAmountAsRootCurrency(Currency currency)
        {
            return GetFullAmountAsRootCurrency(m_CurrencyAmounts, currency);
        }

        /// <summary>
        /// Get the currency amounts.
        /// </summary>
        /// <returns>The counts.</returns>
        public ListSlice<CurrencyAmount> GetCurrencyAmounts()
        {
            return m_CurrencyAmounts;
        }

        /// <summary>
        /// Get the number of different currencies are part of the collection.
        /// </summary>
        /// <returns>The counts.</returns>
        public int GetCurrencyAmountCount()
        {
            return m_CurrencyAmounts.Count;
        }

        /// <summary>
        /// Get the currency amount at the index of the array struct.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The currency amount.</returns>
        public CurrencyAmount GetCurrencyAmountAt(int index)
        {
            return m_CurrencyAmounts[index];
        }

        #endregion

        #region Comparators

        /// <summary>
        /// Check if the collection has at least that amount of currency.
        /// </summary>
        /// <param name="currency">The currency.</param>
        /// <param name="amount">The amount.</param>
        /// <returns>True if the collection has more or equal that amount.</returns>
        public virtual bool HasCurrency(Currency currency, double amount)
        {
            var currentAmountAsRoot = GetFullAmountAsRootCurrency(currency);
            var amountAsRoot = currency.GetRootExchangeRate().ExchangeRate * amount;
            return currentAmountAsRoot >= amountAsRoot;
        }

        /// <summary>
        /// Check if the collection has at least that amounts of currencies.
        /// </summary>
        /// <param name="currencyAmounts">The currency amounts.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <returns>True if the collection has more or equal that amount.</returns>
        public virtual bool HasCurrency(ListSlice<CurrencyAmount> currencyAmounts, double multiplier = 1)
        {
            return HasCurrencyInternal(currencyAmounts, multiplier);
        }

        /// <summary>
        /// Check if the collection has at least that amounts of currencies.
        /// </summary>
        /// <param name="currencyAmounts">The currency amounts.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <returns>True if the collection has more or equal that amount.</returns>
        public virtual bool HasCurrency(CurrencyAmount[] currencyAmounts, double multiplier = 1)
        {
            if (currencyAmounts == null) { return false; }

            var currencyAmountsStruct = (currencyAmounts, 0, currencyAmounts.Length);
            return HasCurrency(currencyAmountsStruct, multiplier);
        }

        /// <summary>
        /// Check if the collection has at least that amounts of currencies.
        /// </summary>
        /// <param name="currencyCollection">The currency amounts.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <returns>True if the collection has more or equal that amount.</returns>
        public virtual bool HasCurrency(CurrencyCollection currencyCollection, double multiplier = 1)
        {
            return HasCurrency(currencyCollection.m_CurrencyAmounts, multiplier);
        }

        /// <summary>
        /// Check if the collection has at least that amounts of currencies.
        /// </summary>
        /// <param name="currencyAmounts">The currency amounts.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <returns>True if the collection has more or equal that amount.</returns>
        protected virtual bool HasCurrencyInternal(ListSlice<CurrencyAmount> currencyAmounts, double multiplier)
        {
            return GreaterThanOrEqualTo(m_CurrencyAmounts, 1, currencyAmounts, multiplier);
        }

        /// <summary>
        /// Check if the collection has the same amounts of currencies.
        /// </summary>
        /// <param name="currencyAmounts">The currency amounts.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <returns>True if the collection has an equivalent amounts of currencies.</returns>
        public virtual bool EquivalentTo(ListSlice<CurrencyAmount> currencyAmounts, double multiplier = 1)
        {
            return EquivalentToInternal(currencyAmounts, multiplier);
        }

        /// <summary>
        /// Check if the collection has the same amounts of currencies.
        /// </summary>
        /// <param name="currencyAmounts">The currency amounts.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <returns>True if the collection has an equivalent amounts of currencies.</returns>
        public virtual bool EquivalentTo(CurrencyAmount[] currencyAmounts, double multiplier = 1)
        {
            if (currencyAmounts == null) { return false; }

            var currencyAmountsStruct = (currencyAmounts, 0, currencyAmounts.Length);
            return EquivalentTo(currencyAmountsStruct, multiplier);
        }

        /// <summary>
        /// Check if the collection has the same amounts of currencies.
        /// </summary>
        /// <param name="currencyCollection">The currency amounts.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <returns>True if the collection has an equivalent amounts of currencies.</returns>
        public virtual bool EquivalentTo(CurrencyCollection currencyCollection, double multiplier = 1)
        {
            return EquivalentTo(currencyCollection.m_CurrencyAmounts, multiplier);
        }

        /// <summary>
        /// Check if the collection has the same amounts of currencies.
        /// </summary>
        /// <param name="currencyAmounts">The currency amounts.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <returns>True if the collection has an equivalent amounts of currencies.</returns>
        protected virtual bool EquivalentToInternal(ListSlice<CurrencyAmount> currencyAmounts, double multiplier)
        {
            return AreEquivalent(m_CurrencyAmounts, 1, currencyAmounts, multiplier);
        }

        /// <summary>
        /// Returns true if the currency amount >= rhs*rm. Note that it is NOT equivalent to !(lhs*lm < rhs*rm.)
        /// because some currencies may not convert from one to another.
        /// </summary>
        /// <param name="currencyAmounts">The currency amounts.</param>
        /// <param name="multiplier">The multiplier.</param>
        /// <returns>True if greater or equal to.</returns>
        public bool GreaterThanOrEqualTo(ListSlice<CurrencyAmount> currencyAmounts, double multiplier = 1)
        {
            return GreaterThanOrEqualTo(m_CurrencyAmounts, 1, currencyAmounts, multiplier);
        }

        #endregion

        #region Conversions To Discrete

        /// <summary>
        /// Convert the currency amounts specified to a discrete format, meaning a format that follows the constraints of the currencies.
        /// </summary>
        /// <param name="currencyAmountsInput">The currency amounts.</param>
        /// <param name="multiplier">A multiplier for the currency amounts.</param>
        /// <param name="resultCurrencyAmounts">The array that will store the result.</param>
        /// <returns>The array struct containing the resulting array.</returns>
        public virtual ListSlice<CurrencyAmount> ConvertToDiscrete(ListSlice<CurrencyAmount> currencyAmountsInput, double multiplier, ref CurrencyAmount[] resultCurrencyAmounts)
        {
            return ConvertToDiscrete_S(currencyAmountsInput, multiplier, ref resultCurrencyAmounts);
        }

        /// <summary>
        /// Convert a single currency amount to a set of currencyAmounts which are in a discrete format, meaning a format that follows the constraints of the currencies.
        /// </summary>
        /// <param name="currency">The currency.</param>
        /// <param name="amount">The amounts.</param>
        /// <param name="currencyAmounts">The array that will store the result.</param>
        /// <returns>The array struct containing the resulting array.</returns>
        public virtual ListSlice<CurrencyAmount> ConvertToDiscrete(Currency currency, double amount, ref CurrencyAmount[] currencyAmounts)
        {
            return ConvertToDiscrete_S(currency, amount, ref currencyAmounts);
        }

        /// <summary>
        /// Converts only the amounts that are full numbers, does not care about the fractional amounts.
        /// </summary>
        /// <param name="currency">The currency.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="currencyAmounts">The array that will store the result.</param>
        /// <returns>The array struct containing the resulting array.</returns>
        public virtual ListSlice<CurrencyAmount> ConvertOverflow(Currency currency, double amount, ref CurrencyAmount[] currencyAmounts)
        {
            return ConvertOverflow_S(currency, amount, ref currencyAmounts);
        }

        /// <summary>
        /// Converts only the fractional amounts, does not take into account full number amounts.
        /// </summary>
        /// <param name="currency">The currency.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="currencyAmounts">The array that will store the result.</param>
        /// <returns>The array struct containing the resulting array.</returns>
        public virtual ListSlice<CurrencyAmount> ConvertFraction(Currency currency, double amount, ref CurrencyAmount[] currencyAmounts)
        {
            return ConvertFraction_S(currency, amount, ref currencyAmounts);
        }

        #endregion

        /// <summary>
        /// Currency amounts to string.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return CurrencyAmountsToString(m_CurrencyAmounts);
        }

        public static implicit operator ListSlice<CurrencyAmount>(CurrencyCollection x) => x.m_CurrencyAmounts;
    }
}

