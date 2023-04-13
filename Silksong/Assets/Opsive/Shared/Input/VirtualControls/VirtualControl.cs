/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Input.VirtualControls
{
    using UnityEngine;

    /// <summary>
    /// An abstract class which represents a virtual control on the screen.
    /// </summary>
    public abstract class VirtualControl : MonoBehaviour
    {
        protected VirtualControlsManager m_VirtualControlsManager;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected virtual void Awake()
        {
            m_VirtualControlsManager = GetComponentInParent<VirtualControlsManager>();
            if (m_VirtualControlsManager == null) {
                Debug.LogError("Error: Unable to find the VirtualControlsManager. This component must be a parent to the virtual input monitors.");
            }
        }

        /// <summary>
        /// Returns if the button is true with the specified ButtonAction.
        /// </summary>
        /// <param name="action">The type of action to check.</param>
        /// <returns>The status of the action.</returns>
        public virtual bool GetButton(InputBase.ButtonAction action) { return false; }

        /// <summary>
        /// Returns the value of the axis.
        /// </summary>
        /// <param name="buttonName">The name of the axis.</param>
        /// <returns>The value of the axis.</returns>
        public virtual float GetAxis(string buttonName) { return 0; }
    }
}