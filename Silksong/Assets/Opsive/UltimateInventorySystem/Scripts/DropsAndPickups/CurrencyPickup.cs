/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.DropsAndPickups
{
    using Opsive.UltimateInventorySystem.Exchange;
    using Opsive.UltimateInventorySystem.Interactions;
    using UnityEngine;

    /// <summary>
    /// A currency pickup component.
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public class CurrencyPickup : PickupBase
    {

        protected CurrencyOwner m_CurrencyOwner;

        public CurrencyOwner CurrencyOwner => m_CurrencyOwner;

        /// <summary>
        /// Initialize the component.
        /// </summary>
        protected void Awake()
        {
            m_CurrencyOwner = GetComponent<CurrencyOwner>();
        }

        /// <summary>
        /// Add currency to the interactor.
        /// </summary>
        /// <param name="interactor">The interactor.</param>
        protected override void OnInteractInternal(IInteractor interactor)
        {
            if (!(interactor is IInteractorWithInventory interactorWithInventory)) { return; }

            var interactorCurrencyOwner = interactorWithInventory.Inventory.GetCurrencyComponent<CurrencyCollection>();

            interactorCurrencyOwner.AddCurrency(m_CurrencyOwner.CurrencyAmount);
            NotifyPickupSuccess();
        }
    }
}