/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.DropsAndPickups
{
    using Opsive.Shared.Game;
    using UnityEngine;

    /// <summary>
    /// Drop a random amount of currency.
    /// </summary>
    public class RandomCurrencyDropper : CurrencyDropper
    {
        [Tooltip("The minimum offset, min value for the currency multiplier.")]
        [SerializeField] protected float m_MinOffset = 0.3f;
        [Tooltip("The maximum offset, max value for the currency multiplier.")]
        [SerializeField] protected float m_MaxOffset = 3f;

        /// <summary>
        /// Drop the currency.
        /// </summary>
        public override void Drop()
        {
            var currencyPickup = ObjectPool.Instantiate(m_PickUpPrefab,
                m_DropTransform.position + DropOffset(),
                Quaternion.identity).GetComponent<CurrencyPickup>();

            currencyPickup.CurrencyOwner.CurrencyAmount.SetCurrency(
                m_CurrencyOwner.CurrencyAmount, Random.Range(m_MinOffset, m_MaxOffset));
        }
    }
}