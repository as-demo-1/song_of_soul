// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Basic slider-based timer for response menus.
    /// </summary>
	[AddComponentMenu("")] // Use wrapper.
    public class StandardUITimer : MonoBehaviour
    {

        private UnityEngine.UI.Slider slider = null;

        private bool m_isCountingDown = false;
        private float m_startTime; // When the timer started.
        private float m_duration;
        private System.Action m_timeoutHandler;

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
            m_isCountingDown = true;
            m_startTime = DialogueTime.time;
            m_duration = duration;
            m_timeoutHandler = timeoutHandler;
        }

        protected virtual void Update()
        {
            if (m_isCountingDown)
            {
                float elapsed = DialogueTime.time - m_startTime;
                UpdateTimeLeft(Mathf.Clamp01(1 - (elapsed / m_duration)));
                if (elapsed >= m_duration)
                {
                    m_isCountingDown = false;
                    if (m_timeoutHandler != null) m_timeoutHandler();
                }
            }
        }

        public virtual void StopCountdown()
        {
            m_isCountingDown = false;
            m_timeoutHandler = null;
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
