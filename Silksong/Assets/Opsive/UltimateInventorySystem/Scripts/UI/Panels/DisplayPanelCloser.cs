/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.Serialization;

    /// <summary>
    /// A component that allows to close panels when clicking the background image.
    /// </summary>
    public class DisplayPanelCloser : GraphicRaycasterTarget
    {
        [FormerlySerializedAs("m_CloseOnBackgroundClick")]
        [Tooltip("Close the panel when clicking the background image (Use an image that covers the entire screen).")]
        [SerializeField] protected bool m_CloseOnClick = true;
        [Tooltip("The display panel that needs to act as a pop up.")]
        [SerializeField] protected DisplayPanel m_DisplayPanel;

        public bool CloseOnClick {
            get => m_CloseOnClick;
            set => m_CloseOnClick = value;
        }

        /// <summary>
        /// Initialize the panel closer.
        /// </summary>
        protected override void Awake()
        {
            if (m_DisplayPanel == null) { m_DisplayPanel = GetComponent<DisplayPanel>(); }
            base.Awake();
        }

        /// <summary>
        /// Handle the on pointer click event and close the display panel.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (m_CloseOnClick) {
                m_DisplayPanel.Close();
            }

            base.OnPointerClick(eventData);
        }
    }
}