/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.Projectiles
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// A component that spawns projectile repetitively. 
    /// </summary>
    public class AutomaticProjectileSpawner : ProjectileSpawner
    {
        [Tooltip("The time elapsed between each shot.")]
        [SerializeField] private float m_FirePeriod = 1;

        protected Coroutine m_FireCoroutine;

        // Start is called before the first frame update
        void Start()
        {
            StartAutoFire();
        }

        /// <summary>
        /// Start the coroutine.
        /// </summary>
        public void StartAutoFire()
        {
            if (m_FireCoroutine != null) { return; }

            m_FireCoroutine = StartCoroutine(AutomaticFire());
        }

        /// <summary>
        /// Stop the coroutine.
        /// </summary>
        public void StopAutoFire()
        {
            StopCoroutine(m_FireCoroutine);
            m_FireCoroutine = null;
        }

        /// <summary>
        /// The coroutine.
        /// </summary>
        /// <returns>The IEnumerator.</returns>
        private IEnumerator AutomaticFire()
        {
            while (true) {
                yield return new WaitForSeconds(m_FirePeriod);
                Shoot();
            }
        }
    }
}
