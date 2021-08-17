using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    public class SpriteColorChange : MonoBehaviour {

        public Color newColor = Color.white;
        public float timer = 0.2f;

        Color m_InitialColor;
        SpriteRenderer m_SpriteRenderer;

        void OnEnable()
        {
            m_SpriteRenderer = GetComponent<SpriteRenderer>();
            m_InitialColor = m_SpriteRenderer.color;
            StartCoroutine ("ChangeColor");
        }

        IEnumerator ChangeColor()
        {
            m_SpriteRenderer.color = newColor;
            yield return new WaitForSeconds(timer);
            m_SpriteRenderer.color = m_InitialColor;
            this.enabled = false;
        }
    }
}