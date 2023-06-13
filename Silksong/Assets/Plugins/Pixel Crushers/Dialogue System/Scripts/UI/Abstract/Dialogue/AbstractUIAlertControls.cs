// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Abstract alert message controls. Each GUI system implementation derives its own subclass
    /// from this.
    /// </summary>
    [System.Serializable]
    public abstract class AbstractUIAlertControls : AbstractUIControls
    {

        /// <summary>
        /// Gets a value indicating whether an alert is visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if visible; otherwise, <c>false</c>.
        /// </value>
        public abstract bool isVisible { get; }

        /// <summary>
        /// Sets the message text of an alert.
        /// </summary>
        /// <param name='message'>
        /// Message.
        /// </param>
        /// <param name='duration'>
        /// Duration that message will be shown. Used by subclasses to set up fade durations.
        /// </param>
        public abstract void SetMessage(string message, float duration);

        protected float m_alertDoneTime = 0;

        /// <summary>
        /// Has the duration passed for the currently-showing alert?
        /// </summary>
        /// <value>
        /// <c>true</c> if done; otherwise, <c>false</c>.
        /// </value>
        public virtual bool isDone
        {
            get { return (DialogueTime.time > m_alertDoneTime); }
        }

        /// @cond FOR_V1_COMPATIBILITY
        public bool IsVisible { get { return isVisible; } }
        public bool IsDone { get { return isDone; } }
        /// @endcond

        /// <summary>
        /// Sets the GUI controls and shows a message.
        /// </summary>
        /// <param name='message'>
        /// Message to show.
        /// </param>
        /// <param name='duration'>
        /// Duration in seconds.
        /// </param>
        public virtual void ShowMessage(string message, float duration)
        {
            if (!string.IsNullOrEmpty(message))
            {
                m_alertDoneTime = (duration >= 0) ? DialogueTime.time + duration : Mathf.Infinity; // Negative durations show forever.
                SetMessage(message, duration);
                Show();
            }
            else
            {
                Hide();
            }
        }

    }

}
