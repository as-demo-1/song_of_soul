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
    /// Specifies the amount of a given Item Definition.
    /// </summary>
    [Serializable]
    public struct ItemDefinitionAmount : IEquatable<ItemDefinitionAmount>, IObjectAmount<ItemDefinition>
    {
        [Tooltip("The amount.")]
        [SerializeField] private int m_Amount;
        [Tooltip("The item definition.")]
        [SerializeField] private ItemDefinition m_ItemDefinition;

        public int Amount => m_Amount;
        public ItemDefinition ItemDefinition => m_ItemDefinition;
        public ItemDefinition Object => m_ItemDefinition;

        /// <summary>
        /// The item definition amount constructor.
        /// </summary>
        /// <param name="itemDefinition">The item definition.</param>
        /// <param name="amount">The amount.</param>
        public ItemDefinitionAmount(ItemDefinition itemDefinition, int amount)
        {
            m_Amount = amount;
            m_ItemDefinition = itemDefinition;
        }
        
        /// <summary>
        /// The item definition amount constructor.
        /// </summary>
        /// <param name="itemDefinition">The item definition.</param>
        /// <param name="amount">The amount.</param>
        public ItemDefinitionAmount( int amount, ItemDefinition itemDefinition)
        {
            m_Amount = amount;
            m_ItemDefinition = itemDefinition;
        }

        public static implicit operator ItemDefinitionAmount((int, ItemDefinition) x)
            => new ItemDefinitionAmount(x.Item2, x.Item1);
        public static implicit operator ItemDefinitionAmount((ItemDefinition, int) x)
            => new ItemDefinitionAmount(x.Item1, x.Item2);

        /// <summary>
        /// Returns a readable format for the object.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return string.Format("{0} {1}", m_Amount, m_ItemDefinition);
        }

        /// <summary>
        /// Do an Equivalence check.
        /// </summary>
        /// <param name="lhs">Left hand side object.</param>
        /// <param name="rhs">Right hand side object.</param>
        /// <returns>True if equivalent.</returns>
        public static bool operator ==(ItemDefinitionAmount lhs, ItemDefinitionAmount rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Do an Equivalence check.
        /// </summary>
        /// <param name="lhs">Left hand side object.</param>
        /// <param name="rhs">Right hand side object.</param>
        /// <returns>True if not equivalent.</returns>
        public static bool operator !=(ItemDefinitionAmount lhs, ItemDefinitionAmount rhs)
        {
            return !lhs.Equals(rhs);
        }

        /// <summary>
        /// Checks if the objects are equivalent, by comparing values and parameters.
        /// </summary>
        /// <param name="other">The other object.</param>
        /// <returns>Returns true if equivalent.</returns>
        public bool Equals(ItemDefinitionAmount other)
        {
            return m_Amount == other.m_Amount && Equals(m_ItemDefinition, other.m_ItemDefinition);
        }

        /// <summary>
        /// Checks if the object are equivalent.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>Returns true if equivalent.</returns>
        public override bool Equals(object obj)
        {
            return obj is ItemDefinitionAmount other && Equals(other);
        }

        /// <summary>
        /// Returns the hashcode for the object, computed from the value and parameters.
        /// </summary>
        /// <returns>The computed hashcode.</returns>
        public override int GetHashCode()
        {
            unchecked { return (m_Amount * 397) ^ (m_ItemDefinition != null ? m_ItemDefinition.GetHashCode() : 0); }
        }
    }
}