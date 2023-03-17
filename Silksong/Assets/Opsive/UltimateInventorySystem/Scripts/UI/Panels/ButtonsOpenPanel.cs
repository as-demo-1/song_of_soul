/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels
{
    using Opsive.UltimateInventorySystem.Core;
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    [Serializable]
    public struct ButtonToPanel
    {
        [Tooltip("The panel name (optional if the panel is referenced directly)")]
        [SerializeField] private Button m_Button;
        [Tooltip("The panel name (optional if the panel is referenced directly)")]
        [SerializeField] private string m_PanelName;
        [Tooltip("The panel name (optional if the panel is referenced directly)")]
        [SerializeField] private DisplayPanel m_Panel;

        public Button Button => m_Button;
        public string PanelName => m_PanelName;
        public DisplayPanel Panel => m_Panel;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="button">The button.</param>
        /// <param name="panelName">The panel name.</param>
        /// <param name="panel">The panel.</param>
        public ButtonToPanel(Button button, string panelName, DisplayPanel panel)
        {
            m_Button = button;
            m_PanelName = panelName;
            m_Panel = panel;
        }
    }

    /// <summary>
    /// object that opens panels.
    /// </summary>
    public class ButtonsOpenPanel : MonoBehaviour
    {
        [Tooltip("The panel manager index.")]
        [SerializeField] protected uint m_PanelManagerIndex = 1;
        [Tooltip("Should the button open or toggle the panel.")]
        [SerializeField] protected bool m_ToggleOpen = true;
        [Tooltip("The panel that will be used a the 'previous' panel when opening the other panels (if null, smart open will be used).")]
        [SerializeField] protected DisplayPanel m_SourcePanel;
        [Tooltip("Allow only one panel to be opened at a time.")] 
        [SerializeField] protected bool m_AllowOnlyOneOpen;
        [Tooltip("The buttons mapped to panels.")]
        [SerializeField] protected ButtonToPanel[] m_ButtonsToPanels;

        protected bool m_IsInitialized = false;
        protected DisplayPanelManager m_PanelManager;

        /// <summary>
        /// Initialize.
        /// </summary>
        protected virtual void Start()
        {
            Initialize(false);
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="force">Force Initialize.</param>
        protected virtual void Initialize(bool force)
        {
            if (m_IsInitialized && !force) { return; }

            if (m_PanelManager == null) {
                m_PanelManager = InventorySystemManager.GetDisplayPanelManager(m_PanelManagerIndex);
            }

            for (int i = 0; i < m_ButtonsToPanels.Length; i++) {
                var localI = i;
                m_ButtonsToPanels[i].Button.onClick.AddListener(() => HandleButtonClicked(localI));
            }

            m_IsInitialized = true;
        }

        /// <summary>
        /// Handle a click event.
        /// </summary>
        /// <param name="i">The index that was clicked.</param>
        protected virtual void HandleButtonClicked(int i)
        {
            var panel = m_ButtonsToPanels[i].Panel;
            if (panel == null) {
                panel = m_PanelManager.GetPanel(m_ButtonsToPanels[i].PanelName);

                if (panel == null) {
                    Debug.LogWarning($"The panel 'm_ButtonsToPanels[i].PanelName' could not be found.");
                    return;
                }
            }

            if (m_AllowOnlyOneOpen) {
                for (int j = 0; j < m_ButtonsToPanels.Length; j++) {
                    if(j == i){ continue; }
                    if(m_ButtonsToPanels[j].Panel.IsOpen == false){ continue; }
                    m_ButtonsToPanels[j].Panel.Close(false);
                }
            }
            

            if (m_SourcePanel == null) {
                if (m_ToggleOpen) {
                    panel.SmartToggle();
                } else {
                    panel.SmartOpen();
                }
                return;
            }

            if (m_ToggleOpen && panel.IsOpen) {
                panel.Close();
            } else {
                panel.Open(m_SourcePanel);
            }

        }
    }
}