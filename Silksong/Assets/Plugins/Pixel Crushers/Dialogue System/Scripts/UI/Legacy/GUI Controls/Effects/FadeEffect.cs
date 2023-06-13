using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// Applies a fade effect to a GUI control. This effect works by changing the alpha value of
    /// GUI style.
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class FadeEffect : GUIEffect
    {

        /// <summary>
        /// The duration to fade in.
        /// </summary>
        public float fadeInDuration = 0.5f;

        /// <summary>
        /// The duration to stay visible before fading out.
        /// </summary>
        public float duration = 1f;

        /// <summary>
        /// The duration to fade out. If zero, the control doesn't fade out.
        /// </summary>
        public float fadeOutDuration = 0.5f;

        private GUIVisibleControl control = null;

        public void SetFadeDurations(float fadeInDuration, float duration, float fadeOutDuration)
        {
            this.fadeInDuration = fadeInDuration;
            this.duration = duration;
            this.fadeOutDuration = fadeOutDuration;
        }

        /// <summary>
        /// Plays the fade effect.
        /// </summary>
        public override IEnumerator Play()
        {
            control = GetComponent<GUIVisibleControl>();
            if (control == null) yield break;

            // Fade in:
            float startTime = DialogueTime.time;
            float endTime = startTime + fadeInDuration;
            while (DialogueTime.time < endTime)
            {
                float elapsed = DialogueTime.time - startTime;
                control.Alpha = elapsed / fadeInDuration;
                yield return null;
            }
            control.Alpha = 1;

            // If no fade out, exit:
            if (Tools.ApproximatelyZero(fadeOutDuration)) yield break;

            // Visible duration:
            yield return StartCoroutine(DialogueTime.WaitForSeconds(duration)); // new WaitForSeconds(duration);

            // Fade out:
            startTime = DialogueTime.time;
            endTime = startTime + fadeOutDuration;
            while (DialogueTime.time < endTime)
            {
                float elapsed = DialogueTime.time - startTime;
                control.Alpha = 1 - (elapsed / fadeOutDuration);
                yield return null;
            }
            control.Alpha = 0;
        }

    }

}
