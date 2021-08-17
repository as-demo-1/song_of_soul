using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace Gamekit2D
{
    [Serializable]
    public class ScrollingTextBehaviour : PlayableBehaviour
    {
        public string message;
        public float startDelay;
        public float holdDelay;

        protected float m_Duration;
        protected float m_InverseScrollingDuration;
    
        public override void OnGraphStart (Playable playable)
        {
            m_Duration = (float)playable.GetDuration();
            float scrollingDuration = Mathf.Clamp(m_Duration - holdDelay - startDelay, float.Epsilon, m_Duration);
            m_InverseScrollingDuration = 1f / scrollingDuration;
        }

        public string GetMessage (float localTime)
        {
            localTime = Mathf.Clamp (localTime - startDelay, 0f, m_Duration);
            float messageProportion = Mathf.Clamp01(localTime * m_InverseScrollingDuration);
            int characterCount = Mathf.FloorToInt (message.Length * messageProportion);
            return message.Substring (0, characterCount);
        }
    }
}