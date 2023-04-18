/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Add or Remove Items using a debug view with buttons and text fields.
    /// </summary>
    public class AdjustItemAmountInputField : MonoBehaviour
    {
        [Tooltip("The Character Inventory ID.")]
        [SerializeField] protected uint m_CharacterInventoryID = 1;
#if TEXTMESH_PRO_PRESENT
        [Tooltip("The item name field, for the item to add or remove.")]
        [SerializeField] protected TMPro.TMP_InputField m_ItemNameField;
        [Tooltip("The amount of item to add or remove.")]
        [SerializeField] protected TMPro.TMP_InputField m_ItemAmountField;
#else
        [Tooltip("The item name field, for the item to add or remove.")]
        [SerializeField] protected InputField m_ItemNameField;
        [Tooltip("The amount of item to add or remove.")]
        [SerializeField] protected InputField m_ItemAmountField;
#endif
        [Tooltip("The button to add items (Optional).")]
        [SerializeField] protected Button m_AddButton;
        [Tooltip("The amount to remove items (Optional).")]
        [SerializeField] protected Button m_Remove;
        [Tooltip("The amount to add/remove items (Optional).")]
        [SerializeField] protected Button m_AdjustButton;

        private Inventory m_CharacterInventory;
    
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
            m_CharacterInventory = inventoryIdentifier.Inventory;

            if (m_ItemAmountField != null) {
#if TEXTMESH_PRO_PRESENT
                m_ItemAmountField.characterValidation = TMPro.TMP_InputField.CharacterValidation.Integer;
#else
                m_ItemAmountField.characterValidation = InputField.CharacterValidation.Integer;
#endif
            }

            m_AddButton?.onClick.AddListener(AddItem);
            m_Remove?.onClick.AddListener(RemoveItem);
            m_AdjustButton?.onClick.AddListener(AdjustItem);
        }

        /// <summary>
        /// Get the amount from the text input field.
        /// </summary>
        /// <returns>The amount.</returns>
        private int GetAmount()
        {
            var amount = 1;
            if (int.TryParse(m_ItemAmountField.text, out var value)) {
                amount = value;
            }

            return amount;
        }

        /// <summary>
        /// Add the item.
        /// </summary>
        public void AddItem()
        {
            var amount = GetAmount();

            AddItem(m_ItemNameField.text, amount);
        }

        /// <summary>
        /// Add the item.
        /// </summary>
        /// <param name="itemName">The item name to add.</param>
        /// <param name="amount">The amount to add.</param>
        public void AddItem(string itemName, int amount)
        {
            m_CharacterInventory.AddItem(itemName, amount);
        }

        /// <summary>
        /// Remove the item.
        /// </summary>
        public void RemoveItem()
        {
            var amount = GetAmount();

            RemoveItem(m_ItemNameField.text, amount);
        }

        /// <summary>
        /// Remove the item.
        /// </summary>
        /// <param name="itemName">The item name to remove.</param>
        /// <param name="amount">The amount to remove.</param>
        public void RemoveItem(string itemName, int amount)
        {
            m_CharacterInventory.RemoveItem(itemName, amount);
        }

        /// <summary>
        /// Add or remove items.
        /// </summary>
        public void AdjustItem()
        {
            var amount = GetAmount();
            
            AdjustItem(m_ItemNameField.text, amount);
        }

        /// <summary>
        /// Add or remove items.
        /// </summary>
        /// <param name="itemName">The item name to add/remove.</param>
        /// <param name="amount">The amount to add/remove.</param>
        public void AdjustItem(string itemName, int amount)
        {
            if (amount < 0) {
                RemoveItem(itemName, -amount);
            } else {
                AddItem(itemName, amount);
            }
        }
    }
}