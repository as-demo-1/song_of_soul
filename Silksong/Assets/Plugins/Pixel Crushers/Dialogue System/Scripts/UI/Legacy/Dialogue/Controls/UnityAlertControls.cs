using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// GUI controls for UnityDialogueUI's alert message.
    /// </summary>
    [System.Serializable]
    public class UnityAlertControls : AbstractUIAlertControls
    {

        /// <summary>
        /// The alert panel. A panel is optional, but you may want one
        /// so you can include a background image, panel-wide effects, etc.
        /// </summary>
        public GUIControl panel;

        /// <summary>
        /// The label used to show the alert message text.
        /// </summary>
        public GUILabel line;

        /// <summary>
        /// Optional continue button to close the alert immediately.
        /// </summary>
        public GUIButton continueButton;

        /// <summary>
        /// Is an alert currently showing?
        /// </summary>
        /// <value>
        /// <c>true</c> if showing; otherwise, <c>false</c>.
        /// </value>
        public override bool isVisible
        {
            get { return (line != null) && (line.gameObject.activeSelf); }
        }

        public override void SetActive(bool value)
        {
            UnityDialogueUIControls.SetControlActive(line, value);
            UnityDialogueUIControls.SetControlActive(panel, value);
        }

        public override void SetMessage(string message, float duration)
        {
            if (line != null)
            {
                line.SetFormattedText(FormattedText.Parse(message, DialogueManager.masterDatabase.emphasisSettings));
                SetFadeDuration(line.gameObject, duration);
                if (panel != null) SetFadeDuration(panel.gameObject, duration);
            }
        }

        private void SetFadeDuration(GameObject go, float duration)
        {
            if (go != null)
            {
                FadeEffect fadeEffect = go.GetComponent<FadeEffect>();
                if (fadeEffect != null)
                {
                    fadeEffect.SetFadeDurations(fadeEffect.fadeInDuration, duration, fadeEffect.fadeOutDuration);
                    m_alertDoneTime = Mathf.Max(m_alertDoneTime, DialogueTime.time + fadeEffect.fadeInDuration + duration + fadeEffect.fadeOutDuration);
                    if (go.activeInHierarchy)
                    {
                        fadeEffect.StopAllCoroutines();
                        fadeEffect.StartCoroutine(fadeEffect.Play());
                    }
                }
            }
        }

    }

}
