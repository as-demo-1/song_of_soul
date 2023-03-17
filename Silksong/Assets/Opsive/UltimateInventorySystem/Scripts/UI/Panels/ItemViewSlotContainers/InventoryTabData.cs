/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers
{
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.UI.Grid;
    using UnityEngine;

    /// <summary>
    /// Tab toggle is used by a TabController to create tabs.
    /// </summary>
    public class InventoryTabData : MonoBehaviour
    {
        [Tooltip("The inventory to use for this tab data (optional).")]
        [SerializeField] protected Inventory m_Inventory;
        [Tooltip("The item info filter.")]
        [SerializeField] protected internal ItemInfoFilterSorterBase m_ItemInfoFilter;

        protected InventoryGridIndexer m_Indexer;
        protected bool m_IsInitialized;

        public Inventory Inventory => m_Inventory;
        public ItemInfoFilterSorterBase ItemInfoFilter => m_ItemInfoFilter;

        public InventoryGridIndexer Indexer {
            get => m_Indexer;
            set => m_Indexer = value;
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        private void Awake()
        {
            Initialize(false);
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="force">Force initialize.</param>
        public void Initialize(bool force)
        {
            if (m_IsInitialized && force == false) { return; }

            m_Indexer = new InventoryGridIndexer();

            m_IsInitialized = true;
        }
    }
}