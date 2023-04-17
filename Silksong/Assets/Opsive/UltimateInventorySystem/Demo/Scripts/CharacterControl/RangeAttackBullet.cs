/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.CharacterControl
{
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.Demo.Damageable;
    using Opsive.UltimateInventorySystem.Interactions;
    using UnityEngine;

    /// <summary>
    /// A component used to deal damage.
    /// </summary>
    public class RangeAttackBullet : MonoBehaviour
    {
        [Tooltip("The time between the bullet appearing and disappearing.")]
        [SerializeField] protected float m_LifeTime;
        [Tooltip("The bullets speed.")]
        [SerializeField] protected float m_Speed;
        [Tooltip("The layer mask which the bullet can hit.")]
        [SerializeField] protected LayerMask m_HitLayerMask = -1;

        protected float m_CurrentLifeTime;
        private Character m_Character;

        public Character Character {
            get => m_Character;
            set => m_Character = value;
        }

        /// <summary>
        /// Initialize from pool.
        /// </summary>
        private void OnEnable()
        {
            m_CurrentLifeTime = m_LifeTime;
        }

        /// <summary>
        /// Move forward.
        /// </summary>
        private void Update()
        {
            m_CurrentLifeTime -= Time.deltaTime;
            if (m_CurrentLifeTime <= 0) { DestroyBullet(); }

            transform.position += transform.forward * (m_Speed * Time.deltaTime);
        }

        /// <summary>
        /// Destroy.
        /// </summary>
        private void DestroyBullet()
        {
            ObjectPool.Destroy(gameObject);
        }

        /// <summary>
        /// Damage on trigger enter.
        /// </summary>
        /// <param name="other">The collider that entered.</param>
        private void OnTriggerEnter(Collider other)
        {

            if (m_HitLayerMask.Contains(other.gameObject) == false) { return; }
            if (other.gameObject == m_Character.gameObject) { return; }

            if (m_Character is EnemyCharacter) {
                if (other.GetComponent<EnemyCharacter>() != null) return;
            }

            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null) {
                Damage(damageable);
            }

            DestroyBullet();
        }

        /// <summary>
        /// Deal damage.
        /// </summary>
        /// <param name="damageable">The damageable.</param>
        public void Damage(IDamageable damageable)
        {
            damageable.TakeDamage(m_Character.CharacterStats.Attack);
        }
    }
}