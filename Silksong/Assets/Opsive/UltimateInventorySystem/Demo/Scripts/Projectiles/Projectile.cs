/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.Projectiles
{
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.Demo.Damageable;
    using Opsive.UltimateInventorySystem.Utility;
    using UnityEngine;

    /// <summary>
    /// Projectile component.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Projectile : MonoBehaviour, IDamager
    {
        [Tooltip("The projectiles speed in unit per second.")]
        [SerializeField] protected float m_Speed = 1;
        [Tooltip("The damage dealt by the projectile.")]
        [SerializeField] protected int m_Damage = 10;
        [Tooltip("The life time of the projectile before it is returned to the pool.")]
        [SerializeField] protected int m_LifeTime = 5;

        protected float m_StartTime;

        /// <summary>
        /// Validate the projectile by setting the collider to trigger.
        /// </summary>
        private void OnValidate()
        {
            if (OnValidateUtility.IsPrefab(this)) { return; }
            var collider = GetComponent<Collider>();
            if (collider.isTrigger == false) { collider.isTrigger = true; }
        }

        /// <summary>
        /// Set the start time.
        /// </summary>
        private void OnEnable()
        {
            m_StartTime = Time.time;
        }

        /// <summary>
        /// Move the transform.
        /// </summary>
        void Update()
        {
            transform.position += m_Speed * Time.deltaTime * transform.forward;

            if (m_StartTime + m_LifeTime < Time.time) {
                DestroyProjectile();
            }
        }

        /// <summary>
        /// Trigger on enter.
        /// </summary>
        /// <param name="other">The other collider.</param>
        private void OnTriggerEnter(Collider other)
        {
            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null) {
                Damage(damageable);
            }

            DestroyProjectile();
        }

        /// <summary>
        /// Destroy the projectile.
        /// </summary>
        public void DestroyProjectile()
        {
            ObjectPool.Destroy(gameObject);
        }

        /// <summary>
        /// Deal damage to damageable.
        /// </summary>
        /// <param name="damageable">The damageable.</param>
        public void Damage(IDamageable damageable)
        {
            damageable.TakeDamage(m_Damage);
        }
    }
}
