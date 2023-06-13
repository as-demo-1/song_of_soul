// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Abstract dialogue user interface controls. This class collects the various control groups
    /// used by the conversation interface. Each GUI system implementation derives its own subclass
    /// from this.
    /// </summary>
    [System.Serializable]
    public abstract class AbstractDialogueUIControls : AbstractUIControls
    {

        /// <summary>
        /// Gets the NPC subtitle controls.
        /// </summary>
        /// <value>
        /// The NPC subtitle controls.
        /// </value>
        public abstract AbstractUISubtitleControls npcSubtitleControls { get; }

        /// <summary>
        /// Gets the PC subtitle controls.
        /// </summary>
        /// <value>
        /// The PC subtitle controls.
        /// </value>
        public abstract AbstractUISubtitleControls pcSubtitleControls { get; }

        /// <summary>
        /// Gets the response menu.
        /// </summary>
        /// <value>
        /// The response menu.
        /// </value>
        public abstract AbstractUIResponseMenuControls responseMenuControls { get; }

        /// <summary>
        /// Shows the main conversation panel, if assigned.
        /// </summary>
        public abstract void ShowPanel();

        /// <summary>
        /// Sets the conversation controls active/inactive.
        /// </summary>
        /// <param name='value'>
        /// <c>true</c> for active, <c>false</c> for inactive.
        /// </param>
        public override void SetActive(bool value)
        {
            npcSubtitleControls.SetActive(value);
            pcSubtitleControls.SetActive(value);
            responseMenuControls.SetActive(value);
        }

    }

}
