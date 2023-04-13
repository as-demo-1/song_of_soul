/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Grid
{
    using UnityEngine;

    /// <summary>
    /// Grid Binding component
    /// </summary>
    public abstract class GridBinding : MonoBehaviour
    {

        protected GridBase m_Grid;
        protected bool m_IsInitialized;

        /// <summary>
        /// Awake.
        /// </summary>
        protected virtual void Awake()
        {
            Initialize(false);
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="force">Force Intialize.</param>
        public virtual void Initialize(bool force)
        {
            if (m_IsInitialized && force == false) { return; }

            m_IsInitialized = true;
        }

        /// <summary>
        /// Bind a Grid.
        /// </summary>
        /// <param name="grid">The grid.</param>
        public virtual void Bind(GridBase grid)
        {
            Initialize(false);
            if (m_Grid == grid) { return; }

            UnBind();

            m_Grid = grid;
        }

        /// <summary>
        /// Unbind the grid.
        /// </summary>
        public virtual void UnBind()
        {
            Initialize(false);
            m_Grid = null;
        }
    }
}