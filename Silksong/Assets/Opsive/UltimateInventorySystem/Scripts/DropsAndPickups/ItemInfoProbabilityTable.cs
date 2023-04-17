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
    /// a probability table made out of a list of item infos.
    /// </summary>
    public class ItemInfoProbabilityTable
    {
        protected int m_ProbabilitySum;
        protected ItemInfo[] m_ItemInfos;
        protected ResizableArray<ItemInfo> m_RandomResultItemInfos;

        public int Count => m_ItemInfos.Length;
        public ItemInfo this[int index] {
            get => m_ItemInfos[index];
            set => m_ItemInfos[index] = value;
        }

        public int ProbabilitySum => m_ProbabilitySum;

        /// <summary>
        /// Construct the probability table.
        /// </summary>
        /// <param name="itemInfos">The item amounts.</param>
        public ItemInfoProbabilityTable(ListSlice<ItemAmount> itemAmount)
        {
            m_RandomResultItemInfos = new ResizableArray<ItemInfo>();
            m_ItemInfos = new ItemInfo[itemAmount.Count];

            m_ProbabilitySum = 0;
            for (int i = 0; i < m_ItemInfos.Length; i++) {
                m_ProbabilitySum += itemAmount[i].Amount;
                var item = itemAmount[i].Item;
                if (item.IsMutable) { item = Item.Create(item); }
                m_ItemInfos[i] = (m_ProbabilitySum, (ItemInfo)itemAmount[i]);
            }
        }

        /// <summary>
        /// Construct the probability table.
        /// </summary>
        /// <param name="itemInfos">The item amounts.</param>
        public ItemInfoProbabilityTable(ListSlice<ItemInfo> itemInfos)
        {
            m_RandomResultItemInfos = new ResizableArray<ItemInfo>();
            m_ItemInfos = new ItemInfo[itemInfos.Count];

            m_ProbabilitySum = 0;
            for (int i = 0; i < m_ItemInfos.Length; i++) {
                m_ProbabilitySum += itemInfos[i].Amount;
                var item = itemInfos[i].Item;
                if (item.IsMutable) { item = Item.Create(item); }
                m_ItemInfos[i] = (m_ProbabilitySum, itemInfos[i]);
            }
        }

        /// <summary>
        /// Construct the probability table.
        /// </summary>
        /// <param name="itemStacks">The item stacks.</param>
        public ItemInfoProbabilityTable(ListSlice<ItemStack> itemStacks)
        {
            m_RandomResultItemInfos = new ResizableArray<ItemInfo>();
            m_ItemInfos = new ItemInfo[itemStacks.Count];

            m_ProbabilitySum = 0;
            for (int i = 0; i < m_ItemInfos.Length; i++) {
                m_ProbabilitySum += itemStacks[i].Amount;
                var item = itemStacks[i].Item;
                if (item.IsMutable) { item = Item.Create(item); }
                m_ItemInfos[i] = (m_ProbabilitySum, (ItemInfo)itemStacks[i]);
            }
        }

        /// <summary>
        /// Get a list of random item amounts from the probability table.
        /// </summary>
        /// <param name="minAmount">Minimum amount of items.</param>
        /// <param name="maxAmount">Maximum amount of items.</param>
        /// <returns>A random list of item amounts.</returns>
        public IReadOnlyList<ItemInfo> GetRandomItemInfos(int minAmount, int maxAmount)
        {
            var randomAmount = Random.Range(minAmount, maxAmount + 1);
            m_RandomResultItemInfos.Clear();

            for (int i = 0; i < randomAmount; i++) {

                var selectedItemInfo = GetRandomItemInfo();
                var foundMatch = false;
                for (int j = 0; j < m_RandomResultItemInfos.Count; j++) {
                    if (m_RandomResultItemInfos[j].Item == selectedItemInfo.Item) {
                        m_RandomResultItemInfos[j] = (m_RandomResultItemInfos[j].Amount + 1, m_RandomResultItemInfos[j]);
                        foundMatch = true;
                        break;
                    }
                }

                if (!foundMatch) { m_RandomResultItemInfos.Add((1, selectedItemInfo)); }
            }

            return m_RandomResultItemInfos;
        }

        /// <summary>
        /// Get a random item from the probability table.
        /// </summary>
        /// <returns>A random list of item amounts.</returns>
        public ItemInfo GetRandomItemInfo()
        {
            var randomProbabilityIndex = Random.Range(0, ProbabilitySum);
            //Binary search because probabilitySum is sorted by default
            var min = 0;
            var max = m_ItemInfos.Length - 1;
            var mid = 0;

            while (min <= max) {
                mid = (min + max) / 2;
                if (m_ItemInfos[mid].Amount == randomProbabilityIndex) {
                    ++mid;
                    break;
                }

                if (randomProbabilityIndex < m_ItemInfos[mid].Amount
                    && (mid == 0 || randomProbabilityIndex > m_ItemInfos[mid - 1].Amount)) { break; }

                if (randomProbabilityIndex < m_ItemInfos[mid].Amount) { max = mid - 1; } else {
                    min = mid + 1;
                }
            }

            return m_ItemInfos[mid];
        }
    }
}