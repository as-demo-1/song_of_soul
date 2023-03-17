/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.DropsAndPickups
{
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using UnityEngine;

    /// <summary>
    /// The Item Object Spawner is used to spawn Item Objects.
    /// </summary>
    public class ItemObjectSpawner : MonoBehaviour
    {
        [Tooltip("The item object spawner ID used to identify in the Global Register.")]
        [SerializeField] protected uint m_ID = 1;
        [Tooltip("The pick up prefab, it must have a pick up component.")]
        [SerializeField] protected ItemObject m_ItemObjectPrefab;

        public ItemObject ItemObjectPrefab {
            get => m_ItemObjectPrefab;
            set => m_ItemObjectPrefab = value;
        }

        /// <summary>
        /// Register to the global Register.
        /// </summary>
        private void Awake()
        {
            InventorySystemManager.GlobalRegister.Set(this, m_ID);
        }

        /// <summary>
        /// Spawn the item object.
        /// </summary>
        /// <param name="itemInfo">The item info to set on the item object.</param>
        /// <param name="position">The position to spawn the item.</param>
        /// <returns>The item object instance that was spawned.</returns>
        public ItemObject Spawn(ItemInfo itemInfo, Vector3 position)
        {
            var itemObject = ObjectPool.Instantiate(m_ItemObjectPrefab,
                position, Quaternion.identity).GetComponent<ItemObject>();

            itemObject.SetItem(itemInfo);

            return itemObject;
        }

        /// <summary>
        /// Spawn an item object after a certain delay and destroy it after a certain delay.
        /// </summary>
        /// <param name="itemInfo">The item info to set on the item object.</param>
        /// <param name="position">The position to spawn the item.</param>
        /// <param name="delay">The delay before the item is spawned.</param>
        /// <param name="autoDestroyTimer">The time before the item is automatically destroyed.</param>
        public void Spawn(ItemInfo itemInfo, Vector3 position, float delay, float autoDestroyTimer)
        {
            Scheduler.Schedule(delay, () => SpawnWithAutoDestroy(itemInfo, position, autoDestroyTimer));
        }

        /// <summary>
        /// Spawn the item with a delay.
        /// </summary>
        /// <param name="itemInfo">The item info to set on the item object.</param>
        /// <param name="position">The position to spawn the item.</param>
        /// <param name="delay">The delay before the item is spawned.</param>
        public void SpawnWithDelay(ItemInfo itemInfo, Vector3 position, float delay)
        {
            Scheduler.Schedule(delay, () => Spawn(itemInfo, position));
        }

        /// <summary>
        /// Spawn the item object and automatically destroy it after a certain delay.
        /// </summary>
        /// <param name="itemInfo">The item info to set on the item object.</param>
        /// <param name="position">The position to spawn the item.</param>
        /// <param name="autoDestroyTimer">The time before the item is automatically destroyed.</param>
        /// <returns>The item object instance that was spawned.</returns>
        public ItemObject SpawnWithAutoDestroy(ItemInfo itemInfo, Vector3 position, float autoDestroyTimer)
        {
            var itemObject = Spawn(itemInfo, position);
            Scheduler.Schedule(autoDestroyTimer, () => DestroyItemObject(itemObject));
            return itemObject;
        }

        /// <summary>
        /// Destory an item Object correctly using the pool if necessary.
        /// </summary>
        /// <param name="itemObject">The item object.</param>
        public void DestroyItemObject(ItemObject itemObject)
        {
            if (ObjectPool.IsPooledObject(itemObject.gameObject)) {
                ObjectPool.Destroy(itemObject.gameObject);
            } else {
                Destroy(itemObject);
            }
        }
    }
}