using UnityEngine;
using System;
using System.Collections;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// Applies a timer effect to a GUIProgressBar that counts down from 1 to 0. When time is up,
    /// it calls an event handler.
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class TimerEffect : GUIEffect
    {

        /// <summary>
        /// Occurs when the timer is done (i.e., at the end of duration). If the coroutine is
        /// stopped or the control is deactivated, this will never get called.
        /// </summary>
        public event Action TimeoutHandler = null;

        /// <summary>
        /// The timer duration.
        /// </summary>
        public float duration = 5f;

        private GUIProgressBar progressBar = null;

        /// <summary>
        /// Run the timer.
        /// </summary>
        public override IEnumerator Play()
        {
            progressBar = GetComponent<GUIProgressBar>();
            if (progressBar == null) yield break;
            float startTime = DialogueTime.time;
            float endTime = startTime + duration;
            while (DialogueTime.time < endTime)
            {
                float elapsed = DialogueTime.time - startTime;
                progressBar.progress = Mathf.Clamp(1 - (elapsed / duration), 0, 1);
                yield return null;
            }
            if (TimeoutHandler != null) TimeoutHandler();
        }

    }

}
