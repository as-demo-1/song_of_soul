// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Subtitle Unity UI controls for UnityUIDialogueUI.
    /// </summary>
    [System.Serializable]
    public class UnityUISubtitleControls : AbstractUISubtitleControls
    {

        /// <summary>
        /// The panel containing the response menu controls. A panel is optional, but you may want one
        /// so you can include a background image, panel-wide effects, etc.
        /// </summary>
        [Tooltip("Optional panel for the subtitle elements")]
        public UnityEngine.UI.Graphic panel;

        /// <summary>
        /// The label that will show the text of the subtitle.
        /// </summary>
        [Tooltip("Subtitle text")]
        public UnityEngine.UI.Text line;

        /// <summary>
        /// The label that will show the portrait image.
        /// </summary>
        [Tooltip("Optional image for speaker's portrait")]
        public UnityEngine.UI.Image portraitImage;

        /// <summary>
        /// The label that will show the name of the speaker.
        /// </summary>
        [Tooltip("Optional label for speaker's name")]
        public UnityEngine.UI.Text portraitName;

        /// <summary>
        /// The continue button. This is only required if DisplaySettings.waitForContinueButton 
        /// is <c>true</c> -- in which case this button should send "OnContinue" to the UI when clicked.
        /// </summary>
        [Tooltip("Optional continue button; configure OnClick to invoke dialogue UI's OnContinue method")]
        public UnityEngine.UI.Button continueButton;

        [Tooltip("Ignore RPGMaker-style pause codes")]
        public bool ignorePauseCodes = false;

        [Tooltip("Optional animation transitions; panel should have an Animator")]
        public UIAnimationTransitions animationTransitions = new UIAnimationTransitions();

        [Tooltip("When the subtitle UI elements should be visible.")]
        public UIVisibility uiVisibility = UIVisibility.OnlyDuringContent;

        public bool isVisible
        {
            get
            {
                return (panel != null) ? panel.gameObject.activeInHierarchy : (line != null && line.gameObject.activeInHierarchy);
            }
        }

        /// <summary>
        /// Indicates whether this subtitle is currently assigned text.
        /// </summary>
        /// <value>
        /// <c>true</c> if it has text; otherwise, <c>false</c>.
        /// </value>
        public override bool hasText
        {
            get { return (line != null) && !string.IsNullOrEmpty(line.text); }
        }

        private UIShowHideController m_showHideController = null;
        private UIShowHideController showHideController
        {
            get
            {
                if (m_showHideController == null) m_showHideController = new UIShowHideController(null, panel, animationTransitions.transitionMode, animationTransitions.debug);
                return m_showHideController;
            }
        }

        private bool m_haveSavedOriginalColor = false;
        private Color m_originalColor = Color.white;

        // Called by UnityUIDialogueUI.Open. If visibility is AlwaysFromStart, set the portrait info.
        public void CheckSubtitlePortrait(CharacterType characterType)
        {
            if (uiVisibility == UIVisibility.AlwaysFromStart)
            {
                DialogueManager.instance.StartCoroutine(SetSubtitlePortrait(characterType));
            }
        }

        private IEnumerator SetSubtitlePortrait(CharacterType characterType)
        {
            // Need to wait until end of frame:
            if (portraitName != null) portraitName.text = string.Empty;
            if (portraitImage != null) portraitImage.sprite = null;
            if (line != null) line.text = string.Empty;
            yield return CoroutineUtility.endOfFrame;
            var characterInfo = (characterType == CharacterType.NPC) ? DialogueManager.conversationModel.conversantInfo : DialogueManager.conversationModel.actorInfo;
            if (characterInfo != null)
            {
                if (portraitName != null && string.IsNullOrEmpty(portraitName.text)) portraitName.text = characterInfo.Name;
                if (portraitImage != null && portraitImage.sprite == null) portraitImage.sprite = characterInfo.portrait;
            }
        }

        public override void SetActive(bool value)
        {
            if (value == true || uiVisibility == UIVisibility.AlwaysFromStart || ((uiVisibility == UIVisibility.AlwaysOnceShown || uiVisibility == UIVisibility.UntilSuperceded) && isVisible))
            {
                ShowPanel();
            }
            else
            {
                HidePanel();
            }
        }

        public void ForceHide()
        {
            HidePanel();
        }

        public void ForceShow()
        {
            showHideController.state = UIShowHideController.State.Hidden;
            ActivateUIElements();
        }

        private void ShowPanel()
        {
            ActivateUIElements();
            animationTransitions.ClearTriggers(showHideController);
            showHideController.Show(animationTransitions.showTrigger, false, null);
        }

        private void HidePanel()
        {
            animationTransitions.ClearTriggers(showHideController);
            showHideController.Hide(animationTransitions.hideTrigger, DeactivateUIElements);
        }

        public void ActivateUIElements()
        {
            SetUIElementsActive(true);
        }

        public void DeactivateUIElements()
        {
            SetUIElementsActive(false);
        }

        private void SetUIElementsActive(bool value)
        {
            Tools.SetGameObjectActive(panel, value);
            Tools.SetGameObjectActive(line, value);
            Tools.SetGameObjectActive(portraitImage, value);
            Tools.SetGameObjectActive(portraitName, value);
            Tools.SetGameObjectActive(continueButton, false); // Let ConversationView determine if continueButton should be shown.
        }

        public override void ShowContinueButton()
        {
            Tools.SetGameObjectActive(continueButton, true);
        }

        public override void HideContinueButton()
        {
            Tools.SetGameObjectActive(continueButton, false);
        }

        /// <summary>
        /// Sets the subtitle.
        /// </summary>
        /// <param name='subtitle'>
        /// Subtitle.
        /// </param>
        public override void SetSubtitle(Subtitle subtitle)
        {
            if ((subtitle != null) && !string.IsNullOrEmpty(subtitle.formattedText.text))
            {
                if (portraitImage != null) portraitImage.sprite = subtitle.GetSpeakerPortrait();
                if (portraitName != null)
                {
                    portraitName.text = subtitle.speakerInfo.Name;
                    UITools.SendTextChangeMessage(portraitName);
                }
                if (line != null)
                {
                    var typewriterEffect = line.GetComponent<UnityUITypewriterEffect>();
                    if (typewriterEffect != null && typewriterEffect.enabled)
                    {
                        typewriterEffect.Stop();
                        typewriterEffect.playOnEnable = false;
                    }
                    SetFormattedText(line, subtitle.formattedText);
                    if (typewriterEffect != null && typewriterEffect.enabled) typewriterEffect.PlayText(subtitle.formattedText.text);
                }
            }
            else
            {
                if ((line != null) && (subtitle != null)) SetFormattedText(line, subtitle.formattedText);
            }
        }

        /// <summary>
        /// Clears the subtitle.
        /// </summary>
        public override void ClearSubtitle()
        {
            SetFormattedText(line, null);
        }

        /// <summary>
        /// Sets a label with formatted text.
        /// </summary>
        /// <param name='label'>
        /// Label to set.
        /// </param>
        /// <param name='formattedText'>
        /// Formatted text.
        /// </param>
        private void SetFormattedText(UnityEngine.UI.Text label, FormattedText formattedText)
        {
            if (label != null)
            {
                if (formattedText != null)
                {
                    var text = UITools.GetUIFormattedText(formattedText);
                    if (ignorePauseCodes) text = UITools.StripRPGMakerCodes(text);
                    label.text = text;
                    UITools.SendTextChangeMessage(label);
                    if (!m_haveSavedOriginalColor)
                    {
                        m_originalColor = label.color;
                        m_haveSavedOriginalColor = true;
                    }
                    label.color = (formattedText.emphases.Length > 0) ? formattedText.emphases[0].color : m_originalColor;
                }
                else
                {
                    label.text = string.Empty;
                }
            }
        }

        /// <summary>
        /// Sets the portrait sprite to use in the subtitle if the named actor is the speaker.
        /// This is used to immediately update the GUI control if the SetPortrait() sequencer 
        /// command changes the portrait sprite.
        /// </summary>
        /// <param name="actorName">Actor name in database.</param>
        /// <param name="portraitSprite">Portrait sprite.</param>
        public override void SetActorPortraitSprite(string actorName, Sprite portraitSprite)
        {
            if ((currentSubtitle != null) && string.Equals(currentSubtitle.speakerInfo.nameInDatabase, actorName))
            {
                if (portraitImage != null) portraitImage.sprite = AbstractDialogueUI.GetValidPortraitSprite(actorName, portraitSprite);
            }
        }

        /// <summary>
        /// Auto-focuses the continue button. Useful for gamepads.
        /// </summary>
        public void AutoFocus(bool allowStealFocus = true)
        {
            UITools.Select(continueButton, allowStealFocus);
        }

    }

}
