using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    public class DropShadow : MonoBehaviour
    {
        public GameObject origin;
        public float originOffset = 0.4f;
        public float offset = -0.2f;
        public float maxHeight = 3.0f;

        public LayerMask levelLayermask;

        protected SpriteRenderer m_SpriteRenderer;
        protected Vector3 m_OriginalSize;
        protected ContactFilter2D m_ContactFilter;
        protected RaycastHit2D[] m_ContactCache = new RaycastHit2D[6];

        private void Awake()
        {
            m_SpriteRenderer = GetComponent<SpriteRenderer>();
            m_OriginalSize = transform.localScale;

            m_ContactFilter = new ContactFilter2D();
            m_ContactFilter.layerMask = levelLayermask;
            m_ContactFilter.useLayerMask = true;
        }

        void LateUpdate ()
        {
            int count = Physics2D.Raycast((Vector2)origin.transform.position + Vector2.up * originOffset, Vector2.down, m_ContactFilter, m_ContactCache);

            if(count > 0)
            {
                m_SpriteRenderer.enabled = true;
                transform.position = m_ContactCache[0].point + m_ContactCache[0].normal * offset;

                float height = Vector3.SqrMagnitude(origin.transform.position - transform.position);
                float ratio = Mathf.Clamp(1.0f - height / (maxHeight * maxHeight), 0.0f, 1.0f);

                transform.localScale = m_OriginalSize * ratio;
            }
            else
            {
                m_SpriteRenderer.enabled = false;
            }
        }
    }
}