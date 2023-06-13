using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// Contains all dialogue (conversation) controls for a Unity Dialogue UI.
    /// </summary>
    [System.Serializable]
    public class UnityDialogueControls : AbstractDialogueUIControls
    {

        /// <summary>
        /// The alert panel. A panel is optional, but you may want one
        /// so you can include a background image, panel-wide effects, etc.
        /// </summary>
        public GUIControl panel;

        /// <summary>
        /// The NPC subtitle controls.
        /// </summary>
        public UnitySubtitleControls npcSubtitle;

        /// <summary>
        /// The PC subtitle controls.
        /// </summary>
        public UnitySubtitleControls pcSubtitle;

        /// <summary>
        /// The response menu controls.
        /// </summary>
        public UnityResponseMenuControls responseMenu;

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

        public override void ShowPanel()
        {
            UnityDialogueUIControls.SetControlActive(panel, true);
        }

        public override void SetActive(bool value)
        {
            base.SetActive(value);
            UnityDialogueUIControls.SetControlActive(panel, value);
        }

    }

}
