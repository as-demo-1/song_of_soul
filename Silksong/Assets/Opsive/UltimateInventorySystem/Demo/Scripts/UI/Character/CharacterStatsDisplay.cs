/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.UI.Menus.Main.Inventory
{
    using CharacterControl;
    using CharacterControl.Player;
    using Opsive.UltimateInventorySystem.Demo.Events;
    using Opsive.UltimateInventorySystem.UI.CompoundElements;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;
    using Text = Opsive.Shared.UI.Text;

    /// <summary>
    /// The character stats UI.
    /// </summary>
    public class CharacterStatsDisplay : MonoBehaviour
    {
        [Tooltip("The max Hp text.")]
        [SerializeField] protected Text m_MaxHpValueText;
        [Tooltip("The attack text.")]
        [SerializeField] protected Text m_AttackValueText;
        [Tooltip("The defense text.")]
        [SerializeField] protected Text m_DefenseValueText;

        protected CharacterStats m_CharacterStats;

        private void Start()
        {
            Initialize();
        }

        /// <summary>
        /// Initialize the listener.
        /// </summary>
        private void Initialize()
        {
            m_CharacterStats = FindObjectOfType<PlayerCharacter>().CharacterStats;

            EventHandler.RegisterEvent(m_CharacterStats, DemoEventNames.c_CharacterStats_OnChanged, Draw);
        }

        /// <summary>
        /// Draw the stats.
        /// </summary>
        public void Draw()
        {
            if (m_CharacterStats == null) { Initialize(); }

            m_MaxHpValueText.text = m_CharacterStats.MaxHp.ToString();
            m_AttackValueText.text = m_CharacterStats.Attack.ToString();
            m_DefenseValueText.text = m_CharacterStats.Defense.ToString();
        }
    }
}
