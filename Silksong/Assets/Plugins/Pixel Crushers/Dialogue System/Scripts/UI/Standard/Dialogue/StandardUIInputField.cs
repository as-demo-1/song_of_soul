// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// StandardDialogueUI input field implementation.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class StandardUIInputField : UIPanel, ITextFieldUI
    {

        [Tooltip("(Optional) Text field panel.")]
        public UnityEngine.UI.Graphic panel;

        [Tooltip("(Optional) Text element for prompt.")]
        public UITextField label;

        [Tooltip("Input field.")]
        public UIInputField inputField;

        [Tooltip("(Optional) Key code that accepts user's text input.")]
        public KeyCode acceptKey = KeyCode.Return;

        [Tooltip("(Optional) Input button that accepts user's text input.")]
        public string acceptButton = string.Empty;

        [Tooltip("(Optional) Key code that cancels user's text input.")]
        public KeyCode cancelKey = KeyCode.Escape;

        [Tooltip("(Optional) Input button that cancels user's text input.")]
        public string cancelButton = string.Empty;

        [Tooltip("Automatically open touchscreen keyboard.")]
        public bool showTouchScreenKeyboard = false;

        [Tooltip("Allow blank text input.")]
        public bool allowBlankInput = true;

        public UnityEvent onAccept = new UnityEvent();

        public UnityEvent onCancel = new UnityEvent();

        /// <summary>
        /// Call this delegate when the player accepts the input in the text field.
        /// </summary>
        protected AcceptedTextDelegate m_acceptedText = null;

        protected bool m_isAwaitingInput = false;

        protected TouchScreenKeyboard m_touchScreenKeyboard = null;

        protected bool m_isQuitting = false;

        protected virtual void OnApplicationQuit()
        {
            m_isQuitting = true;
        }

        protected override void Start()
        {
            if (DialogueDebug.logWarnings && (inputField == null)) Debug.LogWarning("Dialogue System: No InputField is assigned to the text field UI " + name + ". TextInput() sequencer commands or [var?=] won't work.");
            SetActive(false);
        }

        /// <summary>
        /// Starts the text input field.
        /// </summary>
        /// <param name="labelText">The label text.</param>
        /// <param name="text">The current value to use for the input field.</param>
        /// <param name="maxLength">Max length, or <c>0</c> for unlimited.</param>
        /// <param name="acceptedText">The delegate to call when accepting text.</param>
        public virtual void StartTextInput(string labelText, string text, int maxLength, AcceptedTextDelegate acceptedText)
        {
            if (label != null)
            {
                label.text = labelText;
            }
            if (inputField != null)
            {
                inputField.text = text;
                inputField.characterLimit = maxLength;
            }
            m_acceptedText = acceptedText;
            m_isAwaitingInput = true;
            Show();
        }

        protected override void Update()
        {
            if (m_isAwaitingInput && !DialogueManager.IsDialogueSystemInputDisabled())
            {
                if (InputDeviceManager.IsKeyDown(acceptKey) || InputDeviceManager.IsButtonDown(acceptButton) ||
                    (m_touchScreenKeyboard != null && m_touchScreenKeyboard.status == TouchScreenKeyboard.Status.Done))
                {
                    AcceptTextInput();
                }
                else if (InputDeviceManager.IsKeyDown(cancelKey) || InputDeviceManager.IsButtonDown(cancelButton) ||
                    (m_touchScreenKeyboard != null && m_touchScreenKeyboard.status == TouchScreenKeyboard.Status.Canceled))
                {
                    CancelTextInput();
                }
            }
        }

        /// <summary>
        /// Cancels the text input field.
        /// </summary>
        public virtual void CancelTextInput()
        {
            m_isAwaitingInput = false;
            Hide();
            onCancel.Invoke();
        }

        /// <summary>
        /// Accepts the text input and calls the accept handler delegate.
        /// </summary>
        public virtual void AcceptTextInput()
        {
            if (!allowBlankInput && string.IsNullOrEmpty(inputField.text)) return;
            m_isAwaitingInput = false;
            if (m_acceptedText != null)
            {
                if (inputField != null) m_acceptedText(inputField.text);
                m_acceptedText = null;
            }
            Hide();
            onAccept.Invoke();
        }

        protected virtual void Show()
        {
            SetActive(true);
            Open();
            if (showTouchScreenKeyboard) ShowTouchScreenKeyboard();
            if (inputField != null)
            {
                inputField.ActivateInputField();
                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(inputField.gameObject);
                }
            }
        }

        protected virtual void ShowTouchScreenKeyboard()
        { 
            m_touchScreenKeyboard = TouchScreenKeyboard.Open(inputField.text); 
        }

        protected virtual void Hide()
        {
            if (m_isQuitting) return;
            Close();
            SetActive(false);
            if (m_touchScreenKeyboard != null)
            {
                m_touchScreenKeyboard.active = false;
                m_touchScreenKeyboard = null;
            }
        }

        protected virtual void SetActive(bool value)
        {
            if (panel != null) panel.gameObject.SetActive(value);
            if (panel == null || value == true)
            {
                if (label != null) label.SetActive(value);
                if (inputField != null) inputField.SetActive(value);
            }
        }

    }

}
