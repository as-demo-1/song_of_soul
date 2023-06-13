// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.EventSystems;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Response button for use with Standard Dialogue UI. Add this component to every
    /// response button in the dialogue UI.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class StandardUIResponseButton : MonoBehaviour, ISelectHandler
    {

        [HelpBox("If Button's OnClick() event is empty, this Standard UI Response Button component will automatically assign its OnClick method at runtime. If Button's OnClick() event has other elements, you *must* manually assign the StandardUIResponseButton.OnClick method to it.", HelpBoxMessageType.Info)]
        public UnityEngine.UI.Button button;

        [Tooltip("Text element to display response text.")]
        public UITextField label;

        [Tooltip("Apply emphasis tag colors to button text.")]
        public bool setLabelColor = true;

        [Tooltip("Set button's text to this color by default.")]
        public Color defaultColor = Color.white;

        /// <summary>
        /// Gets or sets the response text element.
        /// </summary>
        public virtual string text
        {
            get
            {
                return label.text;
            }
            set
            {
                label.text = UITools.StripRPGMakerCodes(value);
                UITools.SendTextChangeMessage(label);
            }
        }

        /// <summary>
        /// Indicates whether the button is an allowable response.
        /// </summary>
        public virtual bool isClickable
        {
            get { return (button != null) && button.interactable; }
            set { if (button != null) button.interactable = value; }
        }

        /// <summary>
        /// Indicates whether the button is shown or not.
        /// </summary>
        public virtual bool isVisible { get; set; }

        /// <summary>
        /// Gets or sets the response associated with this button. If the player clicks this 
        /// button, this response is sent back to the dialogue system.
        /// </summary>
        public virtual Response response { get; set; }

        /// <summary>
        /// Gets or sets the target that will receive click notifications.
        /// </summary>
        public virtual Transform target { get; set; }


        /// <summary>
        /// Clears the button.
        /// </summary>
        public virtual void Reset()
        {
            isClickable = false;
            isVisible = false;
            response = null;
            if (label != null)
            {
                label.text = string.Empty;
                SetColor(defaultColor);
            }
        }

        public virtual void Awake()
        {
            if (button == null) button = GetComponent<UnityEngine.UI.Button>();
            if (button == null) Debug.LogWarning("Dialogue System: Response button '" + name + "' is missing a Unity UI Button component!", this);
        }

        public virtual void Start()
        {
            if (button != null && button.onClick.GetPersistentEventCount() == 0)
            {
                button.onClick.AddListener(OnClick);
            }
        }

        /// <summary>
        /// Sets the button's text using the specified formatted text.
        /// </summary>
        public virtual void SetFormattedText(FormattedText formattedText)
        {
            if (formattedText == null) return;
            text = UITools.GetUIFormattedText(formattedText);
            SetColor((formattedText.emphases.Length > 0) ? formattedText.emphases[0].color : defaultColor);
        }

        /// <summary>
        /// Sets the button's text using plain text.
        /// </summary>
        public virtual void SetUnformattedText(string unformattedText)
        {
            text = unformattedText;
            SetColor(defaultColor);
        }

        protected virtual void SetColor(Color currentColor)
        {
            if (setLabelColor) label.color = currentColor;
        }

        /// <summary>
        /// Handles a button click by calling the response handler.
        /// </summary>
        public virtual void OnClick()
        {
            if (target != null)
            {
                SetCurrentResponse();
                target.SendMessage("OnClick", response, SendMessageOptions.RequireReceiver);
            }
        }

        public virtual void OnSelect(BaseEventData eventData)
        {
            SetCurrentResponse();
        }

        protected virtual void SetCurrentResponse()
        {
            if (DialogueManager.instance.conversationController != null)
            {
                DialogueManager.instance.conversationController.SetCurrentResponse(response);
            }
        }
    }

}
