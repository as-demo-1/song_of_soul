/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers.GridFilterSorters.InventoryGridSorters
{
    using Opsive.UltimateInventorySystem.UI.Grid;
    using Opsive.UltimateInventorySystem.UI.Item;
    using System.Collections.Generic;
#if TEXTMESH_PRO_PRESENT
    using TMPro;
#else
    using UnityEngine.UI;
#endif
    using UnityEngine;

    public class InventoryGridSorterDropDown : MonoBehaviour
    {
        [Tooltip("The inventory grid to sort.")]
        [SerializeField] internal InventoryGrid m_InventoryGrid;
        [Tooltip("The dropdown UI component. Maps the Option A to no sort and Option B to sort index 0.")]
#if TEXTMESH_PRO_PRESENT
        [SerializeField] internal TMP_Dropdown m_Dropdown;
#else
        [SerializeField] internal Dropdown m_Dropdown;
#endif
        [Tooltip("The grid sorters.")]
        [SerializeField] internal List<ItemInfoSorterBase> m_GridSorters;
        [Tooltip("Sort the entire grid or just the current selected tab?")]
        [SerializeField] protected bool m_SortSelectedTabOnly;
        [Tooltip("Sort once one click or Bind the sorter to continuously sort the grid on refresh?.")]
        [SerializeField] protected bool m_BindTheSorter;

        /// <summary>
        /// Initialize.
        /// </summary>
        private void Awake()
        {
            m_Dropdown.onValueChanged.AddListener(HandleValueChange);
        }

        /// <summary>
        /// Handle the value changed event.
        /// </summary>
        /// <param name="index">The new index.</param>
        public void HandleValueChange(int index)
        {
            // Unbind or don't sort if index is 0.
            if (index == 0) {
                if (m_BindTheSorter) {
                    m_InventoryGrid.BindGridFilterSorter(null);
                }
                return;
            }

            // Map the dropdown 1 index to the sorter array 0 index.
            index = index - 1;

            if (index < 0 || index >= m_GridSorters.Count) {
                Debug.LogWarning("Index our of range in the grid sorters array.");
                return;
            }

            if (m_BindTheSorter) {
                m_InventoryGrid.BindGridFilterSorter(m_GridSorters[index]);
                return;
            }

            m_Dropdown.SetValueWithoutNotify(0);

            var tabControlBinding = m_InventoryGrid.GetComponent<InventoryGridTabControlBinding>();
            if (m_SortSelectedTabOnly == false && tabControlBinding != null) {
                tabControlBinding.SortItemIndexes(m_GridSorters[index].Comparer);
                m_InventoryGrid.Draw();
                return;
            }

            m_InventoryGrid.SortItemIndexes(m_GridSorters[index].Comparer);
            m_InventoryGrid.Draw();
        }
    }
}