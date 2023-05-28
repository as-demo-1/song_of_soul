// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Defines an input trigger using a key code and/or button name. The easiest way to bind a key
    /// to the trigger is to assign a key code. You can also assign a button name defined in
    /// UnityEngine.InputManager such as "Fire1" or a custom-defined button.
    /// </summary>
    [System.Serializable]
    public class InputTrigger
    {

        /// <summary>
        /// The key that fires the trigger.
        /// </summary>
        [Tooltip("This key fires the trigger.")]
        public KeyCode key = KeyCode.None;

        /// <summary>
        /// The name of the button defined in UnityEngine.InputManager that fires the trigger.
        /// </summary>
        [Tooltip("This button fires the trigger. The button name must be defined in your project's Input Settings.")]
        public string buttonName = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="PixelCrushers.DialogueSystem.InputTrigger"/> 
        /// class with no key code or button name.
        /// </summary>
        public InputTrigger() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PixelCrushers.DialogueSystem.InputTrigger"/> 
        /// class with a key code assigned.
        /// </summary>
        /// <param name='key'>
        /// Key that fires the trigger.
        /// </param>
        public InputTrigger(KeyCode key)
        {
            this.key = key;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PixelCrushers.DialogueSystem.InputTrigger"/> 
        /// class with a button name assigned.
        /// </summary>
        /// <param name='buttonName'>
        /// Name of the button defined in UnityEngine.InputManager that fires the trigger.
        /// </param>
        public InputTrigger(string buttonName)
        {
            this.buttonName = buttonName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PixelCrushers.DialogueSystem.InputTrigger"/> 
        /// class with a key code and button name assigned.
        /// </summary>
        /// <param name='key'>
        /// Key that fires the trigger.
        /// </param>
        /// <param name='buttonName'>
        /// Name of the button defined in UnityEngine.InputManager that fires the trigger.
        /// </param>
        public InputTrigger(KeyCode key, string buttonName)
        {
            this.key = key;
            this.buttonName = buttonName;
        }

        /// <summary>
        /// Gets a value indicating whether this input trigger has been triggered (i.e., the
        /// key or button is down).
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is triggered; otherwise, <c>false</c>.
        /// </value>
        public bool isDown
        {
            get
            {
                if (DialogueManager.IsDialogueSystemInputDisabled()) return false;
                return InputDeviceManager.IsKeyDown(key) ||
                    (!string.IsNullOrEmpty(buttonName) && DialogueManager.getInputButtonDown(buttonName));
            }
        }

        /// @cond FOR_V1_COMPATIBILITY
        public bool IsDown { get { return isDown; } }
        /// @endcond

    }

}
