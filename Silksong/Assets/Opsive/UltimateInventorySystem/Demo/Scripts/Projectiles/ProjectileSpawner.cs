/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.Projectiles
{
    using Opsive.Shared.Game;
    using UnityEngine;

    /// <summary>
    /// Component that spawns game objects.
    /// </summary>
    public class ProjectileSpawner : MonoBehaviour
    {
        [Tooltip("The projectile prefab.")]
        [SerializeField] protected GameObject m_ProjectilePrefab;
        [Tooltip("The projectile spawn point.")]
        [SerializeField] protected Transform m_ProjectileSpawnPoint;

        /// <summary>
        /// Shoot the game object.
        /// </summary>
        public void Shoot()
        {
            var projectGameObject = ObjectPool.Instantiate(m_ProjectilePrefab, m_ProjectileSpawnPoint);
            projectGameObject.transform.localPosition = Vector3.zero;
            projectGameObject.transform.localRotation = Quaternion.identity;
        }
    }
}
