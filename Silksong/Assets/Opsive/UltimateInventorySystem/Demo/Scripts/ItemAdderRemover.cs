/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Examples
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using UnityEngine;

    public class ItemAdderRemover : MonoBehaviour
    {
        [SerializeField] protected int m_Amount = 1;
        [SerializeField] protected ItemDefinition m_Definition;
        [SerializeField] protected Inventory m_Inventory;

        /// <summary>
        /// Add an item.
        /// </summary>
        [ContextMenu("Add Item")]
        public void AddItem()
        {
            var item = InventorySystemManager.CreateItem(m_Definition);
            m_Inventory.AddItem((ItemInfo)(m_Amount, item));
        }

        /// <summary>
        /// Remove the item.
        /// </summary>
        [ContextMenu("Remove Item")]
        public void RemoveItem()
        {
            var info = m_Inventory.GetItemInfo(m_Definition);

            if (info.HasValue) {
                m_Inventory.RemoveItem((m_Amount, info.Value));
            }

        }
    }
}
