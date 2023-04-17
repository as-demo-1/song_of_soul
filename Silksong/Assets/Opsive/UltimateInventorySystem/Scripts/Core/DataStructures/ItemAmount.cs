/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.DataStructures
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Specifies the amount of each item.
    /// </summary>
    [Serializable]
    public struct ItemAmount : IEquatable<ItemAmount>, IObjectAmount<Item>
    {
        public static ItemAmount None => new ItemAmount();
        public static ItemAmount One => new ItemAmount() { m_Amount = 1 };

        [Tooltip("The amount.")]
        [SerializeField] private int m_Amount;
        [Tooltip("The item.")]
        [SerializeField] private Item m_Item;

        public int Amount => m_Amount;
        public Item Item => m_Item;
        public Item Object => m_Item;

        /// <summary>
        /// The item amount constructor.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="amount">The amount.</param>
        public ItemAmount(Item item, int amount)
        {
            m_Amount = amount;
            m_Item = item;
        }

        /// <summary>
        /// ItemInfo constructor.
        /// </summary>
        /// <param name="itemDefinition">The itemDefinition.</param>
        /// <param name="amount">The amount.</param>
        public ItemAmount(ItemDefinition itemDefinition, int amount)
        {
            m_Amount = amount;
            m_Item = InventorySystemManager.CreateItem(itemDefinition);
        }

        /// <summary>
        /// ItemInfo constructor.
        /// </summary>
        /// <param name="itemName">The itemName.</param>
        /// <param name="amount">The amount.</param>
        public ItemAmount(string itemName, int amount)
        {
            m_Amount = amount;
            m_Item = InventorySystemManager.CreateItem(itemName);
        }
        
        /// <summary>
        /// The item amount constructor.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="amount">The amount.</param>
        public ItemAmount(int amount, Item item)
        {
            m_Amount = amount;
            m_Item = item;
        }

        /// <summary>
        /// ItemInfo constructor.
        /// </summary>
        /// <param name="itemDefinition">The itemDefinition.</param>
        /// <param name="amount">The amount.</param>
        public ItemAmount(int amount, ItemDefinition itemDefinition)
        {
            m_Amount = amount;
            m_Item = InventorySystemManager.CreateItem(itemDefinition);
        }

        /// <summary>
        /// ItemInfo constructor.
        /// </summary>
        /// <param name="itemName">The itemName.</param>
        /// <param name="amount">The amount.</param>
        public ItemAmount(int amount, string itemName)
        {
            m_Amount = amount;
            m_Item = InventorySystemManager.CreateItem(itemName);
        }

        public static implicit operator ItemAmount((int, Item) x)
            => new ItemAmount(x.Item2, x.Item1);
        public static implicit operator ItemAmount((Item, int) x)
            => new ItemAmount(x.Item1, x.Item2);

        /// <summary>
        /// Checks if the amount and the item is value equivalent to another. Equivalent and Equals is not the same!
        /// </summary>
        /// <param name="other">The other item amount.</param>
        /// <returns>True if equivalent.</returns>
        public bool ValueEquivalentTo(ItemAmount other)
        {
            return m_Amount == other.m_Amount && Item.AreValueEquivalent(m_Item, other.m_Item);
        }

        /// <summary>
        /// Checks if the amount are equal and if the items are stackable!
        /// </summary>
        /// <param name="other">The other item.</param>
        /// <returns>True if equivalent.</returns>
        public bool StackableEquivalentTo(ItemAmount other)
        {
            return m_Amount == other.m_Amount && Item.AreStackableEquivalent(m_Item, other.m_Item);
        }

        /// <summary>
        /// Returns a readable format for an item info.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            var item = m_Item == null ? "Item is NULL" : m_Item.ToString();
            return string.Format("{0} {1}", m_Amount, item);
        }

        /// <summary>
        /// Do an Equivalence check.
        /// </summary>
        /// <param name="lhs">Left hand side object.</param>
        /// <param name="rhs">Right hand side object.</param>
        /// <returns>True if equivalent.</returns>
        public static bool operator ==(ItemAmount lhs, ItemAmount rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Do an Equivalence check.
        /// </summary>
        /// <param name="lhs">Left hand side object.</param>
        /// <param name="rhs">Right hand side object.</param>
        /// <returns>True if not equivalent.</returns>
        public static bool operator !=(ItemAmount lhs, ItemAmount rhs)
        {
            return !lhs.Equals(rhs);
        }

        /// <summary>
        /// Checks if the objects are equivalent, by comparing values and parameters.
        /// </summary>
        /// <param name="other">The other object.</param>
        /// <returns>Returns true if equivalent.</returns>
        public bool Equals(ItemAmount other)
        {
            return m_Amount == other.m_Amount && Equals(m_Item, other.m_Item);
        }

        /// <summary>
        /// Checks if the object are equivalent.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>Returns true if equivalent.</returns>
        public override bool Equals(object obj)
        {
            return obj is ItemAmount other && Equals(other);
        }

        /// <summary>
        /// Returns the hashcode for the object, computed from the value and parameters.
        /// </summary>
        /// <returns>The computed hashcode.</returns>
        public override int GetHashCode()
        {
            unchecked { return (m_Amount * 397) ^ (m_Item != null ? m_Item.GetHashCode() : 0); }
        }
    }
}