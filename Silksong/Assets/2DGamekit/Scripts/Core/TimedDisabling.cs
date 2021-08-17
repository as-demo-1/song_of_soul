using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    public class TimedDisabling : MonoBehaviour
    {

        public float timeBeforeDisable = 1.0f;

        private float m_Timer = 0.0f;

        private void OnEnable()
        {
            m_Timer = timeBeforeDisable;
        }

        private void Update()
        {
            m_Timer -= Time.deltaTime;

            if(m_Timer < 0.0f)
            {
                gameObject.SetActive(false);
            }
        }
    }
}