/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.AttributeSystem
{
    using UnityEngine;

    /// <summary>
    /// Item ID is a simple struct that lets easily serialize an Item ID and get the item from it.
    /// </summary>
    [System.Serializable]
    public struct ItemID
    {
        [Tooltip("The item ID.")]
        [SerializeField] private uint m_ID;
        [System.NonSerialized] private Item m_Item;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="itemID">Item ID.</param>
        public ItemID(uint itemID)
        {
            m_ID = itemID;
            m_Item = null;
        }

        public uint ID => m_ID;

        public Item Item {
            get {
                if (m_Item == null || m_Item.ID != m_ID) {
                    InventorySystemManager.ItemRegister.TryGetValue(m_ID, out m_Item);
                }

                return m_Item;
            }
        }
    }
}