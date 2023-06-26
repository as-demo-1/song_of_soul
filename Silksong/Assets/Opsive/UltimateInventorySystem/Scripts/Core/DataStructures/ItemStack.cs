/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.DataStructures
{
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using UnityEngine;

    /// <summary>
    /// The item stack class used to keep reference to an amount of items in a collection.
    /// </summary>
    public class ItemStack
    {
        [Tooltip("The amount.")]
        protected int m_Amount;
        [Tooltip("The item.")]
        protected Item m_Item;
        [Tooltip("The itemCollection.")]
        protected ItemCollection m_ItemCollection;

        protected bool m_IsInitialized = false;

        public int Amount => m_Amount;
        public Item Item => m_Item;
        public ItemAmount ItemAmount => (m_Item, Amount);
        public ItemCollection ItemCollection => m_ItemCollection;
        public IInventory Inventory => m_ItemCollection?.Inventory;
        public bool IsInitialized => m_IsInitialized;

        /// <summary>
        /// Initialize the item stack.
        /// </summary>
        /// <param name="itemAmount">The item amount.</param>
        /// <param name="itemCollection">The item collection.</param>
        internal virtual void Initialize(ItemAmount itemAmount, ItemCollection itemCollection)
        {
            if (m_IsInitialized) {
                Debug.LogError("An Item Stack should never be initialized without being Reset first. Make sure the Item Stack is reset before it is returned to the Generic Object Pool.");
                return;
            }
            m_Item = itemAmount.Item;
            m_Amount = itemAmount.Amount;
            m_ItemCollection = itemCollection;
            m_IsInitialized = true;
        }

        /// <summary>
        /// Set the amount in the stack.
        /// </summary>
        /// <param name="amount">The amount.</param>
        internal void SetAmount(int amount)
        {
            m_Amount = amount;
        }

        /// <summary>
        /// Reset the item stack to be used again later.
        /// </summary>
        internal void Reset()
        {
            m_Item = null;
            m_Amount = 0;
            m_ItemCollection = null;
            m_IsInitialized = false;
        }

        /// <summary>
        /// Overrides the ToString method.
        /// </summary>
        /// <returns>The overridden string.</returns>
        public override string ToString()
        {
            var itemCollection = m_ItemCollection == null ? "ItemCollection is NULL" : m_ItemCollection.ToString();
            return string.Format("ItemStack({0})[ {1} in {2}]", GetHashCode(), ItemAmount, itemCollection);
        }
    }
}