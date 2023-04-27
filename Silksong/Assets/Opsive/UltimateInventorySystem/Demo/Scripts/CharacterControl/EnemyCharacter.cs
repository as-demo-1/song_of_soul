/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.CharacterControl
{
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.DatabaseNames;
    using Opsive.UltimateInventorySystem.Demo.CharacterControl.Equiping;
    using Opsive.UltimateInventorySystem.Demo.CharacterControl.Player;
    using Opsive.UltimateInventorySystem.Demo.Damageable;
    using Opsive.UltimateInventorySystem.Equipping;
    using Opsive.UltimateInventorySystem.ItemObjectBehaviours;
    using UnityEngine;
    using UnityEngine.AI;
    using UnityEngine.Events;
    using EventHandler = Shared.Events.EventHandler;

    /// <summary>
    /// Enemy Character component.
    /// </summary>
    public class EnemyCharacter : Character
    {
        [Tooltip("The radius where the enemy can see the character.")]
        [SerializeField] protected float m_ViewRadius;
        [Tooltip("The enemies radius for starting range attacks.")]
        [SerializeField] protected float m_RangedAttackRadius;
        [Tooltip("The enemies radius for starting melee attacks.")]
        [SerializeField] protected float m_MeleeAttackRadius;
        [Tooltip("The enemies coolDown between attacks (Does not override the weapon coolDown).")]
        [SerializeField] protected float m_AttackCooldown;
        [Tooltip("The transforms the enemy will follow when no player is in sight.")]
        [SerializeField] protected Transform[] m_WalkCycle;
        [Tooltip("A unity event triggered when the enemy dies.")]
        [SerializeField] protected UnityEvent m_OnDie;

        protected DemoCharacterEquipper m_DemoCharacterEquipper;
        protected UsableEquippedItemsHandler m_UsableEquippedItemsHandler;
        protected CharacterNavMeshMover m_NavMeshMover;
        protected PlayerCharacter m_PlayerCharacter;

        protected int m_WalkCycleIndex;

        protected ItemCategory m_SwordCategory;
        protected float m_AttackRadius;
        protected float m_NextAttack;

        /// <summary>
        /// Initialize the enemy character.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            m_NavMeshMover = new CharacterNavMeshMover(this);
            m_CharacterMover = m_NavMeshMover;

            m_PlayerCharacter = FindObjectOfType<PlayerCharacter>();
            m_DemoCharacterEquipper = GetComponent<DemoCharacterEquipper>();
            m_UsableEquippedItemsHandler = GetComponent<UsableEquippedItemsHandler>();

            EventHandler.RegisterEvent(m_Equipper, EventNames.c_Equipper_OnChange, EquipmentChanged);
        }

        /// <summary>
        /// Initialize the enemy character.
        /// </summary>
        protected void Start()
        {
            m_SwordCategory = InventorySystemManager.GetItemCategory(DemoInventoryDatabaseNames.Sword.name);
            DamagePopupSpawner.RegisterDamageable(m_CharacterDamageable, DamagePopupSpawner.DamageableType.ENEMY);

            EquipmentChanged();
        }

        /// <summary>
        /// Unregister any events.
        /// </summary>
        protected void OnDestroy()
        {
            DamagePopupSpawner.UnregisterDamageable(m_CharacterDamageable, DamagePopupSpawner.DamageableType.ENEMY);
        }

        /// <summary>
        /// Reinitialize an enemy coming from the pool.
        /// </summary>
        private void OnEnable()
        {
            if (NavMesh.SamplePosition(gameObject.transform.position, out var closestHit, 500f, NavMesh.AllAreas)) {
                gameObject.transform.position = closestHit.position;
            } else {
                Debug.LogError("Could not find position on NavMesh.");
            }
        }

        /// <summary>
        /// Update the enemy AI when the equipment is modified.
        /// </summary>
        private void EquipmentChanged()
        {
            if (m_SwordCategory == null || m_Equipper.Slots == null) { return; }

            var item = m_Equipper.GetEquippedItem(0);
            if (item == null) { return; }

            if (m_SwordCategory.InherentlyContains(item)) {
                m_AttackRadius = m_MeleeAttackRadius;
            } else {
                m_AttackRadius = m_RangedAttackRadius;
            }
        }

        /// <summary>
        /// Follow the player character and attack him when in range.
        /// </summary>
        protected override void Update()
        {

            var playerDistance = Vector3.Distance(transform.position, m_PlayerCharacter.transform.position);

            if (playerDistance > m_ViewRadius) {
                if (m_WalkCycle.Length == 0) { m_NavMeshMover.Target = null; } else {
                    m_NavMeshMover.Target = m_WalkCycle[m_WalkCycleIndex];
                    if (Vector3.Distance(transform.position, m_NavMeshMover.Target.position) < 0.8f) {
                        m_WalkCycleIndex = m_WalkCycleIndex == m_WalkCycle.Length - 1 ? 0 : m_WalkCycleIndex + 1;
                    }
                }

            } else {
                m_NavMeshMover.Target = m_PlayerCharacter.transform;
            }

            var attack = Time.time > m_NextAttack && playerDistance < m_AttackRadius;
            if (attack) {
                if (m_DemoCharacterEquipper.Slots.Length > 0) {
                    var usableItem = m_DemoCharacterEquipper.Slots[0].ItemObject.gameObject.GetCachedComponent<IItemObjectBehaviourHandler>();
                    m_UsableEquippedItemsHandler.UseItem(usableItem, 0);
                }

                m_NextAttack = Time.time + m_AttackCooldown;
            }

            base.Update();
        }

        /// <summary>
        /// Respawn the enemy on death.
        /// </summary>
        public override void Die()
        {
            m_NavMeshMover.NavMeshAgent.enabled = false;
            gameObject.SetActive(false);
            if (m_RespawnOnDeath) {
                Scheduler.Schedule(0.5f, Respawn);
            }
            m_OnDie.Invoke();
        }

        /// <summary>
        /// Respawn on the first walk cycle transform.
        /// </summary>
        public override void Respawn()
        {
            if (m_WalkCycle.Length == 0 || gameObject.activeInHierarchy) {
                return;
            }

            transform.position = m_WalkCycle[0].position + Vector3.up;
            m_CharacterDamageable.Heal(int.MaxValue);
            gameObject.SetActive(true);
            m_NavMeshMover.NavMeshAgent.enabled = true;

        }
    }
}