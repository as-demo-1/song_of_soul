/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.Damageable
{
    using System;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// The flash settings when a character is hit.
    /// </summary>
    [Serializable]
    public class Flash
    {
        [Tooltip("If true the material will not flash.")]
        [SerializeField] protected bool m_Disable;
        [Tooltip("The flash color.")]
        [SerializeField] protected Color m_Color = Color.red;
        [Tooltip("The renderer which has the material to manipulate.")]
        [SerializeField] protected Renderer m_Renderer;
        [Tooltip("The material index on the renderer.")]
        [SerializeField] protected int m_MaterialIndex;
        [Tooltip("The number of flashes during the flash life time.")]
        [SerializeField] protected int m_Flashes = 3;

        private static readonly int s_BaseColor = Shader.PropertyToID("_BaseTint");

        protected Material m_Material;
        protected Color m_StartColor;

        /// <summary>
        /// Reset the color.
        /// </summary>
        public virtual void Reset()
        {
            if (m_Disable) { return; }

            if (m_Material == null) { return; }
            m_Material.SetColor(s_BaseColor, m_StartColor);
        }

        /// <summary>
        /// Flash the renderer for x seconds.
        /// </summary>
        /// <param name="lifeTime">The x seconds to flash for.</param>
        public virtual IEnumerator CoroutineIE(float lifeTime)
        {
            if (m_Disable) { yield break; }

            if (m_Material == null) {
                m_Material = m_Renderer.materials[m_MaterialIndex];
                if (m_Material.HasProperty(s_BaseColor) == false) {
                    Debug.LogWarning("Base color is missing.");
                    yield break;
                }
                m_StartColor = m_Material.GetColor(s_BaseColor);
            }

            var startTime = Time.time;
            var normalizedTime = 0f;
            while (normalizedTime < 1) {
                normalizedTime = (Time.time - startTime) / lifeTime;
                var t = normalizedTime * m_Flashes * Mathf.PI * 2;

                var multiplier = Mathf.Sin(t);
                var colorBlend = m_Color * multiplier + m_StartColor * (1 - multiplier);
                m_Material.SetColor(s_BaseColor, colorBlend);
                yield return null;
            }

            Reset();
        }
    }
}