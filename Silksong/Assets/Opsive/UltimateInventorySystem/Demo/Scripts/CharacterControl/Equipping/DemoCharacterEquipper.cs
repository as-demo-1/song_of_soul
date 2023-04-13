/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.CharacterControl.Equiping
{
    using Core;
    using Opsive.UltimateInventorySystem.Equipping;
    using UnityEngine;

    /// <summary>
    /// Visual character equiper, is used to equip items to a character and trigger animations.
    /// </summary>
    public class DemoCharacterEquipper : Equipper
    {
        protected Character m_Character;
        public Character Character => m_Character;

        protected CharacterAnimator m_CharacterAnimator;

        /// <summary>
        /// Initialize the component.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            InitializeCharacterAnimator();
        }

        /// <summary>
        /// Initialize the character animator.
        /// </summary>
        protected virtual void InitializeCharacterAnimator()
        {
            if (m_Character == null) {
                m_Character = GetComponent<Character>();
            }

            if (m_CharacterAnimator != null) { return; }

            if (m_Character == null) {
                m_CharacterAnimator = new CharacterAnimator(GetComponent<Animator>());
            } else {
                m_CharacterAnimator = m_Character.CharacterAnimator;
            }
        }

        /// <summary>
        /// Equip the item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="index">The item slot index.</param>
        /// <returns>True if the item equipped.</returns>
        public override bool Equip(Item item, int index)
        {
            var result = base.Equip(item, index);
            if (m_CharacterAnimator == null) { InitializeCharacterAnimator(); }

            m_CharacterAnimator.EquipItem(item, index);

            return result;
        }

        /// <summary>
        /// Unequip the item at the slot specified.
        /// </summary>
        /// <param name="index">The item slot index.</param>
        public override void UnEquip(int index)
        {
            var itemToUnequip = GetEquippedItem(index);
            base.UnEquip(index);

            if (m_CharacterAnimator == null) { InitializeCharacterAnimator(); }
            m_CharacterAnimator.UnequipItem(itemToUnequip, index);
        }
    }
}