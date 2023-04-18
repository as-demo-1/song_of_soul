/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels
{
    using UnityEngine;

    /// <summary>
    /// Abstract class for a display binding.
    /// </summary>
    public abstract class DisplayPanelBinding : MonoBehaviour
    {
        protected DisplayPanel m_DisplayPanel;

        public DisplayPanel DisplayPanel {
            get {
                if (m_DisplayPanel == null) {
                    m_DisplayPanel = GetComponent<DisplayPanel>();
                }
                return m_DisplayPanel;
            }
        }

        protected bool m_IsInitialized;

        /// <summary>
        /// Initialize the display binding.
        /// </summary>
        /// <param name="display">The display to bind to.</param>
        /// <param name="force"></param>
        public virtual void Initialize(DisplayPanel display, bool force)
        {
            if (m_IsInitialized && display == m_DisplayPanel && !force) { return; }
            m_DisplayPanel = display;
            m_IsInitialized = true;
        }

        /// <summary>
        /// The panel was opened.
        /// </summary>
        public virtual void OnOpen()
        { }

        /// <summary>
        /// The panel was closed.
        /// </summary>
        public virtual void OnClose()
        { }

        /// <summary>
        /// The panel was opened back.
        /// </summary>
        public virtual void OnOpenBack()
        { }
    }
}