// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System;

namespace PixelCrushers.DialogueSystem
{

    [AddComponentMenu("")] // Use wrapper.
    public class StandardUISubtitlePanel : UIPanel
    {

        #region Serialized Fields

        [Tooltip("(Optional) Main panel for subtitle.")]
        public RectTransform panel;

        [Tooltip("(Optional) Image for actor's portrait.")]
        public UnityEngine.UI.Image portraitImage;

        [Tooltip("(Optional) Text element for actor's name.")]
        public UITextField portraitName;

        [Tooltip("Subtitle text.")]
        public UITextField subtitleText;

        [Tooltip("Add speaker's name to subtitle text.")]
        public bool addSpeakerName = false;

        [Tooltip("Format to add speaker name, where {0} is name and {1} is subtitle text.")]
        public string addSpeakerNameFormat = "{0}: {1}";

        [Tooltip("Each subtitle adds to Subtitle Text instead of replacing it.")]
        public bool accumulateText = false;

        [Tooltip("If Accumulate Text is ticked, accumulate up to this many lines, removing the oldest lines when over the limit.")]
        public int maxLines = 100;

        [Tooltip("If panel has a typewriter effect, don't start typing until panel's Show animation has completed.")]
        public bool delayTypewriterUntilOpen = false;

        [Tooltip("(Optional) Continue button. Only shown if Dialogue Manager's Continue Button mode uses continue button.")]
        public UnityEngine.UI.Button continueButton;

        [Tooltip("If non-zero, prevent continue button clicks for this duration in seconds when opening subtitle panel.")]
        public float blockInputDuration = 0;

        [Tooltip("When the subtitle UI elements should be visible.")]
        public UIVisibility visibility = UIVisibility.OnlyDuringContent;

        [Tooltip("When focusing panel, set this animator trigger.")]
        public string focusAnimationTrigger = string.Empty;

        [Tooltip("When unfocusing panel, set this animator trigger.")]
        public string unfocusAnimationTrigger = string.Empty;

        [Tooltip("If a player actor uses this panel, don't show player portrait name & image; keep previous NPC portrait visible instead.")]
        public bool onlyShowNPCPortraits = false;

        [Tooltip("Check Dialogue Actors for portrait animator controllers. Portrait image must have an Animator.")]
        public bool useAnimatedPortraits = false;

        [Tooltip("Set Portrait Image to actor portrait's native size. Image's Rect Transform can't use Stretch anchors.")]
        public bool usePortraitNativeSize = false;

        [Tooltip("Wait for panel state to be Open before showing subtitle.")]
        public bool waitForOpen = false;

        [Tooltip("Wait for panels within this dialogue UI (not external panels) to close before showing.")]
        public bool waitForClose = false;

        [Tooltip("Clear text when closing panel, including when hiding using SetDialoguePanel().")]
        public bool clearTextOnClose = true;

        [Tooltip("Clear text when any conversation starts.")]
        public bool clearTextOnConversationStart = false;

        [Tooltip("If Subtitle Text doesn't have a typewriter effect, to enable scroll to bottom add UIScrollbarEnabler to Scroll Rect and assign it here.")]
        public UIScrollbarEnabler scrollbarEnabler;

        /// <summary>
        /// Invoked when the subtitle panel gains focus.
        /// </summary>
        public UnityEvent onFocus = new UnityEvent();

        /// <summary>
        /// Invoked when the subtitle panel loses focus.
        /// </summary>
        public UnityEvent onUnfocus = new UnityEvent();

        #endregion

        #region Public Properties

        [SerializeField, Tooltip("Panel is currently in focused state.")]
        private bool m_hasFocus = true;
        public bool hasFocus
        {
            get { return m_hasFocus; }
            set { m_hasFocus = value; }
        }

        [SerializeField, Tooltip("Panel is playing the focus animation.")]
        private bool m_isFocusing = true;
        public bool isFocusing
        {
            get { return m_isFocusing; }
            set { m_isFocusing = value; }
        }

        public override bool waitForShowAnimation { get { return true; } }

        private Subtitle m_currentSubtitle = null;
        public virtual Subtitle currentSubtitle
        {
            get { return m_currentSubtitle; }
            protected set { m_currentSubtitle = value; }
        }

        /// <summary>
        /// The database name of the actor whose display name appears in the Portrait Name field.
        /// </summary>
        public string portraitActorName { get; protected set; }

        #endregion

        #region Internal Properties

        private bool m_haveSavedOriginalColor = false;
        protected bool haveSavedOriginalColor { get { return m_haveSavedOriginalColor; } set { m_haveSavedOriginalColor = value; } }
        protected Color originalColor { get; set; }
        private string m_accumulatedText = string.Empty;
        public string accumulatedText { get { return m_accumulatedText; } set { m_accumulatedText = value; } }
        protected int numAccumulatedLines = 0;

        private Animator m_portraitAnimator = null;
        protected virtual Animator animator { get { if (m_portraitAnimator == null && portraitImage != null) m_portraitAnimator = portraitImage.GetComponent<Animator>(); return m_portraitAnimator; } set { m_portraitAnimator = value; } }

        private Animator m_panelAnimator = null;

        private bool m_isDefaultNPCPanel = false;
        public bool isDefaultNPCPanel { get { return m_isDefaultNPCPanel; } set { m_isDefaultNPCPanel = value; } }
        private bool m_isDefaultPCPanel = false;
        public bool isDefaultPCPanel { get { return m_isDefaultPCPanel; } set { m_isDefaultPCPanel = value; } }
        private int m_panelNumber = -1;
        public int panelNumber { get { return m_panelNumber; } set { m_panelNumber = value; } }
        public Transform m_actorOverridingPanel = null;
        public Transform actorOverridingPanel { get { return m_actorOverridingPanel; } set { m_actorOverridingPanel = value; } }
        private int m_lastActorID = -1;
        protected int lastActorID { get { return m_lastActorID; } set { m_lastActorID = value; } }
        protected int frameLastSetContent = -1; // Frame when we last set this panel's content.
        protected bool shouldShowContinueButton = false;
        protected const float WaitForCloseTimeoutDuration = 8f;
        private StandardDialogueUI m_dialogueUI = null;
        public StandardDialogueUI dialogueUI
        {
            get
            {
                if (m_dialogueUI == null)
                { 
                    m_dialogueUI = GetComponentInParent<StandardDialogueUI>();
                    if (m_dialogueUI == null) m_dialogueUI = DialogueManager.dialogueUI as StandardDialogueUI;
                }
                return m_dialogueUI;
            }
            set
            {
                m_dialogueUI = value;
            }
        }

        protected Coroutine m_focusWhenOpenCoroutine = null;
        protected Coroutine m_showAfterClosingCoroutine = null;
        protected Coroutine m_setAnimatorCoroutine = null;

        #endregion

        #region Initialization

        protected virtual void Awake()
        {
            if (addSpeakerName)
            {
                addSpeakerNameFormat = addSpeakerNameFormat.Replace("\\n", "\n").Replace("\\t", "\t");
            }
            m_panelAnimator = GetComponent<Animator>();
        }

        #endregion

        #region Typewriter Control

        /// <summary>
        /// Returns the typewriter effect on the subtitle text element, or null if there is none.
        /// </summary>
        public AbstractTypewriterEffect GetTypewriter()
        {
            return TypewriterUtility.GetTypewriter(subtitleText);
        }

        /// <summary>
        /// Checks if the subtitle text element has a typewriter effect.
        /// </summary>
        public bool HasTypewriter()
        {
            return GetTypewriter() != null;
        }

        /// <summary>
        /// Returns the speed of the typewriter effect on the subtitle element if it has one.
        /// </summary>
        public float GetTypewriterSpeed()
        {
            return TypewriterUtility.GetTypewriterSpeed(subtitleText);
        }

        /// <summary>
        /// Sets the speed of the typewriter effect on the subtitle element if it has one.
        /// </summary>
        public void SetTypewriterSpeed(float charactersPerSecond)
        {
            TypewriterUtility.SetTypewriterSpeed(subtitleText, charactersPerSecond);
        }

        #endregion

        #region Show & Hide

        /// <summary>
        /// Shows the panel at the start of the conversation; called if it's configured to be visible at the start.
        /// </summary>
        /// <param name="portraitSprite">The image of the first actor who will use this panel.</param>
        /// <param name="portraitName">The name of the first actor who will use this panel.</param>
        /// <param name="dialogueActor">The actor's DialogueActor component, or null if none.</param>
        public virtual void OpenOnStartConversation(Sprite portraitSprite, string portraitName, DialogueActor dialogueActor)
        {
            Open();
            SetUIElementsActive(true);
            SetPortraitImage(portraitSprite);
            portraitActorName = (dialogueActor != null) ? dialogueActor.GetActorName() : portraitName;
            if (this.portraitName != null) this.portraitName.text = portraitActorName;
            if (subtitleText.text != null) subtitleText.text = string.Empty;
            CheckDialogueActorAnimator(dialogueActor);
        }

        [System.Obsolete("Use OpenOnStartConversation(Sprite,string,DialogueActor) instead.")]
        public virtual void OpenOnStartConversation(Texture2D portraitTexture, string portraitName, DialogueActor dialogueActor)
        {
            OpenOnStartConversation(UITools.CreateSprite(portraitTexture), portraitName, dialogueActor);
        }

        public virtual void OnConversationStart(Transform actor)
        {
            if (clearTextOnConversationStart && (frameLastSetContent < (Time.frameCount - 1))) // If we just set content, don't clear the text.
            {
                ClearText();
            }
        }

        /// <summary>
        /// Shows a subtitle, playing the open and focus animations.
        /// </summary>
        public virtual void ShowSubtitle(Subtitle subtitle)
        {
            var supercedeOnActorChange = waitForClose && isOpen && visibility == UIVisibility.UntilSupercededOrActorChange &&
                subtitle != null && lastActorID != subtitle.speakerInfo.id;
            if ((waitForClose && dialogueUI.AreAnyPanelsClosing(this)) || supercedeOnActorChange)
            {
                if (supercedeOnActorChange) Close();
                StopShowAfterClosingCoroutine();
                m_showAfterClosingCoroutine = DialogueManager.instance.StartCoroutine(ShowSubtitleAfterClosing(subtitle));
            }
            else
            {
                ShowSubtitleNow(subtitle);
            }
        }

        protected virtual void ShowSubtitleNow(Subtitle subtitle)
        {
            SetUIElementsActive(true);
            if (!isOpen)
            {
                hasFocus = false;
                isFocusing = false;
            }
            Open();
            Focus();
            SetContent(subtitle);
            actorOverridingPanel = null;
        }

        protected virtual IEnumerator ShowSubtitleAfterClosing(Subtitle subtitle)
        {
            shouldShowContinueButton = false;
            float safeguardTime = Time.realtimeSinceStartup + WaitForCloseTimeoutDuration;
            while (dialogueUI.AreAnyPanelsClosing() && Time.realtimeSinceStartup < safeguardTime)
            {
                yield return null;
            }
            ShowSubtitleNow(subtitle);
            if (shouldShowContinueButton) ShowContinueButton();
            m_showAfterClosingCoroutine = null;
        }

        protected virtual void StopShowAfterClosingCoroutine()
        {
            if (m_showAfterClosingCoroutine != null)
            {
                DialogueManager.instance.StopCoroutine(m_showAfterClosingCoroutine);
                m_showAfterClosingCoroutine = null;
            }
        }

        /// <summary>
        /// Hides a subtitle, playing the unfocus and close animations.
        /// </summary>
        public virtual void HideSubtitle(Subtitle subtitle)
        {
            if (panelState != PanelState.Closed) Unfocus();
            Close();
        }

        /// <summary>
        /// Immediately hides the panel without playing any animations.
        /// </summary>
        public virtual void HideImmediate()
        {
            OnHidden();
        }

        protected override void OnHidden()
        {
            base.OnHidden();
            if (clearTextOnClose) ClearText();
            if (deactivateOnHidden) DeactivateUIElements();
            currentSubtitle = null;
        }

        /// <summary>
        /// Opens the panel.
        /// </summary>
        public override void Open()
        {
            base.Open();
        }

        /// <summary>
        /// Closes the panel.
        /// </summary>
        public override void Close()
        {
            StopShowAfterClosingCoroutine();
            if (isOpen) base.Close();
            if (clearTextOnClose && !waitForClose) ClearText();
            hasFocus = false;
            isFocusing = false;
        }

        /// <summary>
        /// Focuses the panel.
        /// </summary>
        public virtual void Focus()
        {
            if (panelState == PanelState.Opening && enabled && gameObject.activeInHierarchy)
            {
                if (m_focusWhenOpenCoroutine != null) StopCoroutine(m_focusWhenOpenCoroutine);
                m_focusWhenOpenCoroutine = StartCoroutine(FocusWhenOpen());
            }
            else
            {
                FocusNow();
            }
        }

        protected IEnumerator FocusWhenOpen()
        {
            float timeout = Time.realtimeSinceStartup + 5f;
            while (panelState != PanelState.Open && Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }
            m_focusWhenOpenCoroutine = null;
            FocusNow();
        }

        protected virtual void FocusNow()
        {
            panelState = PanelState.Open;
            if (hasFocus) return;
            isFocusing = true;
            if (m_panelAnimator != null && !string.IsNullOrEmpty(unfocusAnimationTrigger)) m_panelAnimator.ResetTrigger(unfocusAnimationTrigger);
            if (string.IsNullOrEmpty(focusAnimationTrigger))
            {
                OnFocused();
            }
            else
            {
                animatorMonitor.SetTrigger(focusAnimationTrigger, OnFocused, true);
            }
            onFocus.Invoke();
        }

        private void OnFocused()
        {
            hasFocus = true;
            isFocusing = false;
        }

        /// <summary>
        /// Unfocuses the panel.
        /// </summary>
        public virtual void Unfocus()
        {
            if (m_panelAnimator != null && !string.IsNullOrEmpty(focusAnimationTrigger)) m_panelAnimator.ResetTrigger(focusAnimationTrigger);
            if (m_focusWhenOpenCoroutine != null)
            {
                StopCoroutine(m_focusWhenOpenCoroutine);
                m_focusWhenOpenCoroutine = null;
            }
            if (!string.IsNullOrEmpty(focusAnimationTrigger) && animatorMonitor.currentTrigger == focusAnimationTrigger)
            {
                animatorMonitor.CancelCurrentAnimation();
            }
            else
            {
                if (!(hasFocus || isFocusing))
                {
                    hasFocus = false;
                    isFocusing = false;
                    return;
                }
            }
            if (panelState == PanelState.Opening) panelState = PanelState.Open;
            hasFocus = false;
            animatorMonitor.SetTrigger(unfocusAnimationTrigger, null, false);
            onUnfocus.Invoke();
        }

        public virtual void ActivateUIElements()
        {
            SetUIElementsActive(true);
        }

        public virtual void DeactivateUIElements()
        {
            SetUIElementsActive(false);
            if (clearTextOnClose) ClearText();
        }

        protected virtual void SetUIElementsActive(bool value)
        {
            Tools.SetGameObjectActive(panel, value);
            Tools.SetGameObjectActive(portraitImage, value && portraitImage != null && portraitImage.sprite != null);
            portraitName.SetActive(value);
            subtitleText.SetActive(value);
            Tools.SetGameObjectActive(continueButton, false); // Let ConversationView determine if continueButton should be shown.
        }

        public virtual void ClearText()
        {
            m_accumulatedText = string.Empty;
            subtitleText.text = string.Empty;
            numAccumulatedLines = 0;
        }

        public virtual void ShowContinueButton()
        {
            if (blockInputDuration > 0)
            {
                DialogueManager.instance.StartCoroutine(ShowContinueButtonAfterBlockDuration());
            }
            else
            {
                ShowContinueButtonNow();
            }
        }

        protected virtual IEnumerator ShowContinueButtonAfterBlockDuration()
        {
            if (continueButton == null) yield break;
            continueButton.interactable = false;

            // Wait for panel to open, or timeout:
            var timeout = Time.realtimeSinceStartup + 10f;
            while (panelState != PanelState.Open && Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }

            yield return DialogueManager.instance.StartCoroutine(DialogueTime.WaitForSeconds(blockInputDuration));
            continueButton.interactable = true;
            ShowContinueButtonNow();
        }

        protected virtual void ShowContinueButtonNow()
        { 
            Tools.SetGameObjectActive(continueButton, true);
            if (InputDeviceManager.autoFocus) Select(); 
            if (continueButton != null && continueButton.onClick.GetPersistentEventCount() == 0)
            {
                continueButton.onClick.RemoveAllListeners();
                var fastForward = continueButton.GetComponent<StandardUIContinueButtonFastForward>();
                if (fastForward != null)
                {
                    continueButton.onClick.AddListener(fastForward.OnFastForward);
                }
                else
                {
                    continueButton.onClick.AddListener(OnContinue);
                }
            }
            shouldShowContinueButton = true;
        }

        public virtual void HideContinueButton()
        {
            Tools.SetGameObjectActive(continueButton, false);
        }

        /// <summary>
        /// Finishes the subtitle without hiding the panel. Called if the panel is configured to stay open.
        /// Hides the continue button and stops the typewriter effect.
        /// </summary>
        public virtual void FinishSubtitle()
        {
            HideContinueButton();
            var typewriter = GetTypewriter();
            if (typewriter != null && typewriter.isPlaying) typewriter.Stop();
        }

        /// <summary>
        /// Selects the panel's continue button (i.e., navigates to it).
        /// </summary>
        /// <param name="allowStealFocus">Select continue button even if another element is already selected.</param>
        public virtual void Select(bool allowStealFocus = true)
        {
            UITools.Select(continueButton, allowStealFocus);
        }

        /// <summary>
        /// The continue button should call this method to continue.
        /// </summary>
        public virtual void OnContinue()
        {
            if (dialogueUI != null) dialogueUI.OnContinueConversation();
        }

        /// <summary>
        /// Sets the content of the panel. Assumes the panel is already open.
        /// </summary>
        public virtual void SetContent(Subtitle subtitle)
        {
            if (subtitle == null) return;
            currentSubtitle = subtitle;
            lastActorID = subtitle.speakerInfo.id;
            CheckSubtitleAnimator(subtitle);
            if (!onlyShowNPCPortraits || subtitle.speakerInfo.isNPC)
            {                
                if (portraitImage != null)
                {
                    var sprite = subtitle.GetSpeakerPortrait();
                    SetPortraitImage(sprite);
                }
                portraitActorName = subtitle.speakerInfo.nameInDatabase;
                if (portraitName.text != subtitle.speakerInfo.Name)
                {
                    portraitName.text = subtitle.speakerInfo.Name;
                    UITools.SendTextChangeMessage(portraitName);
                }
            }

            if (waitForOpen && panelState != PanelState.Open)
            {
                DialogueManager.instance.StartCoroutine(SetSubtitleTextContentAfterOpen(subtitle));
            }
            else
            {
                SetSubtitleTextContent(subtitle);
            }
            frameLastSetContent = Time.frameCount;
        }

        protected virtual IEnumerator SetSubtitleTextContentAfterOpen(Subtitle subtitle)
        {
            float timeout = Time.realtimeSinceStartup + WaitForCloseTimeoutDuration;
            while (panelState != PanelState.Open && Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }
            SetSubtitleTextContent(subtitle);
        }

        protected virtual void SetSubtitleTextContent(Subtitle subtitle)
        {
            TypewriterUtility.StopTyping(subtitleText);
            var previousText = accumulateText ? m_accumulatedText : string.Empty;
            if (accumulateText && !string.IsNullOrEmpty(subtitle.formattedText.text))
            {
                if (numAccumulatedLines < maxLines)
                {
                    numAccumulatedLines++;
                }
                else
                {
                    // If we're at the max number of lines, remove the first line from the accumulated text:
                    previousText = previousText.Substring(previousText.IndexOf("\n") + 1);
                }
            }
            var previousChars = accumulateText ? UITools.StripRPGMakerCodes(Tools.StripTextMeshProTags(Tools.StripRichTextCodes(previousText))).Length : 0;
            SetFormattedText(subtitleText, previousText, subtitle.formattedText);
            if (accumulateText) m_accumulatedText = UITools.StripRPGMakerCodes(subtitleText.text) + "\n";
            if (scrollbarEnabler != null && !HasTypewriter())
            {
                scrollbarEnabler.CheckScrollbarWithResetValue(0);
            }
            else if (delayTypewriterUntilOpen && !hasFocus)
            {
                DialogueManager.instance.StartCoroutine(StartTypingWhenFocused(subtitleText, subtitleText.text, previousChars));
            }
            else
            {
                TypewriterUtility.StartTyping(subtitleText, subtitleText.text, previousChars);
            }
        }

        protected virtual IEnumerator StartTypingWhenFocused(UITextField subtitleText, string text, int fromIndex)
        {
            subtitleText.text = string.Empty;
            float timeout = Time.realtimeSinceStartup + 5f;
            while ((!hasFocus || panelState != PanelState.Open) && Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }
            subtitleText.text = text;
            TypewriterUtility.StartTyping(subtitleText, text, fromIndex);
        }

        protected virtual void SetFormattedText(UITextField textField, string previousText, FormattedText formattedText)
        {
            textField.text = previousText + UITools.GetUIFormattedText(formattedText);
            UITools.SendTextChangeMessage(textField);
            if (!haveSavedOriginalColor)
            {
                originalColor = textField.color;
                haveSavedOriginalColor = true;
            }
            textField.color = (formattedText.emphases != null && formattedText.emphases.Length > 0) ? formattedText.emphases[0].color : originalColor;
        }

        public virtual void SetActorPortraitSprite(string actorName, Sprite portraitSprite)
        {
            if (portraitImage == null) return;
            var sprite = AbstractDialogueUI.GetValidPortraitSprite(actorName, portraitSprite);
            SetPortraitImage(sprite);
        }

        protected virtual void SetPortraitImage(Sprite sprite)
        {
            if (portraitImage == null) return;
            Tools.SetGameObjectActive(portraitImage, sprite != null);
            portraitImage.sprite = sprite;
            if (usePortraitNativeSize && sprite != null)
            {
                portraitImage.rectTransform.sizeDelta = sprite.packed ?
                    new Vector2(sprite.rect.width, sprite.rect.height) :
                    new Vector2(sprite.texture.width, sprite.texture.height);
            }
        }

        public virtual void CheckSubtitleAnimator(Subtitle subtitle)
        {
            if (subtitle != null && useAnimatedPortraits && animator != null)
            {
                var dialogueActor = DialogueActor.GetDialogueActorComponent(subtitle.speakerInfo.transform);
                if (dialogueActor != null) // && dialogueActor.standardDialogueUISettings.portraitAnimatorController != null)
                {
                    var speakerPanelNumber = dialogueActor.GetSubtitlePanelNumber();
                    var isMyPanel =
                        (actorOverridingPanel == subtitle.speakerInfo.transform) ||
                        (PanelNumberUtility.GetSubtitlePanelIndex(speakerPanelNumber) == this.panelNumber) ||
                        (speakerPanelNumber == SubtitlePanelNumber.Default && subtitle.speakerInfo.isNPC && isDefaultNPCPanel) ||
                        (speakerPanelNumber == SubtitlePanelNumber.Default && subtitle.speakerInfo.isPlayer && isDefaultPCPanel) ||
                        (speakerPanelNumber == SubtitlePanelNumber.Custom && dialogueActor.standardDialogueUISettings.customSubtitlePanel == this);
                    if (isMyPanel)
                    {
                        if (m_setAnimatorCoroutine != null) DialogueManager.instance.StopCoroutine(m_setAnimatorCoroutine);
                        m_setAnimatorCoroutine = DialogueManager.instance.StartCoroutine(SetAnimatorAtEndOfFrame(dialogueActor.standardDialogueUISettings.portraitAnimatorController));
                    }
                }
                else
                {
                    if (m_setAnimatorCoroutine != null) DialogueManager.instance.StopCoroutine(m_setAnimatorCoroutine);
                    m_setAnimatorCoroutine = DialogueManager.instance.StartCoroutine(SetAnimatorAtEndOfFrame(null));
                }
            }
        }

        protected virtual void CheckDialogueActorAnimator(DialogueActor dialogueActor)
        {
            if (dialogueActor != null && useAnimatedPortraits && animator != null &&
                dialogueActor.standardDialogueUISettings.portraitAnimatorController != null)
            {
                if (m_setAnimatorCoroutine != null) DialogueManager.instance.StopCoroutine(m_setAnimatorCoroutine);
                m_setAnimatorCoroutine = DialogueManager.instance.StartCoroutine(SetAnimatorAtEndOfFrame(dialogueActor.standardDialogueUISettings.portraitAnimatorController));
            }
        }

        protected virtual IEnumerator SetAnimatorAtEndOfFrame(RuntimeAnimatorController animatorController)
        {
            if (animator == null) yield break;
            if (animator.runtimeAnimatorController != animatorController)
            {
                animator.runtimeAnimatorController = animatorController;
            }
            if (animatorController != null)
            {
                Tools.SetGameObjectActive(portraitImage, portraitImage.sprite != null);
            }
            yield return CoroutineUtility.endOfFrame;
            if (animator.runtimeAnimatorController != animatorController)
            {
                animator.runtimeAnimatorController = animatorController;
            }
            if (animatorController != null)
            {
                Tools.SetGameObjectActive(portraitImage, portraitImage.sprite != null);
            }
            animator.enabled = animatorController != null;
        }

        #endregion

    }
}
