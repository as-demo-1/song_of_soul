/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.UI.CompoundElements;
    using UnityEngine;
    using Text = Opsive.Shared.UI.Text;

    /// <summary>
    /// An Item View component that compares the item amount with an amount from an inventory.
    /// </summary>
    public class InventoryAmountItemView : ItemViewModule, IInventoryDependent
    {
        [Tooltip("The amount text.")]
        [SerializeField] protected Text m_AmountText;
        [Tooltip("The ID of the Inventory to get the amount from.")]
        [SerializeField] protected uint m_InventoryID = 1;
        [Tooltip("Use the Inventory ID to get the Inventory or get it from the Item Info.")]
        [SerializeField] protected bool m_UseInventoryID = false;

        public Inventory Inventory { get; set; }

        /// <summary>
        /// Set the value.
        /// </summary>
        /// <param name="info">The item info.</param>
        public override void SetValue(ItemInfo info)
        {
            if (Inventory == null) {
                if (m_UseInventoryID) {
                    Inventory = InventorySystemManager.GetInventoryIdentifier(m_InventoryID)?.Inventory;
                } else {
                    Inventory = info.Inventory as Inventory;
                }
                
            }
            if (Inventory == null) {
                Debug.LogWarning("Inventory is missing from component.", gameObject);
                Clear();
                return;
            }

            var inventoryAmount = Inventory.GetItemAmount(info.Item.ItemDefinition, false, false);

            m_AmountText.text = $"{inventoryAmount}";
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