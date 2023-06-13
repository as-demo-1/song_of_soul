// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Controls for UnityUIDialogueUI's alert message.
    /// </summary>
    [System.Serializable]
    public class UnityUIAlertControls : AbstractUIAlertControls
    {

        /// <summary>
        /// The panel containing the alert controls. A panel is optional, but you may want one
        /// so you can include a background image, panel-wide effects, etc.
        /// </summary>
        [Tooltip("Optional panel containing the alert line; can contain other doodads and effects, too")]
        public UnityEngine.UI.Graphic panel;

        /// <summary>
        /// The label used to show the alert message text.
        /// </summary>
        [Tooltip("Shows the alert message text")]
        public UnityEngine.UI.Text line;

        /// <summary>
        /// Optional continue button to close the alert immediately.
        /// </summary>
        [Tooltip("Optional continue button; configure OnClick to invoke dialogue UI's OnContinue method")]
        public UnityEngine.UI.Button continueButton;

        [Tooltip("Wait for previous alerts to finish before showing new alert; if unticked, new alerts replace old")]
        public bool queueAlerts = false;

        [Tooltip("Wait for the previous alert's Hide animation to finish before showing the next queued alert")]
        public bool waitForHideAnimation = false;

        [Tooltip("Optional animation transitions; panel should have an Animator")]
        public UIAnimationTransitions animationTransitions = new UIAnimationTransitions();

        /// <summary>
        /// Is an alert currently showing?
        /// </summary>
        /// <value>
        /// <c>true</c> if showing; otherwise, <c>false</c>.
        /// </value>
        public override bool isVisible { get { return showHideController.state != UIShowHideController.State.Hidden; } }

        public bool IsHiding { get { return showHideController.state == UIShowHideController.State.Hiding; } }

        private UIShowHideController m_showHideController = null;
        private UIShowHideController showHideController
        {
            get
            {
                if (m_showHideController == null) m_showHideController = new UIShowHideController(null, panel, animationTransitions.transitionMode, animationTransitions.debug);
                return m_showHideController;
            }
        }

        /// <summary>
        /// Sets the alert controls active. If a hide animation is available, this method
        /// depends on the hide animation to hide the controls.
        /// </summary>
        /// <param name='value'>
        /// <c>true</c> for active.
        /// </param>
        public override void SetActive(bool value)
        {
            if (value == true)
            {
                if (showHideController.state != UIShowHideController.State.Showing) ShowPanel();
            }
            else
            {
                if (showHideController.state != UIShowHideController.State.Hiding) HidePanel();
            }
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
            Tools.SetGameObjectActive(panel, true);
            Tools.SetGameObjectActive(line, true);
        }

        public void DeactivateUIElements()
        {
            Tools.SetGameObjectActive(panel, false);
            Tools.SetGameObjectActive(line, false);
        }

        /// <summary>
        /// Sets the alert message UI Text.
        /// </summary>
        /// <param name='message'>
        /// Alert message.
        /// </param>
        /// <param name='duration'>
        /// Duration to show message.
        /// </param>
        public override void SetMessage(string message, float duration)
        {
            if (line != null) line.text = FormattedText.Parse(message, DialogueManager.masterDatabase.emphasisSettings).text;
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
