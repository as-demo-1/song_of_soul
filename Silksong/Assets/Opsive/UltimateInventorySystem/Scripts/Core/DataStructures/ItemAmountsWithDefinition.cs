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
    public class ItemAmountsWithDefinition : ItemAmounts
    {
        [Tooltip("All items in this item amounts must be part of this definition.")]
        [SerializeField] protected ItemDefinition m_ItemDefinition;
        [Tooltip("Does it accept item that are inherently part of the definition or must they be direct?")]
        [SerializeField] protected bool m_Inherently;

        public ItemDefinition ItemDefinition {
            get => m_ItemDefinition;
            internal set {
                if (m_ItemDefinition == value) { return; }
                m_ItemDefinition = value;
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
                   || m_ItemDefinition == null
                   || m_ItemDefinition == value.Item.ItemDefinition
                   || (m_Inherently && m_ItemDefinition.InherentlyContains(value.Item));
        }

        /// <summary>
        /// Check if the object can be added to the array.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>True if it can be added.</returns>
        public bool Condition(ItemDefinition value)
        {
            return value == null
                   || m_ItemDefinition == null
                   || m_ItemDefinition == value
                   || (m_Inherently && m_ItemDefinition.InherentlyContains(value));
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
        public ItemAmountsWithDefinition() : this(null, true)
        {
        }

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="itemDefinition">The item definition.</param>
        /// <param name="inherently">Set if it is inherently or not.</param>
        public ItemAmountsWithDefinition(ItemDefinition itemDefinition, bool inherently) : base()
        {
            m_ItemDefinition = itemDefinition;
            m_Inherently = inherently;
        }
    }
}