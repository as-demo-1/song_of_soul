/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.Events
{
    using Opsive.UltimateInventorySystem.Demo.CharacterControl;
    using UnityEngine;

    /// <summary>
    /// Respawn the enemies when something enters the trigger.
    /// </summary>
    public class EnemyRespawnerTrigger : MonoBehaviour
    {
        protected EnemyCharacter[] m_Enemies;

        /// <summary>
        /// Get all the enemies in the scene.
        /// </summary>
        private void Awake()
        {
            m_Enemies = FindObjectsOfType<EnemyCharacter>();
        }

        /// <summary>
        /// Respawn all the enemies when something enters the trigger.
        /// </summary>
        /// <param name="other">The other collider.</param>
        private void OnTriggerEnter(Collider other)
        {
            for (int i = 0; i < m_Enemies.Length; i++) { m_Enemies[i].Respawn(); }
        }
    }
}