// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Basic slider-based timer for response menus.
    /// </summary>
	[AddComponentMenu("")] // Use wrapper.
    public class StandardUITimer : MonoBehaviour
    {

        private UnityEngine.UI.Slider slider = null;

        private float m_startTime; // When the timer started.

        public virtual void Awake()
        {
            slider = GetComponent<UnityEngine.UI.Slider>();
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
            m_startTime = DialogueTime.time;
            float endTime = m_startTime + duration;
            while (DialogueTime.time < endTime)
            {
                float elapsed = DialogueTime.time - m_startTime;
                UpdateTimeLeft(Mathf.Clamp(1 - (elapsed / duration), 0, 1));
                yield return null;
            }
            if (timeoutHandler != null) timeoutHandler();
        }

        public virtual void StopCountdown()
        {
            StopAllCoroutines();
        }

        /// <summary>
        /// Adjusts the amount of time left.
        /// </summary>
        /// <param name="amountToSkip">Seconds to fast-forward the timer (or rewind the timer if negative).</param>
        public void SkipTime(float amountToSkip)
        {
            m_startTime -= amountToSkip;
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

    }

}
