/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Exchange
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Utility;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The currency class.
    /// </summary>
    public class Currency : ScriptableObject, IObjectWithID
    {
        [Tooltip("The ID of the category.")]
        [SerializeField] protected uint m_ID;
        [Tooltip("The parent of this currency, used to compute an exchange rate.")]
        [SerializeField] protected Currency m_Parent;
        [Tooltip("The Exchange rate from this currency to its parent. It must be higher than 1. The parent is always less valuable.")]
        [SerializeField] protected double m_ExchangeRateToParent;
        [Tooltip("The serialization data for the direct children of this currency, used to search the descendants without going through the entire database.")]
        [SerializeField] protected Serialization m_ChildrenData;
        [Tooltip("The maxAmount can be used to limit the amount of currency in a currencyCollection. It can be ignored if not needed.")]
        [SerializeField] protected int m_MaxAmount;
        [Tooltip("If specified the currency will overflow to this one when the amount is greater than max amount.")]
        [SerializeField] protected Currency m_OverflowCurrency;
        [Tooltip("If specified the currency fraction remainder will be converted to it.")]
        [SerializeField] protected Currency m_FractionCurrency;
        [Tooltip("An Icon used to display the currency in a UI.")]
        [SerializeField] protected Sprite m_Icon;

        [System.NonSerialized] protected bool m_Initialized = false;

        [System.NonSerialized] protected ResizableArray<Currency> m_Children;

        protected bool m_Dirty;
        internal bool Dirty {
            get => m_Dirty;
            set => m_Dirty = value;
        }

        public uint ID {
            get => m_ID;
            internal set {
                if (m_ID == value) { return; }
                m_ID = value;
                m_Dirty = true;
            }
        }

        uint IObjectWithID.ID {
            get => ID;
            set {
                if (m_ID == value) { return; }
                m_ID = value;
                m_Dirty = true;
            }
        }

        [System.NonSerialized] protected IInventorySystemManager m_Manager;

        public IInventorySystemManager Manager => m_Manager;

        public Currency Parent => m_Parent;
        public double ExchangeRateToParent => m_ExchangeRateToParent;
        public int MaxAmount => m_MaxAmount;
        public Currency OverflowCurrency => m_OverflowCurrency;
        public Currency FractionCurrency => m_FractionCurrency;
        public Sprite Icon => m_Icon;
        public bool IsInitialized => m_Initialized;

        public IReadOnlyList<Currency> ChildrenReadOnly => m_Children;

        internal ResizableArray<Currency> Children => m_Children;

        /// <summary>
        /// Creates an ItemDefinition using a category.
        /// </summary>
        /// <param name="name">The name of the currency.</param>
        /// <param name="parent">The parent currency, if there is one.</param>
        /// <param name="exchangeRateToParent">The exchange rate to the parent currency provided.</param>
        /// <param name="manager">The inventory system manager where the definition will be registered to.</param>
        /// <returns>The created currency.</returns>
        internal static Currency Create(string name, Currency parent = null, double exchangeRateToParent = 1, IInventorySystemManager manager = null)
        {
            //construct
            var createdCurrency = CreateInstance<Currency>();

            createdCurrency.m_Manager = manager;
            createdCurrency.ID = RandomID.Generate();
            createdCurrency.name = name;
            createdCurrency.m_ExchangeRateToParent = exchangeRateToParent;
            createdCurrency.m_MaxAmount = int.MaxValue;

            if (parent != null) {
                createdCurrency.SetParent(parent);
            }

            //Register
            var result = createdCurrency.Initialize(true);

            if (result == false) { return null; }

            return createdCurrency;
        }

        /// <summary>
        /// Initializes the currency.
        /// </summary>
        /// <returns>True if initialized correctly.</returns>
        public virtual bool Initialize(bool force)
        {
            if (m_Initialized && !force) { return true; }
            Deserialize();

            // It is required to check .Equals as the interface could point to a unity Object.
            if (InterfaceUtility.IsNotNull(m_Manager)) {
                var success = m_Manager.Register?.CurrencyRegister?.Register(this);
                if (success.HasValue) { return success.Value; }
            }

#if UNITY_EDITOR
            if (!Application.isPlaying) {
                m_Initialized = true;
                return true;
            }
#endif
            m_Initialized = InventorySystemManager.CurrencyRegister.Register(this);
            return m_Initialized;
        }

        /// <summary>
        /// Create the ArrayStruct for the children.
        /// </summary>
        internal void Deserialize()
        {
            if (m_ChildrenData != null && m_ChildrenData.Values != null && m_ChildrenData.Values.Length > 0) {
                m_Children = (ResizableArray<Currency>)m_ChildrenData.DeserializeFields(MemberVisibility.Public);
            }

            if (m_Children == null || m_Children.Array == null) {
                m_Children = new ResizableArray<Currency>();
            }
        }

        /// <summary>
        /// Serializes the relationship ArrayStructs to Serialization data.
        /// </summary>
        public void Serialize()
        {
            m_Children.Truncate();
            m_ChildrenData = Serialization.Serialize(m_Children);
        }

        /// <summary>
        /// Try to get the exchange rate to another currency as in "1 this = exchangeRate otherCurrency" (can be found inherently).
        /// </summary>
        /// <param name="otherCurrency">The other currency.</param>
        /// <param name="exchangeRate">Output of the exchangeRate (-1 if it does not exist)</param>
        /// <returns>True if the exchange rate exists.</returns>
        public bool TryGetExchangeRateTo(Currency otherCurrency, out double exchangeRate)
        {
            if (otherCurrency == false) {
                exchangeRate = -1;
                return false;
            }
            if (otherCurrency == this) {
                exchangeRate = 1;
                return true;
            }
            var rootExchangeRate = GetRootExchangeRate();
            var otherRootExchangeRate = otherCurrency.GetRootExchangeRate();
            if (rootExchangeRate.Currency != otherRootExchangeRate.Currency) {
                exchangeRate = -1;
                return false;
            }

            exchangeRate = rootExchangeRate.ExchangeRate / otherRootExchangeRate.ExchangeRate;
            return true;
        }

        /// <summary>
        /// Is the currency related to the same root currency.
        /// </summary>
        /// <param name="otherCurrency">The other currency.</param>
        /// <returns>True if they have the same root.</returns>
        public bool IsCurrencyPartOfFamily(Currency otherCurrency)
        {
            return GetRoot() == otherCurrency.GetRoot();
        }

        /// <summary>
        /// Get the root currency.
        /// </summary>
        /// <returns>The root.</returns>
        public Currency GetRoot()
        {
            return m_Parent != null ? m_Parent.GetRoot() : this;
        }

        /// <summary>
        /// Get the exchange rate to this currency root.
        /// </summary>
        /// <param name="additionalExchangeRate">External exchange rate for recursive pattern.</param>
        /// <returns>The exchange rate to the root.</returns>
        public CurrencyExchangeRate GetRootExchangeRate(double additionalExchangeRate = 1)
        {
            if (m_Parent != null) {
                return m_Parent.GetRootExchangeRate(additionalExchangeRate * m_ExchangeRateToParent);
            }

            return new CurrencyExchangeRate(this, additionalExchangeRate);
        }

        /// <summary>
        /// Set a new parent currency with a new exchange rate.
        /// </summary>
        /// <param name="newParentExchangeRate">The new currency exchange rate.</param>
        /// <returns>Fail message if the new currencyExchange rate is invalid.</returns>
        public virtual bool SetParentCurrencyExchangeRate(CurrencyExchangeRate newParentExchangeRate)
        {
            return SetParentCurrencyExchangeRate(newParentExchangeRate.ExchangeRate, newParentExchangeRate.Currency);
        }

        /// <summary>
        /// Set a new parent currency with a new exchange rate.
        /// </summary>
        /// <param name="rate">The new exchange rate.</param>
        /// <param name="newParent">The new parent currency.</param>
        /// <returns>Fail message if the new currencyExchange rate is invalid.</returns>
        public virtual bool SetParentCurrencyExchangeRate(double rate, Currency newParent)
        {
            var result = SetParent(newParent);
            if (!result) { return false; }

            result = SetParentExchangeRate(rate);
            if (!result) { return false; }

            return true;
        }

        /// <summary>
        /// Set a new parent currency with a new exchange rate.
        /// </summary>
        /// <param name="rate">The new exchange rate.</param>
        /// <returns>Fail message if the new currencyExchange rate is invalid.</returns>
        public virtual bool SetParentExchangeRateCondition(double rate)
        {
            if (rate < 1) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Set a new parent currency with a new exchange rate.
        /// </summary>
        /// <param name="rate">The new exchange rate.</param>
        /// <returns>Fail message if the new currencyExchange rate is invalid.</returns>
        public virtual bool SetParentExchangeRate(double rate)
        {
            if (SetParentExchangeRateCondition(rate) == false) {
                return false;
            }

            m_ExchangeRateToParent = rate;
            m_Dirty = true;
            return true;
        }

        /// <summary>
        /// Set a new parent currency with a new exchange rate.
        /// </summary>
        /// <param name="newParent">The new parent currency.</param>
        /// <returns>Fail message if the new currencyExchange rate is invalid.</returns>
        public virtual bool SetParentCondition(Currency newParent)
        {
            if (newParent == null) { return true; }

            if (newParent == this) {
                return false;
            }

            var currencies = GenericObjectPool.Get<Currency[]>();

            var newParentAllParentsCount = newParent.GetAllParents(ref currencies, true);
            if (currencies.Contains(this, 0, newParentAllParentsCount)) {
                GenericObjectPool.Return(currencies);
                return false;
            }
            GenericObjectPool.Return(currencies);

            return true;
        }

        /// <summary>
        /// Set a new parent currency with a new exchange rate.
        /// </summary>
        /// <param name="newParent">The new parent currency.</param>
        /// <returns>Fail message if the new currencyExchange rate is invalid.</returns>
        public virtual bool SetParent(Currency newParent)
        {
            if (SetParentCondition(newParent) == false) {
                return false;
            }

            RemoveParent();

            if (newParent != null) {
                m_Parent = newParent;
                m_Parent.m_Children.Add(this);
                m_Dirty = true;
            }

            //The parent changed, so the overflow and fraction currencies could be invalid, and therefore need to be removed.
            if (SetOverflowCurrencyCondition(m_OverflowCurrency) == false) { SetOverflowCurrency(null); }
            if (SetFractionCurrencyCondition(m_FractionCurrency) == false) { SetFractionCurrency(null); }

            return true;
        }

        /// <summary>
        /// Set a new parent currency with a new exchange rate.
        /// </summary>
        /// <param name="newParent">The new parent currency.</param>
        /// <returns>Fail message if the new currencyExchange rate is invalid.</returns>
        internal void SetParentWithoutNotify(Currency newParent)
        {
            m_Parent = newParent;
        }

        /// <summary>
        /// Removes the Parent Currency.
        /// </summary>
        protected virtual void RemoveParent()
        {
            if (m_Parent == null) { return; }

            if (m_Parent.m_Children != null) {
                m_Parent.m_Children.Remove(this);
            }

            m_Dirty = true;
            m_Parent = null;
        }

        /// <summary>
        /// Set the default max amount of this currency when part of a currencyCollection.
        /// </summary>
        /// <param name="newMaxAmount">The new max amount (must be > 1).</param>
        public bool SetMaxAmountCondition(int newMaxAmount)
        {
            if (newMaxAmount < 1) { return false; }

            return true;
        }

        /// <summary>
        /// Set the default max amount of this currency when part of a currencyCollection.
        /// </summary>
        /// <param name="newMaxAmount">The new max amount (must be > 1).</param>
        public void SetMaxAmount(int newMaxAmount)
        {
            if (SetMaxAmountCondition(newMaxAmount) == false) { return; }

            m_MaxAmount = newMaxAmount;
            m_Dirty = true;
        }

        /// <summary>
        /// Set the auto convert overflow currency. If null the overflow will be lost.
        /// The overflow currency must be of the same family and have More value.
        /// </summary>
        /// <param name="newOverflowCurrency">The new overflow currency.</param>
        /// <returns>False if invalid overflow currency.</returns>
        public bool SetOverflowCurrencyCondition(Currency newOverflowCurrency)
        {
            if (newOverflowCurrency == null) {
                return true;
            }

            if (newOverflowCurrency == this) { return false; }

            var thisRootRate = GetRootExchangeRate();
            var otherRootRate = newOverflowCurrency.GetRootExchangeRate();
            if (thisRootRate.Currency != otherRootRate.Currency || thisRootRate.ExchangeRate >= otherRootRate.ExchangeRate) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Set the auto convert overflow currency. If null the overflow will be lost.
        /// The overflow currency must be of the same family and have More value.
        /// </summary>
        /// <param name="newOverflowCurrency">The new overflow currency.</param>
        /// <returns>False if invalid overflow currency.</returns>
        public bool SetOverflowCurrency(Currency newOverflowCurrency)
        {
            if (newOverflowCurrency == null) {
                m_OverflowCurrency = null;
                m_Dirty = true;
                return true;
            }

            if (SetOverflowCurrencyCondition(newOverflowCurrency) == false) { return false; }

            m_OverflowCurrency = newOverflowCurrency;
            m_Dirty = true;
            return true;
        }

        /// <summary>
        /// Set the auto convert overflow currency. If null the overflow will be lost.
        /// The overflow currency must be of the same family and have More value.
        /// </summary>
        /// <param name="newOverflowCurrency">The new overflow currency.</param>
        /// <returns>False if invalid overflow currency.</returns>
        internal void SetOverflowCurrencyWithoutNotify(Currency newOverflowCurrency)
        {
            m_OverflowCurrency = newOverflowCurrency;
        }

        /// <summary>
        /// Set the auto convert fraction currency. If null the remainder will be lost.
        /// The fraction currency must be of the same family and have LESS value. 
        /// </summary>
        /// <param name="newFractionCurrency">The new fraction currency.</param>
        /// <returns>False if the fraction currency is invalid.</returns>
        public bool SetFractionCurrencyCondition(Currency newFractionCurrency)
        {
            if (newFractionCurrency == null) {
                return true;
            }

            if (newFractionCurrency == this) { return false; }

            var thisRootRate = GetRootExchangeRate();
            var otherRootRate = newFractionCurrency.GetRootExchangeRate();
            if (thisRootRate.Currency != otherRootRate.Currency || thisRootRate.ExchangeRate <= otherRootRate.ExchangeRate) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Set the auto convert fraction currency. If null the remainder will be lost.
        /// The fraction currency must be of the same family and have LESS value. 
        /// </summary>
        /// <param name="newFractionCurrency">The new fraction currency.</param>
        /// <returns>False if the fraction currency is invalid.</returns>
        public bool SetFractionCurrency(Currency newFractionCurrency)
        {
            if (newFractionCurrency == null) {
                m_FractionCurrency = null;
                m_Dirty = true;
                return true;
            }

            if (SetFractionCurrencyCondition(newFractionCurrency) == false) { return false; }

            m_FractionCurrency = newFractionCurrency;
            m_Dirty = true;
            return true;
        }

        /// <summary>
        /// Set the auto convert fraction currency. If null the remainder will be lost.
        /// The fraction currency must be of the same family and have LESS value. 
        /// </summary>
        /// <param name="newFractionCurrency">The new fraction currency.</param>
        /// <returns>False if the fraction currency is invalid.</returns>
        internal void SetFractionCurrencyWithoutNotify(Currency newFractionCurrency)
        {
            m_FractionCurrency = newFractionCurrency;
        }

        /// <summary>
        /// Get the direct children count of this itemDefinition.
        /// </summary>
        /// <returns>The number of direct children.</returns>
        public int GetChildrenCount()
        {
            return m_Children.Count;
        }

        /// <summary>
        /// Get the children at the index specified, loop over this function for no boxing.
        /// </summary>
        /// <param name="index">The index of the child.</param>
        /// <returns>Returns the child at the specified index.</returns>
        public Currency GetChildrenAt(int index)
        {
            return m_Children[index];
        }

        /// <summary>
        /// Returns the parents of all generations.
        /// </summary>
        /// <param name="parents">Reference to an Currency array. Can be resized up.</param>
        /// <param name="includeThis">If true this Currency will be part of the result.</param>
        /// <returns>A count of the assigned elements of the array.</returns>
        public int GetAllParents(ref Currency[] parents, bool includeThis)
        {
            return IterationHelper.GetAllRecursiveDFS(ref parents, includeThis, this, x => x.m_Parent);
        }

        /// <summary>
        /// Returns the children of all generations.
        /// </summary>
        /// <param name="children">Reference to an Currency array. Can be resized up.</param>
        /// <param name="includeThis">If true this Currency will be part of the result.</param>
        /// <returns>A count of the assigned elements of the array.</returns>
        public int GetAllChildren(ref Currency[] children, bool includeThis)
        {
            return IterationHelper.GetAllRecursive(ref children, includeThis, this, x => x.m_Children, true);
        }

        /// <summary>
        /// Returns the first child.
        /// </summary>
        /// <returns>The first child currency.</returns>
        public Currency GetFirstChild()
        {
            if (m_Children.Count > 0) { return m_Children[0]; }

            return null;
        }

        /// <summary>
        /// Returns all the Currencies that are related to this one.
        /// Parents and children.
        /// </summary>
        /// <param name="family">Reference to an Currency array. Can be resized up.</param>
        /// <returns>A count of the assigned elements of the array.</returns>
        public int GetAllFamily(ref Currency[] family)
        {
            var root = GetRoot();
            return root.GetAllChildren(ref family, true);
        }

        /// <summary>
        /// Set the Icon of the currency.
        /// </summary>
        /// <param name="sprite">The sprite.</param>
        public void SetIcon(Sprite sprite)
        {
            m_Icon = sprite;
            m_Dirty = true;
        }

        /// <summary>
        /// Returns the custom string.
        /// </summary>
        /// <returns>The custom string.</returns>
        public override string ToString()
        {
            return name;
        }
    }
}

