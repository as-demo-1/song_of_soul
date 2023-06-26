/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels
{
    using UnityEngine;

    /// <summary>
    /// The base class for UI panels.
    /// </summary>
    public class DisplayPanelDragHandler : PanelDragHandler
    {
        [Tooltip("The display panel which should be selected when this gameobject is dragged.")]
        [SerializeField] protected DisplayPanel m_Panel;

        /// <summary>
        /// Initialize.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            if (m_Panel == null) { m_Panel = GetComponentInParent<DisplayPanel>(); }
            m_Panel.OnOpen += HandleDisplayPanelOpen;
        }

        /// <summary>
        /// Set the panel in front of its siblings when selected.
        /// </summary>
        private void HandleDisplayPanelOpen()
        {
            m_PanelRect.SetAsLastSibling();
        }
    }
}