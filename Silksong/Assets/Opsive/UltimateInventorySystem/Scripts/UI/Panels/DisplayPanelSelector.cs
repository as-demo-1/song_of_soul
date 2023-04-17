/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels
{
    using UnityEngine;
    using UnityEngine.EventSystems;

    /// <summary>
    /// A component used to select a panel by clicking or hovering it.
    /// </summary>
    public class DisplayPanelSelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Tooltip("The display panel that needs to act as a pop up.")]
        [SerializeField] protected DisplayPanel m_DisplayPanel;
        [Tooltip("Should the display panel be selected when this is clicked?")]
        [SerializeField] protected bool m_SelectOnClick = true;
        [Tooltip("Should the display panel be selected when this  is hovered?")]
        [SerializeField] protected bool m_SelectOnHover = true;

        /// <summary>
        /// Initialize.
        /// </summary>
        protected void Awake()
        {
            if (m_DisplayPanel == null) { m_DisplayPanel = GetComponent<DisplayPanel>(); }

            if (m_DisplayPanel == null) {
                Debug.LogError("The Display Panel Selector Must reference a Display Panel Manager", gameObject);
                return;
            }
        }

        /// <summary>
        /// Select the panel.
        /// </summary>
        public virtual void SelectPanel()
        {
            if (m_DisplayPanel.Manager.SelectedDisplayPanel != m_DisplayPanel) {
                m_DisplayPanel.SmartOpen();
            }
        }

        /// <summary>
        /// Handle the On pointer click event.
        /// </summary>
        /// <param name="eventData">The pointer event data.</param>
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (m_SelectOnClick) { SelectPanel(); }
        }

        // <summary>
        /// Handle the On pointer enter event.
        /// </summary>
        /// <param name="eventData">The pointer event data.</param>
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (m_SelectOnHover) { SelectPanel(); }
        }

        // <summary>
        /// Handle the On pointer exit event.
        /// </summary>
        /// <param name="eventData">The pointer event data.</param>
        public virtual void OnPointerExit(PointerEventData eventData)
        {

        }
    }
}