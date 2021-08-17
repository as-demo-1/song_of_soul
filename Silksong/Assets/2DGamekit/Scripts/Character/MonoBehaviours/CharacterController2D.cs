using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class CharacterController2D : MonoBehaviour
    {
        [Tooltip("The Layers which represent gameobjects that the Character Controller can be grounded on.")]
        public LayerMask groundedLayerMask;
        [Tooltip("The distance down to check for ground.")]
        public float groundedRaycastDistance = 0.1f;

        Rigidbody2D m_Rigidbody2D;
        CapsuleCollider2D m_Capsule;
        Vector2 m_PreviousPosition;
        Vector2 m_CurrentPosition;
        Vector2 m_NextMovement;
        ContactFilter2D m_ContactFilter;
        RaycastHit2D[] m_HitBuffer = new RaycastHit2D[5];
        RaycastHit2D[] m_FoundHits = new RaycastHit2D[3];
        Collider2D[] m_GroundColliders = new Collider2D[3];
        Vector2[] m_RaycastPositions = new Vector2[3];

        public bool IsGrounded { get; protected set; }
        public bool IsCeilinged { get; protected set; }
        public Vector2 Velocity { get; protected set; }
        public Rigidbody2D Rigidbody2D { get { return m_Rigidbody2D; } }
        public Collider2D[] GroundColliders { get { return m_GroundColliders; } }
        public ContactFilter2D ContactFilter { get { return m_ContactFilter; } }


        void Awake()
        {
            m_Rigidbody2D = GetComponent<Rigidbody2D>();
            m_Capsule = GetComponent<CapsuleCollider2D>();

            m_CurrentPosition = m_Rigidbody2D.position;
            m_PreviousPosition = m_Rigidbody2D.position;

            m_ContactFilter.layerMask = groundedLayerMask;
            m_ContactFilter.useLayerMask = true;
            m_ContactFilter.useTriggers = false;

            Physics2D.queriesStartInColliders = false;
        }

        void FixedUpdate()
        {
            m_PreviousPosition = m_Rigidbody2D.position;
            m_CurrentPosition = m_PreviousPosition + m_NextMovement;
            Velocity = (m_CurrentPosition - m_PreviousPosition) / Time.deltaTime;

            m_Rigidbody2D.MovePosition(m_CurrentPosition);
            m_NextMovement = Vector2.zero;

            CheckCapsuleEndCollisions();
            CheckCapsuleEndCollisions(false);
        }

        /// <summary>
        /// This moves a rigidbody and so should only be called from FixedUpdate or other Physics messages.
        /// </summary>
        /// <param name="movement">The amount moved in global coordinates relative to the rigidbody2D's position.</param>
        public void Move(Vector2 movement)
        {
            m_NextMovement += movement;
        }

        /// <summary>
        /// This moves the character without any implied velocity.
        /// </summary>
        /// <param name="position">The new position of the character in global space.</param>
        public void Teleport(Vector2 position)
        {
            Vector2 delta = position - m_CurrentPosition;
            m_PreviousPosition += delta;
            m_CurrentPosition = position;
            m_Rigidbody2D.MovePosition(position);
        }

        /// <summary>
        /// This updates the state of IsGrounded.  It is called automatically in FixedUpdate but can be called more frequently if higher accurracy is required.
        /// </summary>
        public void CheckCapsuleEndCollisions(bool bottom = true)
        {
            Vector2 raycastDirection;
            Vector2 raycastStart;
            float raycastDistance;

            if (m_Capsule == null)
            {
                raycastStart = m_Rigidbody2D.position + Vector2.up;
                raycastDistance = 1f + groundedRaycastDistance;

                if (bottom)
                {
                    raycastDirection = Vector2.down;

                    m_RaycastPositions[0] = raycastStart + Vector2.left * 0.4f;
                    m_RaycastPositions[1] = raycastStart;
                    m_RaycastPositions[2] = raycastStart + Vector2.right * 0.4f;
                }
                else
                {
                    raycastDirection = Vector2.up;

                    m_RaycastPositions[0] = raycastStart + Vector2.left * 0.4f;
                    m_RaycastPositions[1] = raycastStart;
                    m_RaycastPositions[2] = raycastStart + Vector2.right * 0.4f;
                }
            }
            else
            {
                raycastStart = m_Rigidbody2D.position + m_Capsule.offset;
                raycastDistance = m_Capsule.size.x * 0.5f + groundedRaycastDistance * 2f;

                if (bottom)
                {
                    raycastDirection = Vector2.down;
                    Vector2 raycastStartBottomCentre = raycastStart + Vector2.down * (m_Capsule.size.y * 0.5f - m_Capsule.size.x * 0.5f);

                    m_RaycastPositions[0] = raycastStartBottomCentre + Vector2.left * m_Capsule.size.x * 0.5f;
                    m_RaycastPositions[1] = raycastStartBottomCentre;
                    m_RaycastPositions[2] = raycastStartBottomCentre + Vector2.right * m_Capsule.size.x * 0.5f;
                }
                else
                {
                    raycastDirection = Vector2.up;
                    Vector2 raycastStartTopCentre = raycastStart + Vector2.up * (m_Capsule.size.y * 0.5f - m_Capsule.size.x * 0.5f);

                    m_RaycastPositions[0] = raycastStartTopCentre + Vector2.left * m_Capsule.size.x * 0.5f;
                    m_RaycastPositions[1] = raycastStartTopCentre;
                    m_RaycastPositions[2] = raycastStartTopCentre + Vector2.right * m_Capsule.size.x * 0.5f;
                }
            }

            for (int i = 0; i < m_RaycastPositions.Length; i++)
            {
                int count = Physics2D.Raycast(m_RaycastPositions[i], raycastDirection, m_ContactFilter, m_HitBuffer, raycastDistance);

                if (bottom)
                {
                    m_FoundHits[i] = count > 0 ? m_HitBuffer[0] : new RaycastHit2D();
                    m_GroundColliders[i] = m_FoundHits[i].collider;
                }
                else
                {
                    IsCeilinged = false;

                    for (int j = 0; j < m_HitBuffer.Length; j++)
                    {
                        if (m_HitBuffer[j].collider != null)
                        {
                            if (!PhysicsHelper.ColliderHasPlatformEffector(m_HitBuffer[j].collider))
                            {
                                IsCeilinged = true;
                            }
                        }
                    }
                }
            }

            if (bottom)
            {
                Vector2 groundNormal = Vector2.zero;
                int hitCount = 0;

                for (int i = 0; i < m_FoundHits.Length; i++)
                {
                    if (m_FoundHits[i].collider != null)
                    {
                        groundNormal += m_FoundHits[i].normal;
                        hitCount++;
                    }
                }

                if (hitCount > 0)
                {
                    groundNormal.Normalize();
                }

                Vector2 relativeVelocity = Velocity;
                for (int i = 0; i < m_GroundColliders.Length; i++)
                {
                    if (m_GroundColliders[i] == null)
                        continue;

                    MovingPlatform movingPlatform;

                    if (PhysicsHelper.TryGetMovingPlatform(m_GroundColliders[i], out movingPlatform))
                    {
                        relativeVelocity -= movingPlatform.Velocity / Time.deltaTime;
                        break;
                    }
                }

                if (Mathf.Approximately(groundNormal.x, 0f) && Mathf.Approximately(groundNormal.y, 0f))
                {
                    IsGrounded = false;
                }
                else
                {
                    IsGrounded = relativeVelocity.y <= 0f;

                    if (m_Capsule != null)
                    {
                        if (m_GroundColliders[1] != null)
                        {
                            float capsuleBottomHeight = m_Rigidbody2D.position.y + m_Capsule.offset.y - m_Capsule.size.y * 0.5f;
                            float middleHitHeight = m_FoundHits[1].point.y;
                            IsGrounded &= middleHitHeight < capsuleBottomHeight + groundedRaycastDistance;
                        }
                    }
                }
            }

            for (int i = 0; i < m_HitBuffer.Length; i++)
            {
                m_HitBuffer[i] = new RaycastHit2D();
            }
        }
    }
}