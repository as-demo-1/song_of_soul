/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.SaveSystem
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Exchange;
    using UnityEngine;

    /// <summary>
    /// A saver component used to save the contents of a currency owner.
    /// </summary>
    public class CurrencyOwnerSaver : SaverBase
    {
        /// <summary>
        /// The currency owner save data.
        /// </summary>
        [System.Serializable]
        public struct CurrencyOwnerSaveData
        {
            public IDAmountSaveData[] CurrencyAmounts;
        }

        [Tooltip("The currency owner.")]
        [SerializeField] protected CurrencyOwner m_CurrencyOwner;
        [Tooltip("Is the save data added to the loadout of does it overwrite it.")]
        [SerializeField] protected bool m_Additive;

        /// <summary>
        /// Initialize the component.
        /// </summary>
        protected override void Awake()
        {
            if (m_CurrencyOwner == null) { m_CurrencyOwner = GetComponent<CurrencyOwner>(); }
            base.Awake();
        }

        /// <summary>
        /// Serialize the save data.
        /// </summary>
        /// <returns>The serialized data.</returns>
        public override Serialization SerializeSaveData()
        {
            if (m_CurrencyOwner == null) { return null; }

            var currencyAmountCollection = m_CurrencyOwner.CurrencyAmount;
            var currencyAmounts = new IDAmountSaveData[currencyAmountCollection.GetCurrencyAmountCount()];
            for (int i = 0; i < currencyAmounts.Length; i++) {
                var currencyAmount = currencyAmountCollection.GetCurrencyAmountAt(i);
                currencyAmounts[i] = new IDAmountSaveData(currencyAmount.Currency.ID, currencyAmount.Amount);
            }

            var saveData = new CurrencyOwnerSaveData() {
                CurrencyAmounts = currencyAmounts
            };

            return Serialization.Serialize(saveData);
        }

        /// <summary>
        /// Deserialize and load the save data.
        /// </summary>
        /// <param name="serializedSaveData">The serialized save data.</param>
        public override void DeserializeAndLoadSaveData(Serialization serializedSaveData)
        {
            if (m_CurrencyOwner == null) { return; }

            if (!m_Additive) {
                m_CurrencyOwner.CurrencyAmount.RemoveAll();
            }

            var savedData = serializedSaveData.DeserializeFields(MemberVisibility.All) as CurrencyOwnerSaveData?;

            if (savedData.HasValue == false) {
                return;
            }

            var currencySaveData = savedData.Value;

            var currencyAmounts = new CurrencyAmount[currencySaveData.CurrencyAmounts.Length];
            for (int i = 0; i < currencyAmounts.Length; i++) {
                var currencyIDAmounts = currencySaveData.CurrencyAmounts[i];

                if (InventorySystemManager.CurrencyRegister.TryGetValue(currencyIDAmounts.ID, out var currency) == false) {
                    Debug.LogWarning($"Saved Currency ID {currencyIDAmounts.ID} could not be retrieved from the Inventory System Manager.");
                    continue;
                }
                currencyAmounts[i] = new CurrencyAmount(currency, currencyIDAmounts.Amount);
            }
            m_CurrencyOwner.CurrencyAmount.AddCurrency(currencyAmounts);
        }
    }
}