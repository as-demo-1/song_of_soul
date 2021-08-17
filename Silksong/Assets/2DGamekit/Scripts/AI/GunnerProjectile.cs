using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    public class GunnerProjectile : MonoBehaviour
    {
        public Vector2 initialForce;
        public float timer = 1;
        public float fuse = 0.01f;
        public GameObject explosion;
        public float explosionTimer = 3;
        new Rigidbody2D rigidbody;

        protected GameObject m_HitEffect;

        void OnEnable()
        {
            rigidbody = GetComponent<Rigidbody2D>();
            Destroy(gameObject, timer);

            m_HitEffect = Instantiate(explosion);
            m_HitEffect.SetActive(false);
        }

        public void Destroy()
        {
            m_HitEffect.transform.position = transform.position;
            m_HitEffect.SetActive(true);
            GameObject.Destroy(m_HitEffect, explosionTimer);
            Destroy(gameObject);
        }

        void Start()
        {
            rigidbody.AddForce(initialForce);
        }
    }
}