/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using Opsive.Shared.Audio;

namespace Opsive.UltimateInventorySystem.Demo.Throwables
{
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.Demo.Damageable;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// The bomb MonoBehaviour.
    /// </summary>
    public class Bomb : MonoBehaviour, IThrowable
    {
        [Tooltip("The damage dealt by the bomb.")]
        [SerializeField] protected int m_Damage = 30;
        [Tooltip("The bomb gameobject which is disabled when the explosion occurs.")]
        [SerializeField] protected GameObject m_BombModel;
        [Tooltip("The explosion gameobject with the trigger collider and effects.")]
        [SerializeField] protected GameObject m_Explosion;
        [Tooltip("The tick period before exploding. There are 4 ticks before the explosion.")]
        [SerializeField] protected float m_TickPeriod = 1;
        [Tooltip("The bomb throw force.")]
        [SerializeField] protected float m_ThrowForce = 500f;
        [Tooltip("The audio clip played on explosion.")]
        [SerializeField] protected AudioClip m_AudioClip;
        [Tooltip("The flash parameters.")]
        [SerializeField] protected Flash m_Flash;

        protected Rigidbody m_Rigidbody;
        protected WaitForSeconds m_WaitForTick;

        /// <summary>
        /// Reset the bomb.
        /// </summary>
        private void OnEnable()
        {
            m_BombModel.SetActive(true);
            m_Explosion.SetActive(false);
            m_WaitForTick = new WaitForSeconds(m_TickPeriod);
        }

        /// <summary>
        /// Throw the bomb.
        /// </summary>
        /// <param name="direction">The direction it should be thrown in.</param>
        public void Throw(Vector3 direction)
        {
            if (m_Rigidbody == null) { m_Rigidbody = GetComponent<Rigidbody>(); }
            m_Rigidbody.AddForce(direction * m_ThrowForce);
            StartCoroutine(BombLife());
        }

        /// <summary>
        /// The bomb life coroutine.
        /// </summary>
        /// <returns>The IEnumarator.</returns>
        public IEnumerator BombLife()
        {
            yield return m_WaitForTick;

            StartCoroutine(m_Flash.CoroutineIE(m_TickPeriod * 2));
            yield return m_WaitForTick;

            yield return m_WaitForTick;

            m_Explosion.SetActive(true);
            m_BombModel.SetActive(false);
            var audioSource = AudioManager.PlayAtPosition(m_AudioClip, transform.position).AudioSource;
            audioSource.spatialBlend = 0;
            yield return m_WaitForTick;

            ObjectPool.Destroy(gameObject);
        }

        /// <summary>
        /// Damage on trigger.
        /// </summary>
        /// <param name="other">The other colliders.</param>
        private void OnTriggerEnter(Collider other)
        {
            var damageable = other.GetComponent<IDamageable>();
            if (damageable == null) { return; }

            damageable.TakeDamage(m_Damage);
        }
    }

    /// <summary>
    /// The throwable interface used to throw objects.
    /// </summary>
    public interface IThrowable
    {
        /// <summary>
        /// Invoke the throw action.
        /// </summary>
        /// <param name="direction">The direction to throw in.</param>
        void Throw(Vector3 direction);
    }
}