/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.DataStructures
{
    using UnityEngine;

    /// <summary>
    /// Item Amounts is an array of item amounts.
    /// </summary>
    [System.Serializable]
    public class ItemAmountsWithCategory : ItemAmounts
    {
        [Tooltip("All items in this item amounts must be part of this category.")]
        [SerializeField] protected ItemCategory m_ItemCategory;
        [Tooltip("Does it accept item that are inherently part of the category or must they be direct?")]
        [SerializeField] protected bool m_Inherently;

        public ItemCategory ItemCategory {
            get => m_ItemCategory;
            internal set {
                if (m_ItemCategory == value) { return; }
                m_ItemCategory = value;
                Refresh();
            }
        }

        public bool Inherently {
            get => m_Inherently;
            internal set {
                if (m_Inherently == value) { return; }
                m_Inherently = value;
                Refresh();
            }
        }

        public override ItemAmount this[int index] {
            get => m_Array[index];
            set {
                if (Condition(value)) {
                    m_Array[index] = value;
                }
            }
        }

        /// <summary>
        /// Check if the object can be added to the array.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>True if it can be added.</returns>
        public bool Condition(ItemAmount value)
        {
            return value.Item == null
                   || m_ItemCategory == null
                   || m_ItemCategory == value.Item.Category
                   || (m_Inherently && m_ItemCategory.InherentlyContains(value.Item));
        }

        /// <summary>
        /// Check if the object can be added to the array.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>True if it can be added.</returns>
        public bool Condition(ItemDefinition value)
        {
            return value == null
                   || m_ItemCategory == null
                   || m_ItemCategory == value.Category
                   || (m_Inherently && m_ItemCategory.InherentlyContains(value));
        }

        /// <summary>
        /// Check if the object can be added to the array.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>True if it can be added.</returns>
        public void Refresh()
        {
            if (m_Array == null) { return; }

            for (int i = m_Array.Length - 1; i >= 0; i--) {
                if (Condition(m_Array[i]) == false) { RemoveAt(i); }
            }
        }

        /// <summary>
        /// Add an item to the array.
        /// </summary>
        /// <param name="item">The item that will be added to the array.</param>
        /// <return>The position into which the new element was inserted, or -1 to indicate that the item was not inserted into the collection.</return>
        public override int Add(ItemAmount itemAmount)
        {
            if (Condition(itemAmount)) {
                return base.Add(itemAmount);
            }
            return -1;
        }

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="itemCategory">The item category.</param>
        /// <param name="inherently">Set if it is inherently or not.</param>
        public ItemAmountsWithCategory() : this(null, true)
        {
        }

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="itemCategory">The item category.</param>
        /// <param name="inherently">Set if it is inherently or not.</param>
        public ItemAmountsWithCategory(ItemCategory itemCategory, bool inherently) : base()
        {
            m_ItemCategory = itemCategory;
            m_Inherently = inherently;
        }
    }
}