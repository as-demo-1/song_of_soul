/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Grid
{
    using Opsive.UltimateInventorySystem.UI.Panels;
    using UnityEngine;

    /// <summary>
    /// A grid display is similar to a gridUI but it is a panel.
    /// </summary>
    public class GridPanel : DisplayPanelBinding
    {
        [Tooltip("The grid.")]
        [SerializeField] protected GridBase m_GridBase;

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="display">The display panel.</param>
        /// <param name="force"></param>
        public override void Initialize(DisplayPanel display, bool force)
        {
            base.Initialize(display, force);
            m_GridBase.SetParentPanel(m_DisplayPanel);
            m_GridBase.Initialize(force);
        }
    }
}