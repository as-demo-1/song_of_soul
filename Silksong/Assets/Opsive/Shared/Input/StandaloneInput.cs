/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Input
{
    using UnityEngine;

    /// <summary>
    /// Uses Unity's input system to detect input related actions.
    /// </summary>
    public class StandaloneInput : InputBase
    {
        /// <summary>
        /// Returns if the button is true with the specified ButtonAction.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <param name="action">The type of action to check.</param>
        /// <returns>The status of the action.</returns>
        public override bool GetButton(string name, ButtonAction action)
        {
#if UNITY_EDITOR
            try {
                switch (action) {
                    case ButtonAction.GetButton:
                        return UnityEngine.Input.GetButton(name);
                    case ButtonAction.GetButtonDown:
                        return UnityEngine.Input.GetButtonDown(name);
                    case ButtonAction.GetButtonUp:
                        return UnityEngine.Input.GetButtonUp(name);
                }
            }
            catch (System.Exception /*e*/) {
                Debug.LogError("Button \"" + name + "\" is not setup. Please create a button mapping within the Unity Input Manager.");
            }
#else
            switch (action) {
                case ButtonAction.GetButton:
                    return UnityEngine.Input.GetButton(name);
                case ButtonAction.GetButtonDown:
                    return UnityEngine.Input.GetButtonDown(name);
                case ButtonAction.GetButtonUp:
                    return UnityEngine.Input.GetButtonUp(name);
            }
#endif
            return false;
        }

        /// <summary>
        /// Return the value of the axis with the specified name.
        /// </summary>
        /// <param name="name">The name of the axis.</param>
        /// <returns>The value of the axis.</returns>
        public override float GetAxis(string name)
        {
#if UNITY_EDITOR
            try {
                return UnityEngine.Input.GetAxis(name);
            }
            catch (UnityException /*e*/) {
                Debug.LogError("Axis \"" + name + "\" is not setup. Please create an axis mapping within the Unity Input Manager.");
            }
            return 0;
#else
            return UnityEngine.Input.GetAxis(name);
#endif
        }

        /// <summary>
        /// Return the value of theraw  axis with the specified name.
        /// </summary>
        /// <param name="name">The name of the axis.</param>
        /// <returns>The value of the raw axis.</returns>
        public override float GetAxisRaw(string name)
        {
#if UNITY_EDITOR
            try {
                return UnityEngine.Input.GetAxisRaw(name);
            } catch (UnityException /*e*/) {
                Debug.LogError("Axis \"" + name + "\" is not setup. Please create an axis mapping within the Unity Input Manager.");
            }
            return 0;
#else
            return UnityEngine.Input.GetAxisRaw(name);
#endif
        }
    }
}