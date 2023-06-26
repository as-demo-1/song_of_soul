/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.Damageable
{
    using Opsive.UltimateInventorySystem.Demo.Events;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// Damageable component, used to take damage, heal and die.
    /// </summary>
    public class Damageable : MonoBehaviour, IDamageable
    {
        [Tooltip("The starting HP amount.")]
        [SerializeField] protected int m_StartHp;
        [Tooltip("The time in the which the damageable is invincible after getting hit.")]
        [SerializeField] protected float m_InvincibilityTime;

        protected double m_LastHitTime;
        protected int m_MaxHp;
        protected int m_CurrentHp;
        protected bool m_Invincible;

        public virtual int MaxHp => m_MaxHp;
        public virtual int CurrentHp => m_CurrentHp;
        public virtual bool IsInvincible => m_Invincible;
        /// <summary>
        /// Initialize.
        /// </summary>
        private void Start()
        {
            m_MaxHp = m_StartHp;
            m_CurrentHp = m_StartHp;
            EventHandler.ExecuteEvent(this, DemoEventNames.c_Damageable_OnHpChange_Damageable, this);
        }

        /// <summary>
        /// Set if the damageable can or cannot be damaged.
        /// </summary>
        /// <param name="invincible">Is the damageable invincible.</param>
        public virtual void SetInvincible(bool invincible)
        {
            m_Invincible = invincible;
        }

        /// <summary>
        /// Check if the character is invincible.
        /// </summary>
        protected virtual void Update()
        {
            if (!m_Invincible) { return; }
            var invincible = m_LastHitTime + m_InvincibilityTime > Time.time;
            if (invincible != m_Invincible) {
                SetInvincible(invincible);
            }

        }

        /// <summary>
        /// Take Damage.
        /// </summary>
        /// <param name="amount">The amount.</param>
        public virtual void TakeDamage(int amount)
        {
            if (IsInvincible) { return; }

            if (m_InvincibilityTime > 0) {
                SetInvincible(true);
            }
            m_LastHitTime = Time.time;

            if (amount < 0) { amount = 0; }

            m_CurrentHp = Mathf.Clamp(CurrentHp - amount, 0, MaxHp);

            EventHandler.ExecuteEvent(this, DemoEventNames.c_Damageable_OnTakeDamage_Damageable_Int, this, amount);
            EventHandler.ExecuteEvent(this, DemoEventNames.c_Damageable_OnHpChange_Damageable, this);

            if (m_CurrentHp == 0) {
                Die();
            }
        }

        /// <summary>
        /// Heal amount.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="notify">Notify the listeners about the heal?</param>
        public virtual void Heal(int amount, bool notify = true)
        {
            if (amount < 0) { amount = 0; }

            m_CurrentHp = Mathf.Clamp(CurrentHp + amount, 0, MaxHp);

            if (notify) {
                EventHandler.ExecuteEvent(this, DemoEventNames.c_Damageable_OnHeal_Damageable_Int, this, amount);
            }

            EventHandler.ExecuteEvent(this, DemoEventNames.c_Damageable_OnHpChange_Damageable, this);
        }

        /// <summary>
        /// Kill this damageable.
        /// </summary>
        public virtual void Die()
        {
            gameObject.SetActive(false);

            EventHandler.ExecuteEvent(this, DemoEventNames.c_Damageable_OnDie_Damageable, this);
        }
    }
}