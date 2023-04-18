/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Input.VirtualControls
{
    using UnityEngine;
    using UnityEngine.EventSystems;

    /// <summary>
    /// An abstract class for handing axis input.
    /// </summary>
    public abstract class VirtualAxis : VirtualControl, IPointerDownHandler, IPointerUpHandler
    {
        [Tooltip("The name of the horizontal input axis.")]
        [SerializeField] protected string m_HorizontalInputName = "Horizontal";
        [Tooltip("The name of the vertical input axis.")]
        [SerializeField] protected string m_VerticalInputName = "Vertical";

        protected bool m_Pressed;
        protected Vector2 m_DeltaPosition;

        protected override void Awake()
        {
            base.Awake();

            if (m_VirtualControlsManager != null) {
                m_VirtualControlsManager.RegisterVirtualControl(m_HorizontalInputName, this);
                m_VirtualControlsManager.RegisterVirtualControl(m_VerticalInputName, this);
            }
        }

        /// <summary>
        /// Callback when a pointer has pressed on the button.
        /// </summary>
        /// <param name="data">The pointer data.</param>
        public virtual void OnPointerDown(PointerEventData data)
        {
            if (m_Pressed) {
                return;
            }

            m_Pressed = true;
            m_DeltaPosition = Vector2.zero;
        }

        /// <summary>
        /// Callback when a pointer has released the button.
        /// </summary>
        /// <param name="data">The pointer data.</param>
        public virtual void OnPointerUp(PointerEventData data)
        {
            if (!m_Pressed) {
                return;
            }

            m_Pressed = false;
            m_DeltaPosition = Vector2.zero;
        }
    }
}