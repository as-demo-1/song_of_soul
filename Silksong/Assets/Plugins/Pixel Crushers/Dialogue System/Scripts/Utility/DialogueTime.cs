// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// A static wrapper class for the built-in Time class. This class allows the user to specify
    /// whether the dialogue system functions in realtime, gameplay time, or a custom time. If the 
    /// game is paused during conversations by setting <c>Time.timeScale = 0</c>, then the Dialogue 
    /// System should use realtime or it will also be paused. However, if you want the Dialogue 
    /// System to observe the timeScale, then you can use gameplay time (for example, if you want 
    /// the Sequencer to observe timeScale). If you want to manage time on your own, set the mode
    /// to Custom and manually set DialogueTime.time every frame.
    /// </summary>
    public static class DialogueTime
    {

        /// <summary>
        /// Dialogue System time mode.
        /// </summary>
        public enum TimeMode
        {

            /// <summary>
            /// Ignore Time.timeScale. Internally, use Time.realtimeSinceStartup.
            /// </summary>
            Realtime,

            /// <summary>
            /// Observe Time.timeScale. Internally, use Time.time.
            /// </summary>
            Gameplay,

            /// <summary>
            /// Your code must manually manage the time.
            /// </summary>
            Custom
        }

        /// <summary>
        /// Gets or sets the time mode. Setting this also sets GameTime.mode.
        /// </summary>
        /// <value>
        /// The mode.
        /// </value>
        public static TimeMode mode
        {
            get { return m_mode; }
            set
            {
                m_mode = value;
                switch (value)
                {
                    case TimeMode.Realtime:
                        GameTime.mode = GameTimeMode.Realtime;
                        break;
                    case TimeMode.Gameplay:
                        GameTime.mode = GameTimeMode.UnityStandard;
                        break;
                    case TimeMode.Custom:
                        GameTime.mode = GameTimeMode.Manual;
                        break;
                }
            }
        }
        private static TimeMode m_mode = TimeMode.Realtime;

        /// <summary>
        /// Gets the time based on the current Mode.
        /// </summary>
        /// <value>
        /// The time.
        /// </value>
        public static float time
        {
            get
            {
                switch (mode)
                {
                    default:
                    case TimeMode.Realtime:
                        return (m_isPaused ? s_realtimeWhenPaused : Time.realtimeSinceStartup) - s_totalRealtimePaused;
                    case TimeMode.Gameplay:
                        return Time.time;
                    case TimeMode.Custom:
                        return m_customTime;
                }
            }
            set
            {
                m_customTime = value;
                GameTime.time = value;
            }
        }

        /// <summary>
        /// The frame delta time based on the current time mode.
        /// </summary>
        public static float deltaTime
        {
            get
            {
                switch (mode)
                {
                    default:
                    case TimeMode.Realtime:
                        return m_isPaused ? 0 : Time.unscaledDeltaTime;
                    case TimeMode.Gameplay:
                        return Time.deltaTime;
                    case TimeMode.Custom:
                        return m_customDeltaTime;
                }
            }
            set
            {
                m_customDeltaTime = value;
            }
        }

        public static bool isPaused
        {
            get
            {
                switch (mode)
                {
                    default:
                    case TimeMode.Realtime:
                    case TimeMode.Custom:
                        return m_isPaused;
                    case TimeMode.Gameplay:
                        return Tools.ApproximatelyZero(Time.timeScale);
                }
            }
            set
            {
                switch (mode)
                {
                    case TimeMode.Realtime:
                        if (!m_isPaused && value)
                        {
                            // Pausing, so record realtime at time of pause:
                            s_realtimeWhenPaused = Time.realtimeSinceStartup;
                        }
                        else if (m_isPaused && !value)
                        {
                            // Unpausing, so add to totalRealtimePaused:
                            s_totalRealtimePaused += Time.realtimeSinceStartup - s_realtimeWhenPaused;
                        }
                        break;
                    case TimeMode.Gameplay:
                        Time.timeScale = m_isPaused ? 0 : 1;
                        break;
                    case TimeMode.Custom:
                        break;
                }
                m_isPaused = value;
                GameTime.isPaused = value;
            }
        }

        /// @cond FOR_V1_COMPATIBILITY
        public static TimeMode Mode { get { return mode; } set { mode = value; } }
        public static bool IsPaused { get { return isPaused; } set { isPaused = value; } }
        /// @endcond

        private static float m_customTime = 0;

        private static float m_customDeltaTime = 0;

        private static bool m_isPaused = false;

        private static float s_realtimeWhenPaused = 0;

        private static float s_totalRealtimePaused = 0;

        /// <summary>
        /// Initializes the <see cref="PixelCrushers.DialogueSystem.DialogueTime"/> class.
        /// </summary>
        static DialogueTime()
        {
            mode = TimeMode.Realtime;
        }

        /// <summary>
        /// This version of WaitForSeconds respects DialogueTime.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static IEnumerator WaitForSeconds(float seconds)
        {
            float start = DialogueTime.time;
            while (DialogueTime.time < start + seconds)
            {
                yield return null;
            }
        }

    }

}