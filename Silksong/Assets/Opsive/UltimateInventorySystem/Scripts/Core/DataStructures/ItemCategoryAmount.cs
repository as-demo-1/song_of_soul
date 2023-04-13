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
    /// Amount of itemCategories.
    /// </summary>
    [Serializable]
    public struct ItemCategoryAmount : IEquatable<ItemCategoryAmount>, IObjectAmount<ItemCategory>
    {
        [Tooltip("The amount.")]
        [SerializeField] private int m_Amount;
        [Tooltip("The item category.")]
        [SerializeField] private ItemCategory m_ItemCategory;

        public int Amount => m_Amount;
        public ItemCategory ItemCategory => m_ItemCategory;
        public ItemCategory Object => m_ItemCategory;

        /// <summary>
        /// The item category amount constructor.
        /// </summary>
        /// <param name="itemCategory">The item category.</param>
        /// <param name="amount">The amount.</param>
        public ItemCategoryAmount(ItemCategory itemCategory, int amount)
        {
            m_Amount = amount;
            m_ItemCategory = itemCategory;
        }
        
        /// <summary>
        /// The item category amount constructor.
        /// </summary>
        /// <param name="itemCategory">The item category.</param>
        /// <param name="amount">The amount.</param>
        public ItemCategoryAmount(int amount, ItemCategory itemCategory)
        {
            m_Amount = amount;
            m_ItemCategory = itemCategory;
        }

        public static implicit operator ItemCategoryAmount((int, ItemCategory) x)
            => new ItemCategoryAmount(x.Item2, x.Item1);
        public static implicit operator ItemCategoryAmount((ItemCategory, int) x)
            => new ItemCategoryAmount(x.Item1, x.Item2);

        /// <summary>
        /// Returns a readable format for an item info.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return string.Format("{0} {1}", m_Amount, m_ItemCategory);
        }

        /// <summary>
        /// Do an Equivalence check.
        /// </summary>
        /// <param name="lhs">Left hand side object.</param>
        /// <param name="rhs">Right hand side object.</param>
        /// <returns>True if equivalent.</returns>
        public static bool operator ==(ItemCategoryAmount lhs, ItemCategoryAmount rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Do an Equivalence check.
        /// </summary>
        /// <param name="lhs">Left hand side object.</param>
        /// <param name="rhs">Right hand side object.</param>
        /// <returns>True if not equivalent.</returns>
        public static bool operator !=(ItemCategoryAmount lhs, ItemCategoryAmount rhs)
        {
            return !lhs.Equals(rhs);
        }

        /// <summary>
        /// Checks if the objects are equivalent, by comparing values and parameters.
        /// </summary>
        /// <param name="other">The other object.</param>
        /// <returns>Returns true if equivalent.</returns>
        public bool Equals(ItemCategoryAmount other)
        {
            return m_Amount == other.m_Amount && Equals(m_ItemCategory, other.m_ItemCategory);
        }

        /// <summary>
        /// Checks if the object are equivalent.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>Returns true if equivalent.</returns>
        public override bool Equals(object obj)
        {
            return obj is ItemCategoryAmount other && Equals(other);
        }

        /// <summary>
        /// Returns the hashcode for the object, computed from the value and parameters.
        /// </summary>
        /// <returns>The computed hashcode.</returns>
        public override int GetHashCode()
        {
            unchecked { return (m_Amount * 397) ^ (m_ItemCategory != null ? m_ItemCategory.GetHashCode() : 0); }
        }
    }
}