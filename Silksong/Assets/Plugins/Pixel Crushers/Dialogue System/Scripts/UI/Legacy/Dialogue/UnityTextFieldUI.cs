using UnityEngine;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// This is a Unity GUI implementation of ITextFieldUI. It uses GUITextField for text input. It also supports a 
    /// panel that can contain the text field and other controls such as Ok and Cancel buttons. The Ok button should
    /// send an OnAccept message to this object. The Cancel button should send OnCancel.
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class UnityTextFieldUI : MonoBehaviour, ITextFieldUI
    {

        /// <summary>
        /// The (optional) panel. If your text field UI contains more than a label and text field, you should
        /// assign the panel, too.
        /// </summary>
        public GUIControl panel;

        /// <summary>
        /// The label that will contain any label text prompting the user what to enter.
        /// </summary>
        public GUILabel label;

        /// <summary>
        /// The text field.
        /// </summary>
        public GUITextField textField;

        /// <summary>
        /// The accept key.
        /// </summary>
        public KeyCode acceptKey = KeyCode.Return;

        /// <summary>
        /// The cancel key.
        /// </summary>
        public KeyCode cancelKey = KeyCode.None;

        private AcceptedTextDelegate acceptedText = null;

        private GUIControl control;

        /// <summary>
        /// If the text field starts with the accept key in the down position,
        /// we need to ignore the first accept key event. Otherwise it will
        /// immediately accept the input.
        /// </summary>
        private bool ignoreFirstAccept = false;

        /// <summary>
        /// If the text field starts with the cancel key in the down position,
        /// we need to ignore the first cancel key event. Otherwise it will
        /// immediately cancel the input.
        /// </summary>
        private bool ignoreFirstCancel = false;

        public void Awake()
        {
            control = GetComponent<GUIControl>();
            if (control == null) control = gameObject.AddComponent<GUIControl>();
            control.visible = false;
        }

        /// <summary>
        /// Sets up the text input controls.
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
                textField.maxLength = maxLength;
                textField.TakeFocus();
                ignoreFirstAccept = (acceptKey != KeyCode.None) && Input.GetKeyDown(acceptKey);
                ignoreFirstCancel = (cancelKey != KeyCode.None) && Input.GetKeyDown(cancelKey);
            }
            this.acceptedText = acceptedText;
            Show();
        }

        /// <summary>
        /// If the text field is active, this method handles accept/cancel keypresses.
        /// </summary>
        void OnGUI()
        {
            if (control.visible)
            {
                if (textField != null) textField.TakeFocus();
                if (IsAcceptKey())
                {
                    if (ignoreFirstAccept)
                    {
                        ignoreFirstAccept = false;
                    }
                    else
                    {
                        Event.current.Use();
                        AcceptTextInput();
                    }
                }
                else if (Event.current.isKey)
                {
                    if ((cancelKey != KeyCode.None) && (Event.current.keyCode == cancelKey))
                    {
                        if (ignoreFirstCancel)
                        {
                            ignoreFirstCancel = false;
                        }
                        else
                        {
                            Event.current.Use();
                            CancelTextInput();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method includes special handling for Enter/Return to handle Mac webplayers.
        /// </summary>
        /// <returns><c>true</c> if the current event is the accept key; otherwise, <c>false</c>.</returns>
        private bool IsAcceptKey()
        {
            if (IsKeyCodeReturn(acceptKey))
            {
                return
                    Event.current.Equals(Event.KeyboardEvent("[enter]")) ||
                    Event.current.Equals(Event.KeyboardEvent("return")) ||
                    (Event.current.isKey && (Event.current.keyCode == KeyCode.KeypadEnter)) ||
                    (Event.current.isKey && (Event.current.keyCode == KeyCode.Return)) ||
                        ((Event.current.type == EventType.KeyDown) && (Event.current.character == '\n'));
            }
            else
            {
                return (acceptKey != KeyCode.None) && (Event.current.keyCode == acceptKey);
            }
        }

        private bool IsKeyCodeReturn(KeyCode keyCode)
        {
            return (keyCode == KeyCode.KeypadEnter) || (keyCode == KeyCode.Return);
        }

        /// <summary>
        /// Cancels the text input by hiding the controls.
        /// </summary>
        /// <returns><c>true</c> if this instance cancel text input; otherwise, <c>false</c>.</returns>
        public void CancelTextInput()
        {
            Hide();
        }

        /// <summary>
        /// Accepts the text input and calls the accept handler delegate.
        /// </summary>
        private void AcceptTextInput()
        {
            Hide();
            if (acceptedText != null)
            {
                if (IsKeyCodeReturn(acceptKey)) textField.text = textField.text.Replace("\n", "");
                if (textField != null) acceptedText(textField.text);
                acceptedText = null;
            }
        }

        /// <summary>
        /// This is received from the Ok/Accept button if it exists. It simply accepts the text.
        /// </summary>
        public void OnAccept(object data)
        {
            AcceptTextInput();
        }

        /// <summary>
        /// This is received from the Cancel button if it exists. It cancels the input.
        /// </summary>
        public void OnCancel(object data)
        {
            CancelTextInput();
        }

        private void Show()
        {
            SetControlsActive(true);
        }

        private void Hide()
        {
            SetControlsActive(false);
        }

        private void SetControlsActive(bool value)
        {
            control.visible = value;
            UnityDialogueUIControls.SetControlActive(label, value);
            UnityDialogueUIControls.SetControlActive(textField, value);
            UnityDialogueUIControls.SetControlActive(panel, value);
        }

    }

}
