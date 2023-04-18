/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.CharacterControl
{
    using Opsive.UltimateInventorySystem.Core;
    using UnityEngine;

    /// <summary>
    /// This script is used to animate a character.
    /// </summary>
    public class CharacterAnimator
    {
        protected Animator m_Anim;

        private static readonly int m_SpeedAnimHash = Animator.StringToHash("Speed");
        private static readonly int m_DamagedAnimHash = Animator.StringToHash("Damaged");
        private static readonly int m_AttackAnimHash = Animator.StringToHash("Attack");
        private static readonly int m_ItemAnimHash = Animator.StringToHash("Item");
        private static readonly int m_DieAnimHash = Animator.StringToHash("Die");
        private static readonly int m_InteractAnimHash = Animator.StringToHash("Interact");
        private static readonly int m_EquippedItemAnimHash = Animator.StringToHash("EquippedItem");

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="anim">The animator.</param>
        public CharacterAnimator(Animator anim)
        {
            m_Anim = anim;
            m_Anim.keepAnimatorControllerStateOnDisable = true;
        }

        /// <summary>
        /// Move animation.
        /// </summary>
        /// <param name="speed">The move speed.</param>
        public void Move(float speed)
        {
            m_Anim.SetFloat(m_SpeedAnimHash, speed, 0f, Time.deltaTime);
        }

        /// <summary>
        /// Attack animation.
        /// </summary>
        public void Attack(Item item)
        {
            if (item.TryGetAttributeValue("AnimationID", out int animationID) == false) { return; }
            m_Anim.SetInteger(m_ItemAnimHash, animationID);
            m_Anim.SetTrigger(m_AttackAnimHash);
        }

        /// <summary>
        /// Equip animation.
        /// </summary>
        /// <param name="item">The item being equipped.</param>
        /// <param name="index">The slot index it is equipped to.</param>
        public void EquipItem(Item item, int index)
        {
            if (item.TryGetAttributeValue("AnimationID", out int animationID) == false) { return; }
            if (animationID <= 0) { return; }

            m_Anim.SetInteger(m_EquippedItemAnimHash, animationID);
        }

        /// <summary>
        /// Unequip animation.
        /// </summary>
        /// <param name="item">The item being unequipped.</param>
        /// <param name="index">The slot index it is unequipped from.</param>
        public void UnequipItem(Item item, int index)
        {
            if (item.TryGetAttributeValue("AnimationID", out int animationID) == false) { return; }
            if (animationID <= 0) { return; }

            m_Anim.SetInteger(m_EquippedItemAnimHash, -1);
        }

        /// <summary>
        /// Trigger the interact animation.
        /// </summary>
        public void Interact()
        {
            m_Anim.SetTrigger(m_InteractAnimHash);
        }

        /// <summary>
        /// Trigger the damaged animation.
        /// </summary>
        public void Damaged()
        {
            m_Anim.SetTrigger(m_DamagedAnimHash);
        }

        /// <summary>
        /// Trigger the die animation.
        /// </summary>
        public void Die()
        {
            m_Anim.SetTrigger(m_DieAnimHash);
        }
    }
}