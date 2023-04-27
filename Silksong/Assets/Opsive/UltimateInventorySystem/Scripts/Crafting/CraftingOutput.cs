/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Crafting
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using System;
    using UnityEngine;

    /// <summary>
    /// Crafting output product of a recipe.
    /// </summary>
    [Serializable]
    public class CraftingOutput
    {
        [Tooltip("Item amounts.")]
        [SerializeField] [HideInInspector] protected ItemAmounts m_ItemAmounts;

        public ItemAmounts ItemAmounts => m_ItemAmounts;
        public ItemAmount? MainItemAmount {
            get {
                if (m_ItemAmounts == null || m_ItemAmounts.Count == 0) { return null; }
                return m_ItemAmounts[0];
            }
        }

        /// <summary>
        /// Set the ItemAmounts.
        /// </summary>
        public CraftingOutput()
        {
            m_ItemAmounts = new ItemAmounts();
        }

        /// <summary>
        /// Set the ItemAmounts.
        /// </summary>
        /// <param name="ItemAmounts">Item amounts.</param>
        public CraftingOutput(ItemAmounts ItemAmounts = null)
        {
            m_ItemAmounts = ItemAmounts ?? new ItemAmounts();
        }
    }
}