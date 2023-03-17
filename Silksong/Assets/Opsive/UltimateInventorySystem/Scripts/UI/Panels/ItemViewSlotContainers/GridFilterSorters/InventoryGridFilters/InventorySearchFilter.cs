/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers.GridFilterSorters.InventoryGridFilters
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.Grid;
    using Opsive.UltimateInventorySystem.UI.Item;
    using System;
#if TEXTMESH_PRO_PRESENT 
    using TMPro;
#else
    using UnityEngine.UI;
#endif
    using UnityEngine;


    public class InventorySearchFilter : ItemInfoFilterBase
    {
        [Tooltip("The Inventory Grid that the filter will affect.")]
        [SerializeField] internal InventoryGrid m_InventoryGrid;

        [Tooltip("The Input Field used to search the Inventory Grid.")]
#if TEXTMESH_PRO_PRESENT
        [SerializeField] protected TMP_InputField m_InputField;
#else
        [SerializeField] protected InputField m_InputField;
#endif

        [Tooltip("Add a sorter that is effective only while the search filter is active.")]
        [SerializeField] internal ItemInfoFilterSorterBase m_BindSorterWhileSearching;

        protected IFilterSorter<ItemInfo> m_PreviousBoundSorter;
        protected bool m_Searching = false;
        private bool m_DefaultUseIndexerValue;

        public InventoryGrid InventoryGrid => m_InventoryGrid;

        /// <summary>
        /// Initialize.
        /// </summary>
        protected void Awake()
        {
            m_InputField.onValueChanged.AddListener(HandleOnTextChange);

            if (m_InventoryGrid == null) {
                m_InventoryGrid = GetComponent<InventoryGrid>();
                if (m_InventoryGrid == null) {
                    Debug.LogError("The inventory grid search filter is missing an inventory grid.", gameObject);
                    return;
                }
            }

            m_DefaultUseIndexerValue = m_InventoryGrid.UseGridIndex;
        }

        /// <summary>
        /// Handle the input text change to update the filter.
        /// </summary>
        /// <param name="newText">The new text.</param>
        private void HandleOnTextChange(string newText)
        {
            if (m_Searching && string.IsNullOrEmpty(newText)) {
                m_Searching = false;
                if ((IFilterSorter<ItemInfo>)this == m_PreviousBoundSorter) {
                    m_InventoryGrid.BindGridFilterSorter(null);
                } else {
                    m_InventoryGrid.BindGridFilterSorter(m_PreviousBoundSorter);
                }

                m_InventoryGrid.UseGridIndex = m_DefaultUseIndexerValue;
                //m_InventoryGrid.InventoryGridIndexer.Copy(m_GridIndexer);
            } else if (m_Searching == false && string.IsNullOrEmpty(newText) == false) {
                m_Searching = true;
                m_PreviousBoundSorter = m_InventoryGrid.BindGridFilterSorter(this);
                //m_GridIndexer.Copy(m_InventoryGrid.InventoryGridIndexer);
                m_InventoryGrid.UseGridIndex = false;
            }

            m_InventoryGrid.Draw();
        }

        /// <summary>
        /// Filter the items with the search result.
        /// </summary>
        /// <param name="input">The input items.</param>
        /// <param name="outputPooledArray">The filtered items.</param>
        /// <returns>The filtered items.</returns>
        public override ListSlice<ItemInfo> Filter(ListSlice<ItemInfo> input, ref ItemInfo[] outputPooledArray)
        {
            input = base.Filter(input, ref outputPooledArray);

            if (m_BindSorterWhileSearching != null) {
                input = m_BindSorterWhileSearching.Filter(input, ref outputPooledArray);
            }

            return input;
        }

        /// <summary>
        /// Filter the items.
        /// </summary>
        /// <param name="itemInfo">The item infos.</param>
        /// <returns>True if the item should be added.</returns>
        public override bool Filter(ItemInfo itemInfo)
        {
            var search = m_InputField.text;
            var itemName = itemInfo.Item.name;

            return itemName.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }
    }
}