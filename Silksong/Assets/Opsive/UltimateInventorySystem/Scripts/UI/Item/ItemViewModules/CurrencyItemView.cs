/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Exchange;
    using Opsive.UltimateInventorySystem.UI.Currency;
    using Opsive.UltimateInventorySystem.UI.Views;
    using UnityEngine;

    /// <summary>
    /// An Item View UI component that shows currency.
    /// </summary>
    public class CurrencyItemView : ItemViewModule
    {

        [Tooltip("The currency UI.")]
        [SerializeField] protected MultiCurrencyView m_MultiCurrencyView;
        [Tooltip("The currency attribute name.")]
        [SerializeField] protected string m_CurrencyAttributeName;

        public MultiCurrencyView MultiCurrencyView => m_MultiCurrencyView;

        /// <summary>
        /// Initialize the currency Item View.
        /// </summary>
        /// <param name="view">The view for this module.</param>
        public override void Initialize(View view)
        {
            base.Initialize(view);

            if (m_MultiCurrencyView == null) {
                Debug.LogError("Multi Currency View is missing on Item View", gameObject);
            }
        }

        /// <summary>
        /// Set the value.
        /// </summary>
        /// <param name="info">The item info.</param>
        public override void SetValue(ItemInfo info)
        {
            if (info.Item == null) {
                Clear();
                return;
            }

            if (info.Item.TryGetAttributeValue(m_CurrencyAttributeName,
                out CurrencyAmounts currencyAttributeValue)) {

                var pooledCurrencyCollection = GenericObjectPool.Get<CurrencyCollection>();
                pooledCurrencyCollection.SetCurrency(currencyAttributeValue);

                m_MultiCurrencyView.DrawCurrency(pooledCurrencyCollection);

                GenericObjectPool.Return(pooledCurrencyCollection);
                return;
            }

            Clear();
        }

        /// <summary>
        /// Clear the value.
        /// </summary>
        public override void Clear()
        {
            m_MultiCurrencyView.DrawEmptyCurrency();
        }
    }
}