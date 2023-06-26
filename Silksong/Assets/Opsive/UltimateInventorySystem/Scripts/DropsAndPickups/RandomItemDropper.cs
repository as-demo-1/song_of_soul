/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.DropsAndPickups
{
    using Opsive.Shared.Utility;
    using UnityEngine;

    /// <summary>
    /// Random Item dropper. Use the Inventory as a probability table.
    /// </summary>
    public class RandomItemDropper : ItemDropper
    {
        [Tooltip("The minimum amount of item that can be dropped.")]
        [SerializeField] protected int m_MinAmount = 1;
        [Tooltip("The maximum amount of item that can be dropped.")]
        [SerializeField] protected int m_MaxAmount = 2;

        protected ItemInfoProbabilityTable m_ItemAmountProbabilityTable;

        /// <summary>
        /// Initialize the probability table.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            if (m_Inventory == null) { return; }

            var itemCollection = m_Inventory.GetItemCollection(m_ItemCollectionID);
            if (itemCollection == null) { return; }

            m_ItemAmountProbabilityTable = new ItemInfoProbabilityTable(
                itemCollection.GetAllItemStacks().ToListSlice());
        }

        /// <summary>
        /// Drop a random set of item amounts.
        /// </summary>
        public override void Drop()
        {
            var randomItemAmounts =
                m_ItemAmountProbabilityTable.GetRandomItemInfos(m_MinAmount, m_MaxAmount);

            DropItemsInternal(randomItemAmounts.ToListSlice());
        }
    }
}