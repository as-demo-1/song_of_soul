/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.CharacterControl
{
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.ItemActions;
    using Opsive.UltimateInventorySystem.ItemObjectBehaviours;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// A range attack action.
    /// </summary>
    public class RangeAttack : ItemObjectBehaviour
    {
        [Tooltip("The cooldown between each attack.")]
        [SerializeField] protected float m_CoolDown;
        [Tooltip("The delay between the attack is triggered and when the bullets come out (used to sync fire with animation).")]
        [SerializeField] protected float m_StartFireDelay;
        [Tooltip("The number of bullets that should come out when firing.")]
        [SerializeField] protected int m_BulletsPerFire;
        [Tooltip("The Angle between each bullet when firing creating a cone of bullets.")]
        [SerializeField] protected float m_BulletAngle;
        [Tooltip("The bullet prefab.")]
        [SerializeField] protected GameObject m_BulletPrefab;
        [Tooltip("The Audio source to play when attacking.")]
        [SerializeField] protected AudioSource m_AudioSource;

        protected Character m_Character;
        protected Item m_Item;

        public int BulletsPerFire {
            get => m_BulletsPerFire;
            set => m_BulletsPerFire = value;
        }

        public GameObject BulletPrefab {
            get => m_BulletPrefab;
            set => m_BulletPrefab = value;
        }

        /// <summary>
        /// Use the item.
        /// </summary>
        /// <param name="itemObject">The item object.</param>
        /// <param name="itemUser">The item user.</param>
        public override void Use(ItemObject itemObject, ItemUser itemUser)
        {
            m_Character = itemUser.GetComponent<Character>();
            if (m_Character == null) { return; }

            m_Item = itemObject.Item;

            m_NextUseTime = Time.time + m_CoolDown;

            //Character animation
            m_Character.CharacterAnimator.Attack(itemObject.Item);
            if (m_AudioSource != null) { m_AudioSource.Play(); }

            StartCoroutine(FireIE());
        }

        /// <summary>
        /// Fire coroutine.
        /// </summary>
        /// <returns>The IEnumerator.</returns>
        protected IEnumerator FireIE()
        {
            yield return new WaitForSeconds(m_StartFireDelay);
            var charTransform = m_Character.transform;


            for (int i = 0; i < m_BulletsPerFire; i++) {
                var angleDiff = 0f;
                var sign = ((-2 * (i % 2)) + 1);
                if (m_BulletsPerFire % 2 == 0) {
                    angleDiff = sign * Mathf.CeilToInt((i + 1) / 2f) * m_BulletAngle;
                } else {
                    angleDiff = sign * Mathf.CeilToInt(i / 2f) * m_BulletAngle;
                }
                var rot = charTransform.rotation.eulerAngles;
                rot = new Vector3(rot.x, rot.y + angleDiff, rot.z);
                var bulletGO = ObjectPool.Instantiate(
                    m_BulletPrefab,
                    charTransform.position + charTransform.forward + Vector3.up,
                    Quaternion.Euler(rot));

                bulletGO.GetComponent<RangeAttackBullet>().Character = m_Character;
            }

        }
    }
}