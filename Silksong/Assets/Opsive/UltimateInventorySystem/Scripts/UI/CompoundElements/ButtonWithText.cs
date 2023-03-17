/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.CompoundElements
{
    using System;
    using UnityEngine;
    using UnityEngine.Serialization;
    using UnityEngine.UI;
    using Text = Opsive.Shared.UI.Text;

    /// <summary>
    /// A simple component with a Text and Button component.
    /// </summary>
    [Serializable]
    public class ButtonWithText
    {
        [Tooltip("The button.")]
        [SerializeField] protected Button m_Button;
        [FormerlySerializedAs("m_CustomButton")]
        [Tooltip("The action button.")]
        [SerializeField] protected ActionButton m_ActionButton;
        [Tooltip("The text.")]
        [SerializeField] protected Text m_Text;

        public Button Button => m_Button;
        public ActionButton ActionButton => m_ActionButton;
        public Text Text => m_Text;

        /// <summary>
        /// Add a click event.
        /// </summary>
        /// <param name="clickEvent">The click event to add.</param>
        public void AddClickEvent(Action clickEvent)
        {
            if (clickEvent == null) { return; }

            if (m_Button != null) {
                m_Button.onClick.AddListener(clickEvent.Invoke);
            }

            if (m_ActionButton != null) {
                m_ActionButton.OnSubmitE += clickEvent.Invoke;
            }
        }

        /// <summary>
        /// Remove a click event.
        /// </summary>
        /// <param name="clickEvent">The click event to remove.</param>
        public void RemoveClickEvent(Action clickEvent)
        {
            if (clickEvent == null) { return; }

            if (m_Button != null) {
                m_Button.onClick.RemoveListener(clickEvent.Invoke);
            }

            if (m_ActionButton != null) {
                m_ActionButton.OnSubmitE -= clickEvent.Invoke;
            }
        }

        /// <summary>
        /// Enable or disable the button.
        /// </summary>
        /// <param name="text">Should the button be enabled or disabled?</param>
        public void EnableButton(bool enable)
        {
            if (m_Button != null) {
                m_Button.interactable = enable;
            }

            if (m_ActionButton != null) {
                m_ActionButton.interactable = enable;
            }
        }
    }
}