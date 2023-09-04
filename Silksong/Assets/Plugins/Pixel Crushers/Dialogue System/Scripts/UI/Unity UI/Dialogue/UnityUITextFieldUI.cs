// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.Events;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Unity UI text field UI implementation.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class UnityUITextFieldUI : MonoBehaviour, ITextFieldUI
    {

        /// <summary>
        /// The (optional) panel. If your text field UI contains more than a label and text field, you should
        /// assign the panel, too.
        /// </summary>
        [Tooltip("Optional panel containing the UI elements")]
        public UnityEngine.UI.Graphic panel;

        /// <summary>
        /// The label that will contain any label text prompting the user what to enter.
        /// </summary>
        [Tooltip("Optional text element for prompt")]
        public UnityEngine.UI.Text label;

        /// <summary>
        /// The text field.
        /// </summary>
        public UnityEngine.UI.InputField textField;

        /// <summary>
        /// The accept key.
        /// </summary>
        [Tooltip("Optional key code that accepts the input")]
        public KeyCode acceptKey = KeyCode.Return;

        /// <summary>
        /// The cancel key.
        /// </summary>
        [Tooltip("Optional key code that cancels the input")]
        public KeyCode cancelKey = KeyCode.Escape;

        [Tooltip("Automatically open touchscreen keyboard")]
        public bool showTouchScreenKeyboard = false;

        public UnityEvent onAccept = new UnityEvent();

        public UnityEvent onCancel = new UnityEvent();

        /// <summary>
        /// This delegate must be called when the player accepts the input in the text field.
        /// </summary>
        private AcceptedTextDelegate acceptedText = null;

        private bool isAwaitingInput = false;

        private TouchScreenKeyboard touchScreenKeyboard = null;

        private void Awake()
        {
            Tools.DeprecationWarning(this);
        }

        void Start()
        {
            if (DialogueDebug.logWarnings && (textField == null)) Debug.LogWarning(string.Format("{0}: No InputField is assigned to the text field UI {1}. TextInput() sequencer commands or [var?=] won't work.", new object[] { DialogueDebug.Prefix, name }));
            Hide();
        }

        /// <summary>
        /// Starts the text input field.
        /// </summary>
        /// <param name="labelText">The label text.</param>
        /// <param name="text">The current value to use for the input field.</param>
        /// <param name="maxLength">Max length, or <c>0</c> for unlimited.</param>
        /// <param name="acceptedText">The delegate to call when accepting text.</param>
        public void StartTextInput(string labelText, string text, int maxLength, AcceptedTextDelegate acceptedText)
        {
            if (label != null) label.text = labelText;
            if (textField != null)
            {
                textField.text = text;
                textField.characterLimit = maxLength;
            }
            this.acceptedText = acceptedText;
            Show();
            isAwaitingInput = true;
        }

        public void Update()
        {
            if (isAwaitingInput && !DialogueManager.IsDialogueSystemInputDisabled())
            {
                if (Input.GetKeyDown(acceptKey))
                {
                    AcceptTextInput();
                }
                else if (Input.GetKeyDown(cancelKey))
                {
                    CancelTextInput();
                }
            }
        }

        /// <summary>
        /// Cancels the text input field.
        /// </summary>
        public void CancelTextInput()
        {
            isAwaitingInput = false;
            Hide();
            onCancel.Invoke();
        }

        /// <summary>
        /// Accepts the text input and calls the accept handler delegate.
        /// </summary>
        public void AcceptTextInput()
        {
            isAwaitingInput = false;
            if (acceptedText != null)
            {
                if (textField != null) acceptedText(textField.text);
                acceptedText = null;
            }
            Hide();
            onAccept.Invoke();
        }

        private void Show()
        {
            SetActive(true);
            if (textField != null)
            {
                if (showTouchScreenKeyboard) touchScreenKeyboard = TouchScreenKeyboard.Open(textField.text);
                textField.ActivateInputField();
                if (UnityEngine.EventSystems.EventSystem.current != null)
                {
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(textField.gameObject, null);
                }
            }
        }

        private void Hide()
        {
            SetActive(false);
            if (touchScreenKeyboard != null)
            {
                touchScreenKeyboard.active = false;
                touchScreenKeyboard = null;
            }
        }

        private void SetActive(bool value)
        {
            if (textField != null) textField.enabled = value;
            if (panel != null)
            {
                Tools.SetGameObjectActive(panel, value);
            }
            else
            {
                Tools.SetGameObjectActive(label, value);
                Tools.SetGameObjectActive(textField, value);
            }
        }

    }

}
