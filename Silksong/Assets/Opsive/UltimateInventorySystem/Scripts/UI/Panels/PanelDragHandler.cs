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
    /// The base class for UI panels.
    /// </summary>
    public class PanelDragHandler : MonoBehaviour, IDragHandler, IPointerDownHandler
    {
        [Tooltip("The panel rect transform.")]
        [SerializeField] protected RectTransform m_PanelRect;

        protected Canvas m_Canvas;

        /// <summary>
        /// Initialize.
        /// </summary>
        protected virtual void Awake()
        {
            if (m_PanelRect == null) { m_PanelRect = GetComponent<RectTransform>(); }
            if (m_Canvas == null) { m_Canvas = GetComponentInParent<Canvas>(); }
        }

        /// <summary>
        /// Drag event.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDrag(PointerEventData eventData)
        {
            m_PanelRect.anchoredPosition += eventData.delta / m_Canvas.scaleFactor;
        }

        /// <summary>
        /// Pointer down event.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerDown(PointerEventData eventData)
        {
            m_PanelRect.SetAsLastSibling();
        }
    }
}