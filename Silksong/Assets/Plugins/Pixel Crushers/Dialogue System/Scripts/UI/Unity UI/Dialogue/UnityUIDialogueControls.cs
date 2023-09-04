// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Contains all dialogue (conversation) controls for a Unity UI Dialogue UI.
    /// </summary>
    [System.Serializable]
    public class UnityUIDialogueControls : AbstractDialogueUIControls
    {

        /// <summary>
        /// The panel containing the dialogue controls. A panel is optional, but you may want one
        /// so you can include a background image, panel-wide effects, etc.
        /// </summary>
        [Tooltip("Panel containing the entire conversation UI")]
        public UnityEngine.UI.Graphic panel;

        [Tooltip("Optional animation transitions; panel should have an Animator")]
        public UIAnimationTransitions animationTransitions = new UIAnimationTransitions();

        /// <summary>
        /// The NPC subtitle controls.
        /// </summary>
        public UnityUISubtitleControls npcSubtitle;

        /// <summary>
        /// The PC subtitle controls.
        /// </summary>
        public UnityUISubtitleControls pcSubtitle;

        /// <summary>
        /// The response menu controls.
        /// </summary>
        public UnityUIResponseMenuControls responseMenu;

        public override AbstractUISubtitleControls npcSubtitleControls
        {
            get { return npcSubtitle; }
        }

        public override AbstractUISubtitleControls pcSubtitleControls
        {
            get { return pcSubtitle; }
        }

        public override AbstractUIResponseMenuControls responseMenuControls
        {
            get { return responseMenu; }
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

        public override void SetActive(bool value) // Must also play animation transitions to set active/inactive.
        {
            if (value == true)
            {
                ShowPanel();
            }
            else
            {
                HidePanel();
            }
        }

        public override void ShowPanel()
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
            //base.SetActive(true); // Don't show NPC, PC, Response Menu subpanels in case overrides supercede them.
        }

        public void DeactivateUIElements()
        {
            Tools.SetGameObjectActive(panel, false);
            base.SetActive(false); // Hides subpanels.
        }

    }

}
