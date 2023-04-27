/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.CharacterControl
{
    using Damageable;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.ItemActions;
    using Opsive.UltimateInventorySystem.ItemObjectBehaviours;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// A Melee Attack action.
    /// </summary>
    public class MeleeAttack : ItemObjectBehaviour
    {
        [Tooltip("The items cooldown between each hit.")]
        [SerializeField] protected float m_CoolDown;
        [Tooltip("When the weapon should check for collision after the attack started.")]
        [SerializeField] protected float m_StartCollisionDelay;
        [Tooltip("When the weapon should stop checking for collision after start collision.")]
        [SerializeField] protected float m_StopCollisionDelay;
        [Tooltip("The Audio source to play when attacking.")]
        [SerializeField] protected AudioSource m_AudioSource;

        protected Character m_Character;
        protected bool m_ActiveAttackTrigger;

        /// <summary>
        /// Use the item.
        /// </summary>
        /// <param name="itemObject">The item object.</param>
        /// <param name="itemUser">The item user.</param>
        public override void Use(ItemObject itemObject, ItemUser itemUser)
        {
            m_Character = itemUser.GetComponent<Character>();
            if (m_Character == null) { return; }

            m_NextUseTime = Time.time + m_CoolDown;

            //Character animation
            m_Character.CharacterAnimator.Attack(itemObject.Item);
            if (m_AudioSource != null) { m_AudioSource.Play(); }

            StartCoroutine(SwingIE());
        }

        /// <summary>
        /// A swing coroutine used to activate the trigger.
        /// </summary>
        /// <returns>The IEnumartor.</returns>
        protected IEnumerator SwingIE()
        {
            yield return new WaitForSeconds(m_StartCollisionDelay);
            m_ActiveAttackTrigger = true;
            yield return new WaitForSeconds(m_StopCollisionDelay);
            m_ActiveAttackTrigger = false;
        }

        /// <summary>
        /// Check for a trigger event.
        /// </summary>
        /// <param name="other">The other collider.</param>
        private void OnTriggerStay(Collider other)
        {
            if (!m_ActiveAttackTrigger) { return; }
            if (other.gameObject == m_Character.gameObject) { return; }

            if (m_Character is EnemyCharacter) {
                if (other.GetComponent<EnemyCharacter>() != null) return;
            }

            var damageable = other.GetComponent<IDamageable>();
            if (damageable == null) { return; }
            Damage(damageable);
        }

        /// <summary>
        /// Damage a damageable.
        /// </summary>
        /// <param name="damageable">The damageable.</param>
        public void Damage(IDamageable damageable)
        {
            damageable.TakeDamage(m_Character.CharacterStats.Attack);
        }
    }
}