/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.CharacterControl
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Demo.Events;
    using Opsive.UltimateInventorySystem.Equipping;
    using System;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// Stats used in the demo.
    /// </summary>
    [Serializable]
    public class Stats
    {
        [Tooltip("The max amount HP.")]
        [SerializeField] protected int m_MaxHp;
        [Tooltip("The Attack power.")]
        [SerializeField] protected int m_Attack;
        [Tooltip("The defense power.")]
        [SerializeField] protected int m_Defense;

        public int MaxHp => m_MaxHp;
        public int Attack => m_Attack;
        public int Defense => m_Defense;
    }

    /// <summary>
    /// The character stats used in the demo.
    /// </summary>
    public class CharacterStats
    {
        protected Stats m_BaseStats;
        protected IEquipper m_Equipper;

        protected int m_MaxHp;
        protected int m_Attack;
        protected int m_Defense;

        public int BaseMaxHp => m_BaseStats.MaxHp;
        public int BaseAttack => m_BaseStats.Attack;
        public int BaseDefense => m_BaseStats.Defense;

        public int MaxHp => m_MaxHp;
        public int Attack => m_Attack;
        public int Defense => m_Defense;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="baseStats">The base stats.</param>
        /// <param name="equipper">The equipper.</param>
        public CharacterStats(Stats baseStats, IEquipper equipper)
        {
            m_BaseStats = baseStats;
            m_Equipper = equipper;

            if (m_Equipper != null) {
                EventHandler.RegisterEvent(m_Equipper, EventNames.c_Equipper_OnChange, UpdateStats);
            }

            UpdateStats();
        }

        /// <summary>
        /// Update the character stats.
        /// </summary>
        public void UpdateStats()
        {
            m_MaxHp = BaseMaxHp + m_Equipper.GetEquipmentStatInt("MaxHp");
            m_Attack = BaseAttack + m_Equipper.GetEquipmentStatInt("Attack");
            m_Defense = BaseDefense + m_Equipper.GetEquipmentStatInt("Defense");

            EventHandler.ExecuteEvent(this, DemoEventNames.c_CharacterStats_OnChanged);
        }
    }
}