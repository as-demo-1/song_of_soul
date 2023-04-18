/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Exchange;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Add or Remove Currency using a debug view with buttons and text fields.
    /// </summary>
    public class AdjustCurrencyAmountInputField : MonoBehaviour
    {
        [Tooltip("The Character Inventory ID.")]
        [SerializeField] protected uint m_CharacterInventoryID = 1;
#if TEXTMESH_PRO_PRESENT
        [Tooltip("The currency name field, for the currency to add or remove.")]
        [SerializeField] protected TMPro.TMP_InputField m_CurrencyNameField;
        [Tooltip("The amount of currency to add or remove.")]
        [SerializeField] protected TMPro.TMP_InputField m_CurrencyAmountField;
#else
        [Tooltip("The currency name field, for the currency to add or remove.")]
        [SerializeField] protected InputField m_CurrencyNameField;
        [Tooltip("The amount of currency to add or remove.")]
        [SerializeField] protected InputField m_CurrencyAmountField;
#endif
        [Tooltip("The button to add currency (Optional).")]
        [SerializeField] protected Button m_AddButton;
        [Tooltip("The amount to remove currency (Optional).")]
        [SerializeField] protected Button m_Remove;
        [Tooltip("The amount to add/remove currency (Optional).")]
        [SerializeField] protected Button m_AdjustButton;

        private CurrencyOwner m_CurrencyOwner;
    
        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        void Start()
        {
            var inventoryIdentifier = InventorySystemManager.GetInventoryIdentifier(m_CharacterInventoryID);
            if (inventoryIdentifier == null) {
                Debug.LogError("Character Inventory not found for the Debug script");
                return;
            }
            m_CurrencyOwner = inventoryIdentifier.CurrencyOwner;

            if (m_CurrencyAmountField != null) {
#if TEXTMESH_PRO_PRESENT
                m_CurrencyAmountField.characterValidation = TMPro.TMP_InputField.CharacterValidation.Integer;
#else
                m_CurrencyAmountField.characterValidation = InputField.CharacterValidation.Integer;
#endif
            }

            m_AddButton?.onClick.AddListener(AddCurrency);
            m_Remove?.onClick.AddListener(RemoveCurrency);
            m_AdjustButton?.onClick.AddListener(AdjustCurrency);
        }

        /// <summary>
        /// Get the amount from the text input field.
        /// </summary>
        /// <returns>The amount.</returns>
        private int GetAmount()
        {
            var amount = 1;
            if (int.TryParse(m_CurrencyAmountField.text, out var value)) {
                amount = value;
            }

            return amount;
        }

        /// <summary>
        /// Add the currency.
        /// </summary>
        public void AddCurrency()
        {
            var amount = GetAmount();

            AddCurrency(m_CurrencyNameField.text, amount);
        }

        /// <summary>
        /// Add the currency.
        /// </summary>
        /// <param name="currencyName">The currency name to add.</param>
        /// <param name="amount">The amount to add.</param>
        public void AddCurrency(string currencyName, int amount)
        {
            m_CurrencyOwner.AddCurrency(currencyName, amount);
        }

        /// <summary>
        /// Remove the currency.
        /// </summary>
        public void RemoveCurrency()
        {
            var amount = GetAmount();

            RemoveCurrency(m_CurrencyNameField.text, amount);
        }

        /// <summary>
        /// Remove the currency.
        /// </summary>
        /// <param name="currencyName">The currency name to remove.</param>
        /// <param name="amount">The amount to remove.</param>
        public void RemoveCurrency(string currencyName, int amount)
        {
            m_CurrencyOwner.RemoveCurrency(currencyName, amount);
        }

        /// <summary>
        /// Add or remove currency.
        /// </summary>
        public void AdjustCurrency()
        {
            var amount = GetAmount();
            
            AdjustCurrency(m_CurrencyNameField.text, amount);
        }

        /// <summary>
        /// Add or remove currency.
        /// </summary>
        /// <param name="currencyName">The currency name to add/remove.</param>
        /// <param name="amount">The amount to add/remove.</param>
        public void AdjustCurrency(string currencyName, int amount)
        {
            if (amount < 0) {
                RemoveCurrency(currencyName, -amount);
            } else {
                AddCurrency(currencyName, amount);
            }
        }
    }
}