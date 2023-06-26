/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.DropsAndPickups
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// a probability table made out of a list of item amounts.
    /// </summary>
    public class ItemAmountProbabilityTable
    {
        protected int m_ProbabilitySum;
        protected ItemAmount[] m_ItemAmounts;
        protected ResizableArray<ItemAmount> m_RandomResultItemAmounts;

        public int Count => m_ItemAmounts.Length;
        public ItemAmount this[int index] {
            get => m_ItemAmounts[index];
            set => m_ItemAmounts[index] = value;
        }

        public int ProbabilitySum => m_ProbabilitySum;

        /// <summary>
        /// Construct the probability table.
        /// </summary>
        /// <param name="itemAmounts">The item amounts.</param>
        public ItemAmountProbabilityTable(ListSlice<ItemAmount> itemAmounts)
        {
            m_RandomResultItemAmounts = new ResizableArray<ItemAmount>();
            m_ItemAmounts = new ItemAmount[itemAmounts.Count];

            m_ProbabilitySum = 0;
            for (int i = 0; i < m_ItemAmounts.Length; i++) {
                m_ProbabilitySum += itemAmounts[i].Amount;
                var item = itemAmounts[i].Item;
                if (item.IsMutable) { item = Item.Create(item); }
                m_ItemAmounts[i] = (m_ProbabilitySum, item);
            }
        }

        /// <summary>
        /// Construct the probability table.
        /// </summary>
        /// <param name="itemStacks">The item stacks.</param>
        public ItemAmountProbabilityTable(ListSlice<ItemStack> itemStacks)
        {
            m_RandomResultItemAmounts = new ResizableArray<ItemAmount>();
            m_ItemAmounts = new ItemAmount[itemStacks.Count];

            m_ProbabilitySum = 0;
            for (int i = 0; i < m_ItemAmounts.Length; i++) {
                m_ProbabilitySum += itemStacks[i].Amount;
                var item = itemStacks[i].Item;
                if (item.IsMutable) { item = Item.Create(item); }
                m_ItemAmounts[i] = (m_ProbabilitySum, itemStacks[i].Item);
            }
        }

        /// <summary>
        /// Get a list of random item amounts from the probability table.
        /// </summary>
        /// <param name="minAmount">Minimum amount of items.</param>
        /// <param name="maxAmount">Maximum amount of items.</param>
        /// <returns>A random list of item amounts.</returns>
        public IReadOnlyList<ItemAmount> GetRandomItemAmounts(int minAmount, int maxAmount)
        {
            var randomAmount = Random.Range(minAmount, maxAmount + 1);
            m_RandomResultItemAmounts.Clear();

            for (int i = 0; i < randomAmount; i++) {
                var randomProbabilityIndex = Random.Range(0, ProbabilitySum);
                //Binary search because probabilitySum is sorted by default
                var min = 0;
                var max = m_ItemAmounts.Length - 1;
                var mid = 0;

                while (min <= max) {
                    mid = (min + max) / 2;
                    if (m_ItemAmounts[mid].Amount == randomProbabilityIndex) {
                        ++mid;
                        break;
                    }

                    if (randomProbabilityIndex < m_ItemAmounts[mid].Amount
                        && (mid == 0 || randomProbabilityIndex > m_ItemAmounts[mid - 1].Amount)) { break; }

                    if (randomProbabilityIndex < m_ItemAmounts[mid].Amount) { max = mid - 1; } else {
                        min = mid + 1;
                    }
                }

                var selectedItem = m_ItemAmounts[mid].Item;
                var foundMatch = false;
                for (int j = 0; j < m_RandomResultItemAmounts.Count; j++) {
                    if (m_RandomResultItemAmounts[j].Item == selectedItem) {
                        m_RandomResultItemAmounts[j] = (selectedItem, m_RandomResultItemAmounts[j].Amount + 1);
                        foundMatch = true;
                        break;
                    }
                }

                if (!foundMatch) { m_RandomResultItemAmounts.Add((1, selectedItem)); }
            }

            return m_RandomResultItemAmounts;
        }
    }
}