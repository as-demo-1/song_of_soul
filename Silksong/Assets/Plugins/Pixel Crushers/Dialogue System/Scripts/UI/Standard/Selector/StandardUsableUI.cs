// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Replaces the Selector/ProximitySelector's OnGUI method with a method
    /// that enables or disables UI controls.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class StandardUsableUI : AbstractUsableUI
    {

        /// <summary>
        /// The text for the name of the current selection.
        /// </summary>
        public UITextField nameText = null;

        /// <summary>
        /// The text for the use message (e.g., "Press spacebar to use").
        /// </summary>
        public UITextField useMessageText = null;

        public Color inRangeColor = Color.yellow;

        public Color outOfRangeColor = Color.gray;

        /// <summary>
        /// The graphic to show if the selection is in range.
        /// </summary>
        public UnityEngine.UI.Graphic reticleInRange = null;

        /// <summary>
        /// The graphic to show if the selection is out of range.
        /// </summary>
        public UnityEngine.UI.Graphic reticleOutOfRange = null;

        [Serializable]
        public class AnimationTransitions
        {
            public string showTrigger = "Show";
            public string hideTrigger = "Hide";
        }

        public AnimationTransitions animationTransitions = new AnimationTransitions();

        [Tooltip("You can leave this unassigned if the Canvas is on this GameObject.")]
        public Canvas canvas;

        protected Animator animator = null;

        public virtual void Awake()
        {
            if (canvas == null) canvas = GetComponent<Canvas>();
            animator = GetComponent<Animator>();
            if (animator == null && canvas != null) animator = canvas.GetComponent<Animator>();
        }

        public virtual void Start()
        {
            Usable usable = Tools.GetComponentAnywhere<Usable>(gameObject);
            if ((usable != null) && (nameText != null)) nameText.text = usable.GetName();
            if (canvas != null) canvas.enabled = false;
        }

        public override void Show(string useMessage)
        {
            if (canvas != null) canvas.enabled = true;
            if (useMessageText != null) useMessageText.text = DialogueManager.GetLocalizedText(useMessage);
            if (CanTriggerAnimations() && !string.IsNullOrEmpty(animationTransitions.showTrigger))
            {
                animator.SetTrigger(animationTransitions.showTrigger);
            }
        }

        public override void Hide()
        {
            if (CanTriggerAnimations() && !string.IsNullOrEmpty(animationTransitions.hideTrigger))
            {
                animator.SetTrigger(animationTransitions.hideTrigger);
            }
            else
            {
                if (canvas != null) canvas.enabled = false;
            }
        }

        public void OnBarkStart(Transform actor)
        {
            Hide();
        }

        public void OnConversationStart(Transform actor)
        {
            Hide();
        }

        public override void UpdateDisplay(bool inRange)
        {
            Color color = inRange ? inRangeColor : outOfRangeColor;
            if (nameText != null) nameText.color = color;
            if (useMessageText != null) useMessageText.color = color;
            Tools.SetGameObjectActive(reticleInRange, inRange);
            Tools.SetGameObjectActive(reticleOutOfRange, !inRange);
        }

        private bool CanTriggerAnimations()
        {
            return (animator != null) && (animationTransitions != null);
        }

    }

}
