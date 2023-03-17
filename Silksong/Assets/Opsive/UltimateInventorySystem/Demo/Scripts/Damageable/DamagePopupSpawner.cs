/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.Damageable
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.Demo.Events;
    using UnityEngine;

    /// <summary>
    /// Damage popup spawner.
    /// </summary>
    public class DamagePopupSpawner : MonoBehaviour
    {
        [Tooltip("The pop up prefab for damage taken by the player.")]
        [SerializeField] protected TextPopup m_DamagePrefab;
        [Tooltip("The pop up prefab for healing.")]
        [SerializeField] protected TextPopup m_HealPrefab;
        [Tooltip("The pop up prefab for damage taken by the enemy.")]
        [SerializeField] protected TextPopup m_EnemyDamagePrefab;

        /// <summary>
        /// Damageable Type.
        /// </summary>
        public enum DamageableType
        {
            PLAYER,  //A player damageable.
            ENEMY    //An enemy damageable.
        }

        protected static DamagePopupSpawner Instance;

        /// <summary>
        /// make it a singleton.
        /// </summary>
        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Register a damageable.
        /// </summary>
        /// <param name="newDamageable">The damageable.</param>
        /// <param name="type">The damageable type.</param>
        public static void RegisterDamageable(Damageable newDamageable, DamageableType type)
        {
            Instance.RegisterDamageableInternal(newDamageable, type);
        }

        /// <summary>
        /// Unregister a damageable.
        /// </summary>
        /// <param name="newDamageable">The damageable.</param>
        /// <param name="type">The damageable type.</param>
        public static void UnregisterDamageable(Damageable newDamageable, DamageableType type)
        {
            Instance.UnregisterDamageableInternal(newDamageable, type);
        }

        /// <summary>
        /// Register a damageable.
        /// </summary>
        /// <param name="newDamageable">The damageable.</param>
        /// <param name="type">The damageable type.</param>
        public void RegisterDamageableInternal(Damageable newDamageable, DamageableType type)
        {
            if (type == DamageableType.PLAYER) {
                EventHandler.RegisterEvent<Damageable, int>(newDamageable, DemoEventNames.c_Damageable_OnTakeDamage_Damageable_Int, PopDamage);
                EventHandler.RegisterEvent<Damageable, int>(newDamageable, DemoEventNames.c_Damageable_OnHeal_Damageable_Int, PopHeal);
            } else if (type == DamageableType.ENEMY) {
                EventHandler.RegisterEvent<Damageable, int>(newDamageable, DemoEventNames.c_Damageable_OnTakeDamage_Damageable_Int, PopEnemyDamage);
                EventHandler.RegisterEvent<Damageable, int>(newDamageable, DemoEventNames.c_Damageable_OnHeal_Damageable_Int, PopHeal);
            }
        }

        /// <summary>
        /// Unregister a damageable.
        /// </summary>
        /// <param name="newDamageable">The damageable.</param>
        /// <param name="type">The damageable type.</param>
        public void UnregisterDamageableInternal(Damageable newDamageable, DamageableType type)
        {
            if (type == DamageableType.PLAYER) {
                EventHandler.UnregisterEvent<Damageable, int>(newDamageable, DemoEventNames.c_Damageable_OnTakeDamage_Damageable_Int, PopDamage);
                EventHandler.UnregisterEvent<Damageable, int>(newDamageable, DemoEventNames.c_Damageable_OnHeal_Damageable_Int, PopHeal);
            } else if (type == DamageableType.ENEMY) {
                EventHandler.UnregisterEvent<Damageable, int>(newDamageable, DemoEventNames.c_Damageable_OnTakeDamage_Damageable_Int, PopEnemyDamage);
                EventHandler.UnregisterEvent<Damageable, int>(newDamageable, DemoEventNames.c_Damageable_OnHeal_Damageable_Int, PopHeal);
            }
        }

        /// <summary>
        /// Pop an enemy damage
        /// </summary>
        /// <param name="damageable">The damageable.</param>
        /// <param name="amount">The amount.</param>
        public void PopEnemyDamage(Damageable damageable, int amount)
        {
            Pop(damageable.transform.position, amount, m_EnemyDamagePrefab);
        }

        /// <summary>
        /// Pop a damage.
        /// </summary>
        /// <param name="damageable">The damageable.</param>
        /// <param name="amount">The amount.</param>
        public void PopDamage(Damageable damageable, int amount)
        {
            Pop(damageable.transform.position, amount, m_DamagePrefab);
        }

        /// <summary>
        /// Pop heal.
        /// </summary>
        /// <param name="damageable">The damageable.</param>
        /// <param name="amount">The amount.</param>
        public void PopHeal(Damageable damageable, int amount)
        {
            Pop(damageable.transform.position, amount, m_HealPrefab);
        }

        /// <summary>
        /// Base function to pop a damage prefab.
        /// </summary>
        /// <param name="worldPosition">World position.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="prefab">The prefab.</param>
        public void Pop(Vector3 worldPosition, int amount, TextPopup prefab)
        {
            var damagePopupGameObject = ObjectPool.Instantiate(prefab.gameObject, transform);

            Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

            damagePopupGameObject.transform.SetParent(transform, false);
            damagePopupGameObject.transform.position = screenPosition;
            damagePopupGameObject.GetComponent<TextPopup>().Pop(amount.ToString());

        }
    }
}