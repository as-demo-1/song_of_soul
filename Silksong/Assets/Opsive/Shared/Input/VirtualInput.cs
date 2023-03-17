/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Input
{
    using Opsive.Shared.Input.VirtualControls;

    /// <summary>
    /// Uses virtual buttons to detect input related actions.
    /// </summary>
    public class VirtualInput : InputBase
    {
        private VirtualControlsManager m_VirtualControlsManager;

        /// <summary>
        /// Associates the VirtualControlsManager with the VirtualInput object.
        /// </summary>
        /// <param name="virtualControlsManager">The VirtualControlsManager to associate with the VirtualInput object.</param>
        public void RegisterVirtualControlsManager(VirtualControlsManager virtualControlsManager)
        {
            m_VirtualControlsManager = virtualControlsManager;
        }

        /// <summary>
        /// Removes the VirtualControlsManager association.
        /// </summary>
        public void UnregisterVirtualControlsManager()
        {
            m_VirtualControlsManager = null;
        }

        /// <summary>
        /// Returns the axis of the specified button.
        /// </summary>
        /// <param name="axisName">The name of the axis.</param>
        /// <returns>The axis value.</returns>
        public override float GetAxis(string axisName)
        {
            if (m_VirtualControlsManager == null) {
                return 0;
            }

            return m_VirtualControlsManager.GetAxis(axisName);
        }

        /// <summary>
        /// Returns the raw axis of the specified button.
        /// </summary>
        /// <param name="axisName">The name of the axis.</param>
        /// <returns>The raw axis value.</returns>
        public override float GetAxisRaw(string axisName)
        {
            if (m_VirtualControlsManager == null) {
                return 0;
            }

            return m_VirtualControlsManager.GetAxis(axisName);
        }

        /// <summary>
        /// Returns if the button is true with the specified ButtonAction.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <param name="action">The type of action to check.</param>
        /// <returns>The status of the action.</returns>
        public override bool GetButton(string name, ButtonAction action)
        {
            if (m_VirtualControlsManager == null) {
                return false;
            }

            return m_VirtualControlsManager.GetButton(name, action);
        }
    }
}