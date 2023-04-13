/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.Damageable
{
    using Opsive.UltimateInventorySystem.Demo.CharacterControl;
    using Opsive.UltimateInventorySystem.Demo.Events;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;
    using Random = UnityEngine.Random;

    /// <summary>
    /// Character damageable class used to receive damage.
    /// </summary>
    public class DemoCharacterDamageable : Damageable, IDamageable
    {
        [Tooltip("The character for this damageable component.")]
        [SerializeField] protected Character m_Character;
        [Tooltip("Make the character flash a color when damaged.")]
        [SerializeField] protected Flash m_Flash;

        public override int MaxHp => m_Character.CharacterStats.MaxHp;

        /// <summary>
        /// Reset the flash color.
        /// </summary>
        private void OnEnable()
        {
            m_Flash.Reset();
        }

        /// <summary>
        /// Make the character take damage.
        /// </summary>
        /// <param name="amount">The amount of damage.</param>
        public override void TakeDamage(int amount)
        {
            amount -= (int)(m_Character.CharacterStats.Defense * Random.Range(0.9f, 1.1f));

            base.TakeDamage(amount);
            m_Character.CharacterAnimator.Damaged();
            if (gameObject.activeInHierarchy == false) { return; }
            StartCoroutine(m_Flash.CoroutineIE(Mathf.Clamp(m_InvincibilityTime, 0.4f, 1f)));
        }

        /// <summary>
        /// Kill the character.
        /// </summary>
        public override void Die()
        {
            m_Character.Die();
            m_Character.CharacterAnimator.Die();
            EventHandler.ExecuteEvent(this, DemoEventNames.c_Damageable_OnDie_Damageable, this);
        }

        /// <summary>
        /// Reset the flash color.
        /// </summary>
        private void OnDisable()
        {
            m_Flash.Reset();
        }
    }
}