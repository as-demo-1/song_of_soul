/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Input
{
    /// <summary>
    /// The base class for both mobile and standalone (keyboard/mouse and controller) input. This base class exists so UnityInput doesn't need to know if it
    /// is working with mobile controls or standalone controls.
    /// </summary>
    public abstract class InputBase
    {
        /// <summary>
        /// The type of button action to check against.
        /// </summary>
        public enum ButtonAction { GetButton, GetButtonDown, GetButtonUp }

        protected PlayerInput m_PlayerInput;

        /// <summary>
        /// Initializes the UnityInputBase.
        /// </summary>
        /// <param name="playerInput">A reference to the PlayerInput component.</param>
        public void Initialize(PlayerInput playerInput)
        {
            m_PlayerInput = playerInput;
        }

        /// <summary>
        /// Returns if the button is true with the specified ButtonAction.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <param name="action">The type of action to check.</param>
        /// <returns>The status of the action.</returns>
        public abstract bool GetButton(string name, ButtonAction action);

        /// <summary>
        /// Returns the axis of the specified button.
        /// </summary>
        /// <param name="name">The name of the axis.</param>
        /// <returns>The axis value.</returns>
        public abstract float GetAxis(string name);

        /// <summary>
        /// Returns the raw axis of the specified button.
        /// </summary>
        /// <param name="axisName">The name of the axis.</param>
        /// <returns>The raw axis value.</returns>
        public abstract float GetAxisRaw(string axisName);
    }
}