// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Response menu controls for UnityUIDialogueUI.
    /// </summary>
    [System.Serializable]
    public class UnityUIResponseMenuControls : AbstractUIResponseMenuControls
    {

        /// <summary>
        /// The panel containing the response menu controls. A panel is optional, but you may want one
        /// so you can include a background image, panel-wide effects, etc.
        /// </summary>
        [Tooltip("The panel containing the response menu controls. A panel is optional, but you may want one so you can include a background image, panel-wide effects, etc.")]
        public UnityEngine.UI.Graphic panel;

        /// <summary>
        /// The PC portrait image to show during the response menu.
        /// </summary>
        [Tooltip("The PC portrait image to show during the response menu.")]
        public UnityEngine.UI.Image pcImage;

        /// <summary>
        /// The label that will show the PC name.
        /// </summary>
        [Tooltip("The label that will show the PC name.")]
        public UnityEngine.UI.Text pcName;

        /// <summary>
        /// The reminder of the last subtitle.
        /// </summary>
        [Tooltip("The reminder of the last subtitle.")]
        public UnityUISubtitleControls subtitleReminder;

        /// <summary>
        /// The (optional) timer.
        /// </summary>
        [Tooltip("The (optional) timer.")]
        public UnityEngine.UI.Slider timer;

        /// <summary>
        /// If ticked, then select the currently-focused response on timeout.
        /// </summary>
        [Tooltip("Select the currently-focused response on timeout.")]
        public bool selectCurrentOnTimeout = false;

        /// <summary>
        /// The response buttons, if you want to specify buttons at design time.
        /// </summary>
        [Tooltip("Design-time positioned response buttons")]
        public UnityUIResponseButton[] buttons;

        [Tooltip("Template from which to instantiate response buttons; optional to use instead of positioned buttons above")]
        public UnityUIResponseButton buttonTemplate;

        [Tooltip("If using Button Template, instantiated buttons are parented under this GameObject")]
        public UnityEngine.UI.Graphic buttonTemplateHolder;

        [Tooltip("Optional scrollbar if the instantiated button holder is in a scroll rect")]
        public UnityEngine.UI.Scrollbar buttonTemplateScrollbar;

        [Tooltip("Reset the scroll bar to this value when preparing the response menu")]
        public float buttonTemplateScrollbarResetValue = 1;

        [Tooltip("Automatically set up explicit navigation for instantiated template buttons instead of using Automatic navigation")]
        public bool explicitNavigationForTemplateButtons = true;

        [Tooltip("If explicit navigation is enabled, loop around when navigating past end of menu")]
        public bool loopExplicitNavigation = false;

        public UIAutonumberSettings autonumber = new UIAutonumberSettings();

        public UIAnimationTransitions animationTransitions = new UIAnimationTransitions();

        public UnityEvent onContentChanged = new UnityEvent();

        /// <summary>
        /// The instantiated buttons. These are only valid during a specific response menu,
        /// and only if you're using templates. Each showing of the response menu clears 
        /// this list and re-populates it with new buttons.
        /// </summary>
        [HideInInspector]
        public List<GameObject> instantiatedButtons = new List<GameObject>();

        /// <summary>
        /// Assign this delegate if you want it to replace the default timeout handler.
        /// </summary>
        public System.Action TimeoutHandler = null;

        public bool isVisible
        {
            get
            {
                return (panel != null) ? panel.gameObject.activeInHierarchy : false;
            }
        }

        private UIShowHideController m_showHideController = null;
        public UIShowHideController showHideController
        {
            get
            {
                if (m_showHideController == null) 
				{
					m_showHideController = new UIShowHideController(null, panel, animationTransitions.transitionMode, animationTransitions.debug);
					m_showHideController.state = UIShowHideController.State.Hidden;
				}
                return m_showHideController;
            }
        }

        private UnityUITimer unityUITimer = null;
        private Sprite pcPortraitSprite = null;
        private string pcPortraitName = null;
        private Animator animator = null;
        private bool lookedForAnimator = false;

        /// <summary>
        /// Sets the PC portrait name and sprite to use in the response menu.
        /// </summary>
        /// <param name="portraitSprite">Portrait sprite.</param>
        /// <param name="portraitName">Portrait name.</param>
        public override void SetPCPortrait(Sprite portraitSprite, string portraitName)
        {
            pcPortraitSprite = portraitSprite;
            pcPortraitName = portraitName;
        }

        /// <summary>
        /// Sets the Portrait sprite to use in the response menu if the named actor is the player.
        /// This is used to immediately update the GUI control if the SetPortrait() sequencer 
        /// command changes the Portrait sprite.
        /// </summary>
        /// <param name="actorName">Actor name in database.</param>
        /// <param name="portraitSprite">Portrait sprite.</param>
        public override void SetActorPortraitSprite(string actorName, Sprite portraitSprite)
        {
            if (string.Equals(actorName, pcPortraitName))
            {
                var actorPortraitSprite = AbstractDialogueUI.GetValidPortraitSprite(actorName, portraitSprite);
                pcPortraitSprite = actorPortraitSprite;
                if ((pcImage != null) && (DialogueManager.masterDatabase.IsPlayer(actorName)))
                {
                    pcImage.sprite = actorPortraitSprite;
                }
            }
        }

        public override AbstractUISubtitleControls subtitleReminderControls
        {
            get { return subtitleReminder; }
        }

        /// <summary>
        /// Sets the controls active/inactive, except this method never activates the timer. If the
        /// UI's display settings specify a timeout, then the UI will call StartTimer() to manually
        /// activate the timer.
        /// </summary>
        /// <param name='value'>
        /// Value (<c>true</c> for active; otherwise inactive).
        /// </param>
        public override void SetActive(bool value)
        {
            subtitleReminder.SetActive(value && subtitleReminder.HasText);
            Tools.SetGameObjectActive(buttonTemplate, false);
            foreach (var button in buttons)
            {
                if (button != null)
                {
                    if (value == true)
                    {
                        Tools.SetGameObjectActive(button, button.visible);
                    }
                    else
                    {
                        button.clickable = false;
                    }
                }
            }
            Tools.SetGameObjectActive(timer, false);
            Tools.SetGameObjectActive(pcName, value);
            Tools.SetGameObjectActive(pcImage, value);
            if (value == true)
            {
                if ((pcImage != null) && (pcPortraitSprite != null)) pcImage.sprite = pcPortraitSprite;
                if ((pcName != null) && (pcPortraitName != null)) pcName.text = pcPortraitName;
                Tools.SetGameObjectActive(panel, true);

                animationTransitions.ClearTriggers(showHideController);
                showHideController.Show(animationTransitions.showTrigger, false, null);
                if (explicitNavigationForTemplateButtons) SetupTemplateButtonNavigation();
            }
            else
            {
                if (isVisible && CanTriggerAnimation(animationTransitions.hideTrigger))
                {
                    animationTransitions.ClearTriggers(showHideController);
                    showHideController.Hide(animationTransitions.hideTrigger, DeactivateUIElements);
                }
                else
                {
                    if (panel != null) Tools.SetGameObjectActive(panel, false);
                }
            }
        }

        private void DeactivateUIElements()
        {
            if (panel != null) Tools.SetGameObjectActive(panel, false);
            ClearResponseButtons();
        }

        /// <summary>
        /// Clears the response buttons.
        /// </summary>
        protected override void ClearResponseButtons()
        {
            DestroyInstantiatedButtons();
            if (buttons != null)
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (buttons[i] == null) continue;
                    buttons[i].Reset();
                    buttons[i].visible = showUnusedButtons;
                }
            }
        }

        /// <summary>
        /// Sets the response buttons.
        /// </summary>
        /// <param name='responses'>
        /// Responses.
        /// </param>
        /// <param name='target'>
        /// Target that will receive OnClick events from the buttons.
        /// </param>
        protected override void SetResponseButtons(Response[] responses, Transform target)
        {
            DestroyInstantiatedButtons();

            if ((buttons != null) && (responses != null))
            {

                int buttonNumber = 0;

                // Add explicitly-positioned buttons:
                for (int i = 0; i < responses.Length; i++)
                {
                    if (responses[i].formattedText.position != FormattedText.NoAssignedPosition)
                    {
                        int position = responses[i].formattedText.position;
                        if (0 <= position && position < buttons.Length && buttons[position] != null)
                        {
                            SetResponseButton(buttons[position], responses[i], target, buttonNumber++);
                        }
                        else
                        {
                            Debug.LogWarning(DialogueDebug.Prefix + ": Buttons list doesn't contain a button for position " + position);
                        }
                    }
                }

                if ((buttonTemplate != null) && (buttonTemplateHolder != null))
                {

                    // Reset scrollbar to top:
                    if (buttonTemplateScrollbar != null)
                    {
                        buttonTemplateScrollbar.value = buttonTemplateScrollbarResetValue;
                    }

                    // Instantiate buttons from template:
                    for (int i = 0; i < responses.Length; i++)
                    {
                        if (responses[i].formattedText.position != FormattedText.NoAssignedPosition) continue;
                        GameObject buttonGameObject = GameObject.Instantiate(buttonTemplate.gameObject) as GameObject;
                        if (buttonGameObject == null)
                        {
                            Debug.LogError(string.Format("{0}: Couldn't instantiate response button template", DialogueDebug.Prefix));
                        }
                        else
                        {
                            instantiatedButtons.Add(buttonGameObject);
                            buttonGameObject.transform.SetParent(buttonTemplateHolder.transform, false);
                            buttonGameObject.SetActive(true);
                            UnityUIResponseButton responseButton = buttonGameObject.GetComponent<UnityUIResponseButton>();
                            SetResponseButton(responseButton, responses[i], target, buttonNumber++);
                            if (responseButton != null) buttonGameObject.name = "Response: " + responseButton.Text;

                        }
                    }
                }
                else
                {

                    // Auto-position remaining buttons:
                    if (buttonAlignment == ResponseButtonAlignment.ToFirst)
                    {

                        // Align to first, so add in order to front:
                        for (int i = 0; i < Mathf.Min(buttons.Length, responses.Length); i++)
                        {
                            if (responses[i].formattedText.position == FormattedText.NoAssignedPosition)
                            {
                                int position = Mathf.Clamp(GetNextAvailableResponseButtonPosition(0, 1), 0, buttons.Length - 1);
                                SetResponseButton(buttons[position], responses[i], target, buttonNumber++);
                            }
                        }
                    }
                    else
                    {

                        // Align to last, so add in reverse order to back:
                        for (int i = Mathf.Min(buttons.Length, responses.Length) - 1; i >= 0; i--)
                        {
                            if (responses[i].formattedText.position == FormattedText.NoAssignedPosition)
                            {
                                int position = Mathf.Clamp(GetNextAvailableResponseButtonPosition(buttons.Length - 1, -1), 0, buttons.Length - 1);
                                SetResponseButton(buttons[position], responses[i], target, buttonNumber++);
                            }
                        }
                    }
                }
            }
            NotifyContentChanged();
        }

        private void SetResponseButton(UnityUIResponseButton button, Response response, Transform target, int buttonNumber)
        {
            if (button != null)
            {
                button.visible = true;
                button.clickable = response.enabled;
                button.target = target;
                if (response != null) button.SetFormattedText(response.formattedText);
                button.response = response;

                // Auto-number:
                if (autonumber.enabled)
                {
                    button.Text = string.Format(autonumber.format, buttonNumber + 1, button.Text);
                    var keyTrigger = button.GetComponent<UIButtonKeyTrigger>();
                    if (autonumber.regularNumberHotkeys)
                    {
                        if (keyTrigger == null) keyTrigger = button.gameObject.AddComponent<UIButtonKeyTrigger>();
                        keyTrigger.key = (KeyCode)((int)KeyCode.Alpha1 + buttonNumber);
                    }
                    if (autonumber.numpadHotkeys)
                    {
                        if (autonumber.regularNumberHotkeys || keyTrigger == null) keyTrigger = button.gameObject.AddComponent<UIButtonKeyTrigger>();
                        keyTrigger.key = (KeyCode)((int)KeyCode.Keypad1 + buttonNumber);
                    }
                }
            }
        }

        private int GetNextAvailableResponseButtonPosition(int start, int direction)
        {
            if (buttons != null)
            {
                int position = start;
                while ((0 <= position) && (position < buttons.Length))
                {
                    if (buttons[position].visible && buttons[position].response != null)
                    {
                        position += direction;
                    }
                    else
                    {
                        return position;
                    }
                }
            }
            return 5;
        }

        public void SetupTemplateButtonNavigation()
        {
            // Assumes buttons are active (since uses GetComponent), so call after activating panel.
            if (instantiatedButtons == null || instantiatedButtons.Count == 0) return;
            for (int i = 0; i < instantiatedButtons.Count; i++)
            {
                var button = instantiatedButtons[i].GetComponent<UnityUIResponseButton>().button;
                var above = (i == 0) ? (loopExplicitNavigation ? instantiatedButtons[instantiatedButtons.Count - 1].GetComponent<UnityUIResponseButton>().button : null)
                    : instantiatedButtons[i - 1].GetComponent<UnityUIResponseButton>().button;
                var below = (i == instantiatedButtons.Count - 1) ? (loopExplicitNavigation ? instantiatedButtons[0].GetComponent<UnityUIResponseButton>().button : null) 
                    : instantiatedButtons[i + 1].GetComponent<UnityUIResponseButton>().button;
                var navigation = new UnityEngine.UI.Navigation();

                navigation.mode = UnityEngine.UI.Navigation.Mode.Explicit;
                navigation.selectOnUp = above;
                navigation.selectOnLeft = above;
                navigation.selectOnDown = below;
                navigation.selectOnRight = below;
                button.navigation = navigation;
            }
        }

        public void DestroyInstantiatedButtons()
        {
            foreach (var instantiatedButton in instantiatedButtons)
            {
                GameObject.Destroy(instantiatedButton);
            }
            instantiatedButtons.Clear();
            NotifyContentChanged();
        }

        public void NotifyContentChanged()
        {
            onContentChanged.Invoke();
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        /// <param name='timeout'>
        /// Timeout duration in seconds.
        /// </param>
        public override void StartTimer(float timeout)
        {
            if (timer != null)
            {
                if (unityUITimer == null)
                {
                    Tools.SetGameObjectActive(timer, true);
                    unityUITimer = timer.GetComponent<UnityUITimer>();
                    if (unityUITimer == null) unityUITimer = timer.gameObject.AddComponent<UnityUITimer>();
                    Tools.SetGameObjectActive(timer, false);
                }
                if (unityUITimer != null)
                {
                    Tools.SetGameObjectActive(timer, true);
                    unityUITimer.StartCountdown(timeout, OnTimeout);
                }
                else
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: No UnityUITimer component found on timer", DialogueDebug.Prefix));
                }
            }
        }

        /// <summary>
        /// This method is called if the timer runs out. It selects the first response.
        /// </summary>
        public virtual void OnTimeout()
        {
            if (TimeoutHandler != null)
            {
                TimeoutHandler.Invoke();
            }
            else
            {
                DefaultTimeoutHandler();
            }
        }

        public void DefaultTimeoutHandler()
        {
            if (selectCurrentOnTimeout || DialogueManager.displaySettings.inputSettings.responseTimeoutAction == ResponseTimeoutAction.ChooseCurrentResponse)
            {
                var currentButton = (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null)
                    ? UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<UnityUIResponseButton>() : null;
                if (currentButton != null)
                {
                    currentButton.OnClick();
                    return;
                }
            }
            DialogueManager.instance.SendMessage(DialogueSystemMessages.OnConversationTimeout);
        }

        /// <summary>
        /// Auto-focuses the first response. Useful for gamepads.
        /// </summary>
        public void AutoFocus(GameObject lastSelection = null, bool allowStealFocus = true)
        {
            if (UnityEngine.EventSystems.EventSystem.current == null) return;
            var currentSelection = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            if (currentSelection == null)
            {
                currentSelection = lastSelection;
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(lastSelection);
            }

            // If anything is selected and we're not allowed to steal focus, stop:
            if (currentSelection != null && !allowStealFocus) return;

            // If a button is already auto-focused, keep it focused and stop there:
            if (instantiatedButtons.Find(x => x.gameObject == currentSelection) != null) return;
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null && buttons[i].gameObject == currentSelection) return;
            }

            if (instantiatedButtons.Count > 0)
            {
                // Otherwise try to focus an instantiated button:
                UITools.Select(instantiatedButtons[0].GetComponent<UnityEngine.UI.Button>(), allowStealFocus);
            }
            else
            {
                // Failing that, focus a designed button:
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (buttons[i] != null && buttons[i].clickable)
                    {
                        UITools.Select(buttons[i].button, allowStealFocus);
                        return;
                    }
                }
            }
        }

        private bool CanTriggerAnimation(string triggerName)
        {
            return CanTriggerAnimations() && !string.IsNullOrEmpty(triggerName);
        }

        private bool CanTriggerAnimations()
        {
            if ((animator == null) && !lookedForAnimator)
            {
                lookedForAnimator = true;
                if (panel != null) animator = panel.GetComponentInParent<Animator>();
            }
            return (animator != null) && (animationTransitions != null);
        }

    }

}
