// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Controls for StandardDialogueUI's alert message.
    /// </summary>
    [Serializable]
    public class StandardUIAlertControls : AbstractUIAlertControls
    {

        [Tooltip("Main alert panel (optional).")]
        public UIPanel panel;

        [Tooltip("Alert text.")]
        public UITextField alertText;

        [Tooltip("Wait for previous alerts to finish before showing new alert; if unticked, new alerts replace old.")]
        public bool queueAlerts = false;

        [Tooltip("If a message is already queued to display, don't queue another.")]
        public bool dontQueueDuplicates = false;

        [Tooltip("Wait for the previous alert's Hide animation to finish before showing the next queued alert.")]
        public bool waitForHideAnimation = false;

        [Tooltip("If message contains [f], show immediately instead of queueing.")]
        public bool allowForceImmediate = false;

        /// <summary>
        /// Is an alert currently showing?
        /// </summary>
        public override bool isVisible { get { return (panel != null) ? panel.isOpen : (alertText != null && alertText.activeInHierarchy); } }

        /// <summary>
        /// Is the panel currently playing the Hide animation?
        /// </summary>
        public bool isHiding { get { return (panel != null && string.Equals(panel.animatorMonitor.currentTrigger, panel.hideAnimationTrigger)); } }

        private bool m_initializedAnimator = false;

        /// <summary>
        /// Sets the alert controls active.
        /// </summary>
        public override void SetActive(bool value)
        {
            if (panel != null)
            {
                if (!m_initializedAnimator && value == false)
                {
                    if (panel.deactivateOnHidden)
                    {
                        panel.gameObject.SetActive(false);
                    }
                }
                else
                {
                    panel.SetOpen(value);
                }
            }
            m_initializedAnimator = true;
            if (value == true || panel == null) alertText.SetActive(true);
        }

        /// <summary>
        /// Hide without playing animation.
        /// </summary>
        public void HideImmediate()
        {
            alertText.SetActive(false);
        }

        /// <summary>
        /// Sets the alert message UI Text.
        /// </summary>
        /// <param name='message'>Alert message.</param>
        /// <param name='duration'>Duration to show message.</param>
        public override void SetMessage(string message, float duration)
        {
            alertText.text = FormattedText.Parse(message).text;
        }

    }

}
