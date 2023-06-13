// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Basic slider-based timer for response menus.
    /// </summary>
	[AddComponentMenu("")] // Use wrapper.
    public class UnityUITimer : MonoBehaviour
    {

        private UnityEngine.UI.Slider slider = null;

        private float startTime; // When the timer started.

        public virtual void Awake()
        {
            slider = GetComponent<UnityEngine.UI.Slider>();
            Tools.DeprecationWarning(this);
        }

        /// <summary>
        /// Called by the response menu. Starts the timer. Each tick, the UpdateTimeLeft
        /// method is called.
        /// </summary>
        /// <param name="duration">Duration in seconds.</param>
        /// <param name="timeoutHandler">Handler to invoke if the timer reaches zero.</param>
        public virtual void StartCountdown(float duration, System.Action timeoutHandler)
        {
            StartCoroutine(Countdown(duration, timeoutHandler));
        }

        private IEnumerator Countdown(float duration, System.Action timeoutHandler)
        {
            startTime = DialogueTime.time;
            float endTime = startTime + duration;
            while (DialogueTime.time < endTime)
            {
                float elapsed = DialogueTime.time - startTime;
                UpdateTimeLeft(Mathf.Clamp(1 - (elapsed / duration), 0, 1));
                yield return null;
            }
            if (timeoutHandler != null) timeoutHandler();
        }

        /// <summary>
        /// Adjusts the amount of time left.
        /// </summary>
        /// <param name="amountToSkip">Seconds to fast-forward the timer (or rewind the timer if negative).</param>
        public void SkipTime(float amountToSkip)
        {
            startTime -= amountToSkip;
        }

        /// <summary>
        /// Called each tick to update the timer display. The default method updates a UI slider.
        /// </summary>
        /// <param name="normalizedTimeLeft">1 at the start, 0 when the timer times out.</param>
        public virtual void UpdateTimeLeft(float normalizedTimeLeft)
        {
            if (slider == null) return;
            slider.value = normalizedTimeLeft;
        }

        public virtual void OnDisable()
        {
            StopAllCoroutines();
        }

    }

}
