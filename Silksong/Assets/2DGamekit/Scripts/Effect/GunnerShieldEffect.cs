using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    public class GunnerShieldEffect : MonoBehaviour
    {
        protected Material m_Material;
        protected float m_Intensity = 0.0f;

        const int count = 2;

        private void Start()
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            m_Material = renderer.material;
            m_Intensity = 0.0f;
        }

        public void ShieldHit(Damager damager, Damageable damageable)
        {
            Vector3 localPosition = transform.InverseTransformPoint(damager.transform.position);

            m_Material.SetVector("_HitPosition", localPosition);
            m_Intensity = 1.0f;
        }

        private void Update()
        {
            if(m_Intensity > 0.0f)
            {
                m_Intensity = Mathf.Clamp(m_Intensity - Time.deltaTime, 0, 1);
            }

            m_Material.SetFloat("_HitIntensity", m_Intensity);
        }
    }
}