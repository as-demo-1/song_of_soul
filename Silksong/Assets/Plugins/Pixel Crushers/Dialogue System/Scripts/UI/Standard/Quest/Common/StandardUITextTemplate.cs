// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Standard UI template for text.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class StandardUITextTemplate : StandardUIContentTemplate
    {

        [Tooltip("Text UI element.")]
        [SerializeField]
        private UITextField m_text;

        /// <summary>
        /// Text UI element.
        /// </summary>
        public UITextField text
        {
            get { return m_text; }
            set { m_text = value; }
        }

        public virtual void Awake()
        {
            if (UITextField.IsNull(text))
            {
                text.uiText = GetComponentInChildren<UnityEngine.UI.Text>();
                if (UITextField.IsNull(text) && Debug.isDebugBuild) Debug.LogError("Dialogue System: UI Text is unassigned.", this);
            }
        }

        /// <summary>
        /// Assigns a text string to the UI element.
        /// </summary>
        /// <param name="text">Text string.</param>
        public void Assign(string text)
        {
            name = text;
            this.text.text = text;
        }

    }
}
