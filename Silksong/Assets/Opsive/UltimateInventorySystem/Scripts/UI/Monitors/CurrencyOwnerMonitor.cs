/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Monitors
{
    using Opsive.Shared.Events;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Exchange;
    using Opsive.UltimateInventorySystem.UI.Currency;
    using UnityEngine;

    /// <summary>
    /// A monitor component that listens to a currencyOwner events.
    /// </summary>
    public class CurrencyOwnerMonitor : MonoBehaviour
    {
        [Tooltip("The inventory ID to monitor, does not monitor if zero.")]
        [SerializeField] protected uint m_InventoryID = 1;
        [Tooltip("The currency owner.")]
        [SerializeField] protected CurrencyOwner m_CurrencyOwner;

        [Tooltip("The currency UI.")]
        [SerializeField] protected MultiCurrencyView m_MultiCurrencyView;

        public CurrencyOwner CurrencyOwnerCurrency {
            get => m_CurrencyOwner;
            internal set => m_CurrencyOwner = value;
        }

        /// <summary>
        /// Initialize the component.
        /// </summary>
        private void Start()
        {
            Initialize();
        }

        /// <summary>
        /// Initialize the event listener.
        /// </summary>
        protected virtual void Initialize()
        {
            if (m_CurrencyOwner == null) {
                if (m_InventoryID != 0) {
                    if (InventorySystemManager.InventoryIdentifierRegister.TryGetValue(m_InventoryID,
                        out var inventoryIdentifier)) {
                        m_CurrencyOwner = inventoryIdentifier.CurrencyOwner;
                    }
                }

                if (m_CurrencyOwner == null) {
                    Debug.LogWarning("The Currency Owner Monitor cannot find an  Currency Owner reference.", this);
                    return;
                }
            }

            if (m_MultiCurrencyView == null) { m_MultiCurrencyView = GetComponent<MultiCurrencyView>(); }

            EventHandler.RegisterEvent(m_CurrencyOwner, EventNames.c_CurrencyOwner_OnUpdate, () => CurrencyUpdate(m_CurrencyOwner));
            CurrencyUpdate(m_CurrencyOwner);
        }

        /// <summary>
        /// Remove an event listener.
        /// </summary>
        protected void RemoveEvent()
        {
            if (m_CurrencyOwner == null) { return; }

            EventHandler.UnregisterEvent(m_CurrencyOwner, EventNames.c_CurrencyOwner_OnUpdate, () => CurrencyUpdate(m_CurrencyOwner));
        }

        /// <summary>
        /// Update the view when the currency changes.
        /// </summary>
        /// <param name="currencyOwner">The currency owner.</param>
        protected void CurrencyUpdate(CurrencyOwnerBase currencyOwner)
        {
            var currencyOwnerCurrencyAmounts = currencyOwner as CurrencyOwner;

            if (currencyOwnerCurrencyAmounts == null) { return; }

            m_MultiCurrencyView.DrawCurrency(currencyOwnerCurrencyAmounts.CurrencyAmount);
        }

        /// <summary>
        /// Set the currency owner.
        /// </summary>
        /// <param name="currencyOwner">The currency owner.</param>
        public void SetCurrencyOwner(CurrencyOwner currencyOwner)
        {
            if (m_CurrencyOwner == currencyOwner) { return; }
            RemoveEvent();
            m_CurrencyOwner = currencyOwner;
            Initialize();
        }
    }
}