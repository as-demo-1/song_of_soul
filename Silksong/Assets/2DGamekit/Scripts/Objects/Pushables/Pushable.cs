using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Pushable : MonoBehaviour
    {
        static ContactPoint2D[] s_ContactPointBuffer = new ContactPoint2D[16];
        static Dictionary<Collider2D, Pushable> s_PushableCache = new Dictionary<Collider2D, Pushable> ();

        public Transform playerPushingRightPosition;
        public Transform playerPushingLeftPosition;
        public Transform pushablePosition;

        public AudioSource pushableAudioSource;
        public AudioClip startingPushClip;
        public AudioClip loopPushClip;
        public AudioClip endPushClip;

        public bool Grounded {  get { return m_Grounded; } }

        protected SpriteRenderer m_SpriteRenderer;
        protected Rigidbody2D m_Rigidbody2D;
        protected bool m_Grounded;
        Collider2D[] m_WaterColliders;

        void Awake ()
        {
            m_SpriteRenderer = GetComponent<SpriteRenderer>();
            m_Rigidbody2D = GetComponent<Rigidbody2D> ();

            if (s_PushableCache.Count == 0)
            {
                Pushable[] pushables = FindObjectsOfType<Pushable> ();

                for (int i = 0; i < pushables.Length; i++)
                {
                    Collider2D[] pushableColliders = pushables[i].GetComponents<Collider2D> ();

                    for (int j = 0; j < pushableColliders.Length; j++)
                    {
                        s_PushableCache.Add (pushableColliders[j], pushables[i]);
                    }
                }
            }

            WaterArea[] waterAreas = FindObjectsOfType<WaterArea> ();
            m_WaterColliders = new Collider2D[waterAreas.Length];
            for (int i = 0; i < waterAreas.Length; i++)
            {
                m_WaterColliders[i] = waterAreas[i].GetComponent<Collider2D> ();
            }
        }

        void FixedUpdate ()
        {
            Vector2 velocity = m_Rigidbody2D.velocity;
            velocity.x = 0f;
            m_Rigidbody2D.velocity = velocity;

            CheckGrounded();

            for (int i = 0; i < m_WaterColliders.Length; i++)
            {
                if (m_Rigidbody2D.IsTouching (m_WaterColliders[i]))
                {
                    m_Rigidbody2D.constraints |= RigidbodyConstraints2D.FreezePositionX;
                }
            }
        }

        public void StartPushing()
        {
            pushableAudioSource.loop = false;
            pushableAudioSource.clip = startingPushClip;
            pushableAudioSource.Play();
        }

        public void EndPushing()
        {
            pushableAudioSource.loop = false;
            pushableAudioSource.clip = endPushClip;
            pushableAudioSource.Play();
        }

        public void Move (Vector2 movement)
        {
            m_Rigidbody2D.position = m_Rigidbody2D.position + movement;

            if(!pushableAudioSource.isPlaying)
            {
                pushableAudioSource.clip = loopPushClip;
                pushableAudioSource.loop = true;
                pushableAudioSource.Play();
            }
        }

        protected void CheckGrounded()
        {
            m_Grounded = false;

            int count = m_Rigidbody2D.GetContacts(s_ContactPointBuffer);
            for(int i = 0; i < count; ++i)
            {
                if(s_ContactPointBuffer[i].normal.y > 0.9f)
                {
                    m_Grounded = true;

                    Pushable pushable;
                
                    if(s_PushableCache.TryGetValue (s_ContactPointBuffer[i].collider, out pushable))
                    {
                        //if it is grounded on another pushable, we ensure that it is drawn after the one under, so it appear on top.
                        m_SpriteRenderer.sortingOrder = pushable.m_SpriteRenderer.sortingOrder + 1;
                    }
                }
            }
        }
    }
}