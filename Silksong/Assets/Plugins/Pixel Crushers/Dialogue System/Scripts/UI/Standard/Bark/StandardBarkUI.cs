// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Standard UI implementation of IBarkUI.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class StandardBarkUI : AbstractBarkUI
    {

        /// <summary>
        /// The (optional) UI canvas group. If assigned, the fade will occur on this
        /// control. The other controls should be children of this canvas group.
        /// </summary>
        [Tooltip("Optional canvas group, for example to play fade animations.")]
        public CanvasGroup canvasGroup = null;

        /// <summary>
        /// The UI text control for the bark text.
        /// </summary>
        [Tooltip("UI text control for bark text.")]
        public UITextField barkText = null;

        /// <summary>
        /// The (optional) UI text control for the actor's name, if includeName is <c>true</c>.
        /// If <c>null</c>, the name is added to the front of the subtitle text instead.
        /// </summary>
        [Tooltip("Optional UI text control for barker's name if Include Name is ticked. If unassigned and Include Name is ticked, name is prepended to Bark Text.")]
        public UITextField nameText = null;

        /// <summary>
        /// Set <c>true</c> to include the barker's name in the text.
        /// </summary>
        [Tooltip("If Name Text is unassigned, prepend barker's name to Bark Text.")]
        public bool includeName = false;

        [Tooltip("Optional to show barker's portrait image.")]
        public UnityEngine.UI.Image portraitImage = null;

        [Tooltip("Show barker's portrait image.")]
        public bool showPortraitImage = false;

        [HideInInspector]
        public float doneTime = 0;

        [Serializable]
        public class AnimationTransitions
        {
            public string showTrigger = "Show";
            public string hideTrigger = "Hide";
        }

        public AnimationTransitions animationTransitions = new AnimationTransitions();

        /// <summary>
        /// The duration in seconds to show the bark text before fading it out.
        /// </summary>
        [Tooltip("The duration in seconds to show the bark text before fading it out. If zero, use the Dialogue Manager's Bark Settings.")]
        public float duration = 4f;

        [Tooltip("Keep bark canvas anchor point always in camera view.")]
        public bool keepInView = false;

        /// <summary>
        /// Set <c>true</c> to keep the bark text onscreen until the sequence ends.
        /// </summary>
        [Tooltip("Keep the bark text onscreen until the sequence ends.")]
        public bool waitUntilSequenceEnds = false;

        [Tooltip("If bark is visible and waiting for sequence to end, but new bark wants to show, cancel wait for previous sequence.")]
        public bool cancelWaitUntilSequenceEndsIfReplacingBark = false;

        /// <summary>
        /// Wait for an "OnContinue" message.
        /// </summary>
        [Tooltip("Wait for an OnContinue message.")]
        public bool waitForContinueButton = false;

        /// <summary>
        /// The text display setting. Defaults to use the same subtitle setting as the Dialogue
        /// Manager, but you can also set it to always show or always hide the text.
        /// </summary>
        public BarkSubtitleSetting textDisplaySetting = BarkSubtitleSetting.SameAsDialogueManager;

        protected Canvas canvas { get; set; }

        protected Animator animator { get; set; }

        protected AbstractTypewriterEffect typewriter { get; set; }

        protected Vector3 originalCanvasLocalPosition { get; set; }

        protected int numSequencesActive = 0;

        protected bool hasEverBarked = false;

        /// <summary>
        /// Indicates whether a bark is currently playing.
        /// </summary>
        /// <value>
        /// <c>true</c> if playing; otherwise, <c>false</c>.
        /// </value>
        public override bool isPlaying
        {
            get
            {
                return (canvas != null) && canvas.enabled && (DialogueTime.time < doneTime);
            }
        }

        protected virtual void Awake()
        {
            canvas = GetComponentInChildren<Canvas>();
            animator = GetComponentInChildren<Animator>();
            typewriter = TypewriterUtility.GetTypewriter(barkText);
            if ((animator == null) && (canvasGroup != null)) animator = canvasGroup.GetComponentInChildren<Animator>();
        }

        protected virtual void Start()
        {
            if (canvas != null)
            {
                if (waitForContinueButton && (canvas.worldCamera == null)) canvas.worldCamera = UnityEngine.Camera.main;
                canvas.enabled = false;
                originalCanvasLocalPosition = canvas.GetComponent<RectTransform>().localPosition;
            }
            if (nameText != null) nameText.SetActive(includeName);
            Tools.SetGameObjectActive(portraitImage, false);
        }

        protected virtual void Update()
        {
            if (!hasEverBarked) return;
            if (!waitUntilSequenceEnds && doneTime > 0 && DialogueTime.time >= doneTime)
            {
                Hide();
            }
            else if (keepInView && isPlaying)
            {
                var mainCamera = Camera.main;
                if (mainCamera == null) return;
                var pos = mainCamera.WorldToViewportPoint(canvas.transform.position);
                pos.x = Mathf.Clamp01(pos.x);
                pos.y = Mathf.Clamp01(pos.y);
                canvas.transform.position = mainCamera.ViewportToWorldPoint(pos);
            }
        }

        /// <summary>
        /// Indicates whether the bark UI should show the text, based on the 
        /// textDisplaySetting and subtitle.
        /// </summary>
        /// <value>
        /// <c>true</c> to show text; otherwise, <c>false</c>.
        /// </value>
        public virtual bool ShouldShowText(Subtitle subtitle)
        {
            bool settingsAllowShowText = (textDisplaySetting == BarkSubtitleSetting.Show) ||
                ((textDisplaySetting == BarkSubtitleSetting.SameAsDialogueManager) && DialogueManager.displaySettings.subtitleSettings.showNPCSubtitlesDuringLine);
            bool subtitleHasText = (subtitle != null) && !string.IsNullOrEmpty(subtitle.formattedText.text);
            return settingsAllowShowText && subtitleHasText;
        }

        /// <summary>
        /// Barks a subtitle. Does not observe formatting codes in the subtitle's FormattedText, 
        /// instead using the formatting settings defined on this component.
        /// </summary>
        /// <param name='subtitle'>
        /// Subtitle to bark.
        /// </param>
        public override void Bark(Subtitle subtitle)
        {
            if (ShouldShowText(subtitle))
            {
                hasEverBarked = true;
                SetUIElementsActive(false);
                string subtitleText = subtitle.formattedText.text;
                if (includeName)
                {
                    if (nameText != null)
                    {
                        nameText.text = subtitle.speakerInfo.Name;
                    }
                    else
                    {
                        subtitleText = string.Format("{0}: {1}", subtitleText, subtitle.formattedText.text);
                    }
                }
                else
                {
                    if (nameText != null && nameText.gameObject != null) nameText.gameObject.SetActive(false);
                }
                if (showPortraitImage && subtitle.speakerInfo.portrait != null)
                {
                    Tools.SetGameObjectActive(portraitImage, true);
                    portraitImage.sprite = subtitle.speakerInfo.portrait;
                }
                else
                {
                    Tools.SetGameObjectActive(portraitImage, false);
                }
                if (barkText != null) barkText.text = subtitleText;

                SetUIElementsActive(true);
                if (CanTriggerAnimations() && !string.IsNullOrEmpty(animationTransitions.showTrigger))
                {
                    if (!string.IsNullOrEmpty(animationTransitions.hideTrigger))
                    {
                        animator.ResetTrigger(animationTransitions.hideTrigger);
                    }
                    animator.SetTrigger(animationTransitions.showTrigger);
                }
                if (typewriter != null) typewriter.StartTyping(subtitleText);

                var barkDuration = Mathf.Approximately(0, duration) ? DialogueManager.GetBarkDuration(subtitleText) : duration;
                if (waitUntilSequenceEnds) numSequencesActive++;
                doneTime = waitForContinueButton ? Mathf.Infinity : (DialogueTime.time + barkDuration);
            }
        }

        protected virtual void SetUIElementsActive(bool value)
        {
            if (nameText.gameObject != this.gameObject && includeName) nameText.SetActive(value);
            if (barkText.gameObject != this.gameObject) barkText.SetActive(value);
            if (canvas != null && canvas.gameObject != this.gameObject) canvas.gameObject.SetActive(value);
            if (value == true && canvas != null) canvas.enabled = true;
        }

        public virtual void OnBarkEnd(Transform actor)
        {
            if (waitUntilSequenceEnds && !waitForContinueButton && IsActorMe(actor))
            {
                numSequencesActive--;
                if (numSequencesActive <= 0) Hide();
            }
        }

        protected virtual bool IsActorMe(Transform actor)
        {
            var t = transform;
            while (t != null)
            {
                if (t == actor) return true;
                t = t.parent;
            }
            return false;
        }

        public virtual void OnContinue()
        {
            Hide();
        }

        public override void Hide()
        {
            if (!hasEverBarked)
            {
                if (canvas != null) canvas.enabled = false;
                return;
            }
            numSequencesActive = 0;
            if (CanTriggerAnimations() && !string.IsNullOrEmpty(animationTransitions.hideTrigger))
            {
                if (!string.IsNullOrEmpty(animationTransitions.showTrigger))
                {
                    animator.ResetTrigger(animationTransitions.showTrigger);
                }
                animator.SetTrigger(animationTransitions.hideTrigger);
            }
            else if (canvas != null)
            {
                canvas.enabled = false;
            }
            if (canvas != null) canvas.GetComponent<RectTransform>().localPosition = originalCanvasLocalPosition;
            doneTime = 0;
        }

        protected virtual bool CanTriggerAnimations()
        {
            return (animator != null) && (animationTransitions != null);
        }

    }

}
