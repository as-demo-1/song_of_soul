// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Abstract response menu controls. Each GUI system implementation derives its own subclass
    /// from this.
    /// </summary>
    [System.Serializable]
    public abstract class AbstractUIResponseMenuControls : AbstractUIControls
    {

        /// <summary>
        /// The response button alignment -- that is, whether to align them to the first or the
        /// last button. Defaults to the first button.
        /// </summary>
        public ResponseButtonAlignment buttonAlignment = ResponseButtonAlignment.ToFirst;

        /// <summary>
        /// Specifies whether to show buttons that aren't assigned to any responses. If you're
        /// using a "dialogue wheel," for example, you'll want to show unused buttons so the entire
        /// wheel structure is visible.
        /// </summary>
        public bool showUnusedButtons = false;

        /// <summary>
        /// Gets the subtitle reminder controls.
        /// </summary>
        /// <value>
        /// The subtitle reminder controls.
        /// </value>
        public abstract AbstractUISubtitleControls subtitleReminderControls { get; }

        /// <summary>
        /// Clears the response buttons.
        /// </summary>
        protected abstract void ClearResponseButtons();

        /// <summary>
        /// Sets the response buttons.
        /// </summary>
        /// <param name='responses'>
        /// Responses.
        /// </param>
        /// <param name='target'>
        /// Target that will receive OnClick events from the buttons.
        /// </param>
        protected abstract void SetResponseButtons(Response[] responses, Transform target);

        /// <summary>
        /// Starts the timer.
        /// </summary>
        /// <param name='timeout'>
        /// Timeout duration in seconds.
        /// </param>
        public abstract void StartTimer(float timeout);

        /// <summary>
        /// Shows the subtitle reminder and response buttons.
        /// </summary>
        /// <param name='subtitle'>
        /// Subtitle reminder.
        /// </param>
        /// <param name='responses'>
        /// Responses.
        /// </param>
        /// <param name='target'>
        /// Target that will receive OnClick events from the buttons.
        /// </param>
        public virtual void ShowResponses(Subtitle subtitle, Response[] responses, Transform target)
        {
            if ((responses != null) && (responses.Length > 0))
            {
                subtitleReminderControls.ShowSubtitle(subtitle);
                ClearResponseButtons();
                SetResponseButtons(responses, target);
                Show();
            }
            else
            {
                Hide();
            }
        }

        /// <summary>
        /// Sets the PC portrait name and sprite to use in the response menu.
        /// </summary>
        /// <param name="sprite">Portrait sprite.</param>
        /// <param name="portraitName">Portrait name.</param>
        public virtual void SetPCPortrait(Sprite sprite, string portraitName)
        {
        }

        [System.Obsolete("Use SetPCPortrait(Sprite,string) instead.")]
        public virtual void SetPCPortrait(Texture2D texture, string portraitName)
        {
        }
        /// <summary>
        /// Sets the portrait sprite to use in the response menu if the named actor is the player.
        /// </summary>
        /// <param name="actorName">Actor name in database.</param>
        /// <param name="sprite">Portrait sprite.</param>
        public virtual void SetActorPortraitSprite(string actorName, Sprite sprite)
        {
        }

        [System.Obsolete("Use SetActorPortraitSprite instead.")]
        public virtual void SetActorPortraitTexture(string actorName, Texture2D texture)
        {
        }

    }

}
