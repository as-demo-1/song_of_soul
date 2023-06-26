/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.DataStructures
{
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using System;
    using UnityEngine;

    /// <summary>
    /// Specifies the origin of an attribute.
    /// </summary>
    public struct AttributeInfo : IEquatable<AttributeInfo>
    {
        [Tooltip("The attribute.")]
        private AttributeBase m_Attribute;
        [Tooltip("The item info from the attribute.")]
        private ItemInfo m_ItemInfo;

        public AttributeBase Attribute => m_Attribute;
        public ItemInfo ItemInfo => m_ItemInfo;

        /// <summary>
        /// AttributeInfo constructor.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="itemInfo">The item info containing the attribute.</param>
        public AttributeInfo(AttributeBase attribute, ItemInfo itemInfo)
        {
            m_Attribute = attribute;
            m_ItemInfo = itemInfo;
        }

        /// <summary>
        /// AttributeInfo constructor.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        public AttributeInfo(AttributeBase attribute)
        {
            m_Attribute = attribute;
            m_ItemInfo = new ItemInfo();
        }

        public static implicit operator AttributeInfo((AttributeBase, ItemInfo) x)
            => new AttributeInfo(x.Item1, x.Item2);

        public static explicit operator AttributeInfo(AttributeBase x)
            => new AttributeInfo(x);

        /// Returns a readable format for an Attribute Info.
        /// </summary>
        /// <returns>The readable format for an Attribute Info.</returns>
        public override string ToString()
        {
            return string.Format("{0} {1}", m_Attribute, m_ItemInfo);
        }

        /// <summary>
        /// Do an Equivalence check.
        /// </summary>
        /// <param name="lhs">Left hand side object.</param>
        /// <param name="rhs">Right hand side object.</param>
        /// <returns>True if equivalent.</returns>
        public static bool operator ==(AttributeInfo lhs, AttributeInfo rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Do an Equivalence check.
        /// </summary>
        /// <param name="lhs">Left hand side object.</param>
        /// <param name="rhs">Right hand side object.</param>
        /// <returns>True if not equivalent.</returns>
        public static bool operator !=(AttributeInfo lhs, AttributeInfo rhs)
        {
            return !lhs.Equals(rhs);
        }

        /// <summary>
        /// Checks if the objects are equivalent, by comparing values and parameters.
        /// </summary>
        /// <param name="other">The other object.</param>
        /// <returns>Returns true if equivalent.</returns>
        public bool Equals(AttributeInfo other)
        {
            return m_ItemInfo == other.m_ItemInfo
                   && Equals(m_Attribute, other.m_Attribute);
        }

        /// <summary>
        /// Checks if the object are equivalent.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>Returns true if equivalent.</returns>
        public override bool Equals(object obj)
        {
            return obj is AttributeInfo other && Equals(other);
        }

        /// <summary>
        /// Returns the hashcode for the object, computed from the value and parameters.
        /// </summary>
        /// <returns>The computed hashcode.</returns>
        public override int GetHashCode()
        {
            unchecked { return m_ItemInfo.GetHashCode() + m_Attribute?.GetHashCode() ?? 0; }
        }
    }
}