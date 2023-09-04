// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Adds Lua functions that interface with the Pixel Crushers Common Library.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class CommonLibraryLua : MonoBehaviour
    {
        [Tooltip("Unregister functions when this component is disabled. Leave unticked if this script is on Dialogue Manager or other persistent GameObject.")]
        public bool unregisterOnDisable = false;

        private static bool s_registered = false;

        private bool didIRegister = false;

        void OnEnable()
        {
            if (!s_registered)
            {
                // Make the functions available to Lua:
                Lua.RegisterFunction("SendMessageSystem", this, SymbolExtensions.GetMethodInfo(() => SendMessageSystem(string.Empty, string.Empty)));
                Lua.RegisterFunction("SendMessageSystemString", this, SymbolExtensions.GetMethodInfo(() => SendMessageSystemString(string.Empty, string.Empty, string.Empty)));
                Lua.RegisterFunction("SendMessageSystemInt", this, SymbolExtensions.GetMethodInfo(() => SendMessageSystemInt(string.Empty, string.Empty, (double)0)));
                s_registered = true;
                didIRegister = true;
            }
        }

        void OnDisable()
        {
            if (unregisterOnDisable && s_registered && didIRegister)
            {
                // Remove the functions from Lua:
                Lua.UnregisterFunction("SendMessageSystem");
                Lua.UnregisterFunction("SendMessageSystemString");
                Lua.UnregisterFunction("SendMessageSystemInt");
                s_registered = false;
                didIRegister = false;
            }
        }

        /// <summary>
        /// Sends a message to the MessageSystem with a parameter.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="parameter">Parameter.</param>
        public void SendMessageSystem(string message, string parameter)
        {
            MessageSystem.SendMessage(this, message, parameter);
        }

        /// <summary>
        /// Sends a message to the MessageSystem with a parameter and string value.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="parameter">Parameter.</param>
        /// <param name="value">String value.</param>
        public void SendMessageSystemString(string message, string parameter, string value)
        {
            MessageSystem.SendMessage(this, message, parameter, value);
        }

        /// <summary>
        /// Sends a message to the MessageSystem with a parameter and int value.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="parameter">Parameter.</param>
        /// <param name="value">Int value.</param>
        public void SendMessageSystemInt(string message, string parameter, double value)
        {
            MessageSystem.SendMessage(this, message, parameter, (int)value);
        }

    }
}