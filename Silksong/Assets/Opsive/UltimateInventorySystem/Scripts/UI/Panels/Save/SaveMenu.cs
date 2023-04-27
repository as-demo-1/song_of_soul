/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.Save
{
    using UnityEngine;

    /// <summary>
    /// The save menu panel.
    /// </summary>
    public class SaveMenu : DisplayPanelBinding
    {

        [Tooltip("The save grid.")]
        [SerializeField] protected SaveGrid m_SaveGrid;

        public SaveGrid SaveGrid => m_SaveGrid;

        /// <summary>
        /// Open the panel.
        /// </summary>
        public override void OnOpen()
        {
            if (!m_SaveGrid.IsInitialized) {
                m_SaveGrid.Initialize(false);
            }

            m_SaveGrid.SetParentPanel(m_DisplayPanel);
            m_SaveGrid.Refresh();

            m_SaveGrid.SelectButton(0);
        }
    }
}
