/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.DropsAndPickups
{
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.Exchange;
    using UnityEngine;

    /// <summary>
    /// Currency dropper, spawns a currency pickup.
    /// </summary>
    public class CurrencyDropper : Dropper
    {
        [Tooltip("The currency owner.")]
        [SerializeField] protected CurrencyOwner m_CurrencyOwner;

        protected CurrencyPickup m_CurrencyPickupPrefabComponent;

        /// <summary>
        /// Initialize the component.
        /// </summary>
        protected virtual void Awake()
        {
            m_CurrencyPickupPrefabComponent = m_PickUpPrefab?.GetComponent<CurrencyPickup>();
            if (m_CurrencyPickupPrefabComponent == null) {
                Debug.LogWarning("The Currency Pickup component does not exist.");
            }
        }

        /// <summary>
        /// Drop currency.
        /// </summary>
        public override void Drop()
        {
            var currencyPickup = ObjectPool.Instantiate(m_PickUpPrefab,
                m_DropTransform.position + DropOffset(),
                Quaternion.identity).GetComponent<CurrencyPickup>();

            currencyPickup.CurrencyOwner.SetCurrency(m_CurrencyOwner.CurrencyAmount);
        }
    }
}