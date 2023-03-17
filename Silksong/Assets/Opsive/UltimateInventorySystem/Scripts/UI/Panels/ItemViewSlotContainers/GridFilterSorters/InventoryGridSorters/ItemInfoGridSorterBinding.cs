/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers.GridFilterSorters.InventoryGridSorters
{
    using Opsive.UltimateInventorySystem.UI.Grid;
    using UnityEngine;

    /// <summary>
    /// Binds the inventory grid to a sorter.
    /// </summary>
    public class InventoryGridSorterBinding : MonoBehaviour
    {
        [Tooltip("The item info grid to bind.")]
        [SerializeField] protected ItemInfoGrid m_ItemInfoGrid;
        [Tooltip("The item info sorter.")]
        [SerializeField] protected ItemInfoSorterBase m_ItemInfoSorter;
        [Tooltip("Bind on start or bind manually through script?")]
        [SerializeField] protected bool m_BindOnStart;

        /// <summary>
        /// The item info grid.
        /// </summary>
        protected virtual void Start()
        {
            if (m_ItemInfoGrid == null) {
                m_ItemInfoGrid = GetComponent<ItemInfoGrid>();
            }

            if (m_ItemInfoSorter == null) {
                m_ItemInfoSorter = GetComponent<ItemInfoSorterBase>();
            }

            if (m_BindOnStart) { Bind(); }
        }

        /// <summary>
        /// Bind the sorter to the grid.
        /// </summary>
        public void Bind()
        {
            m_ItemInfoGrid.BindGridFilterSorter(m_ItemInfoSorter);
        }

        /// <summary>
        /// Unbind the sorter from the grid.
        /// </summary>
        public void UnBind()
        {
            m_ItemInfoGrid.BindGridFilterSorter(null);
        }
    }
}