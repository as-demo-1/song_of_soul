/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.UI.CompoundElements;
    using UnityEngine;
    using Text = Opsive.Shared.UI.Text;

    /// <summary>
    /// An Item View component that compares the item amount with an amount from an inventory.
    /// </summary>
    public class AmountComparisonItemView : ItemViewModule, IInventoryDependent
    {
        [Tooltip("The amount text.")]
        [SerializeField] protected Text m_AmountText;
        [Tooltip("The text color if the inventory amount is bigger or equal to the item amount.")]
        [SerializeField] protected Color m_PositiveColor = Color.green;
        [Tooltip("The text color if the inventory amount is lower then the item amount.")]
        [SerializeField] protected Color m_NegativeColor = Color.red;

        public Inventory Inventory { get; set; }

        /// <summary>
        /// Set the value.
        /// </summary>
        /// <param name="info">The item info.</param>
        public override void SetValue(ItemInfo info)
        {
            if (Inventory == null) { Inventory = info.Inventory as Inventory; }
            if (Inventory == null) {
                Debug.LogWarning("Inventory is missing from component.");
                Clear();
                return;
            }

            var inventoryAmount = Inventory.GetItemAmount(info.Item.ItemDefinition, false, false);

            m_AmountText.text = $"{inventoryAmount}/{info.Amount}";

            m_AmountText.color = inventoryAmount - info.Amount >= 0 ? m_PositiveColor : m_NegativeColor;
        }

        /// <summary>
        /// Clear the value.
        /// </summary>
        public override void Clear()
        {
            m_AmountText.text = "";
        }
    }
}