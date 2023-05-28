// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Unity UI template for a button.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class StandardUIButtonTemplate : StandardUIContentTemplate
    {

        [Tooltip("Button UI element.")]
        public UnityEngine.UI.Button button;

        public UITextField label;

        private void Awake()
        {
            if (button == null && DialogueDebug.logWarnings) Debug.LogError("Dialogue System: UI Button is unassigned.", this);
        }

        public void Assign(string labelText)
        {
            label.text = labelText;
        }

    }
}
