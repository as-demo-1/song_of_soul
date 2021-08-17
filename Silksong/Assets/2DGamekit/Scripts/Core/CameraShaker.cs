using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    public class CameraShaker : MonoBehaviour
    {
        static protected CameraShaker s_Instance = null;


        protected Vector3 m_LastVector;
        protected float m_SinceShakeTime = 0.0f;
        protected float m_ShakeIntensity = 0.2f;

        private void OnEnable()
        {
            s_Instance = this;
        }

        private void OnPreRender()
        {
            if (m_SinceShakeTime > 0.0f)
            {
                m_LastVector = Random.insideUnitCircle * m_ShakeIntensity;
                transform.localPosition = transform.localPosition + m_LastVector;
            }
        }

        private void OnPostRender()
        {
            if (m_SinceShakeTime > 0.0f)
            {
                transform.localPosition = transform.localPosition - m_LastVector;
                m_SinceShakeTime -= Time.deltaTime;
            }
        }

    

        static public void Shake(float amount, float time)
        {
            if (s_Instance == null)
                return;

            s_Instance.m_ShakeIntensity = amount;
            s_Instance.m_SinceShakeTime = time;
        }
    }
}