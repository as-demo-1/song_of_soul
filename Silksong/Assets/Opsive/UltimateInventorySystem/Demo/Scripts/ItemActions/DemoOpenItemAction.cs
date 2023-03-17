/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.ItemActions
{
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.DropsAndPickups;
    using Opsive.UltimateInventorySystem.ItemActions;
    using UnityEngine;

    /// <summary>
    /// Demo Item action used to open a item containing items. When opened the items within are dropped around the player.
    /// </summary>
    [System.Serializable]
    public class DemoOpenItemAction : ItemAction
    {
        [Tooltip("The pickup item prefab, it must have a ItemPickup component.")]
        [SerializeField] protected GameObject m_PickUpItemPrefab;

        public GameObject PickUpItemPrefab {
            get => m_PickUpItemPrefab;
            set => m_PickUpItemPrefab = value;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DemoOpenItemAction()
        {
            m_Name = "Open";
        }

        /// <summary>
        /// Can the item action be invoked.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        /// <returns>True if it can be invoked.</returns>
        protected override bool CanInvokeInternal(ItemInfo itemInfo, ItemUser itemUser)
        {
            var item = itemInfo.Item;
            var itemCollection = itemInfo.ItemCollection;
            return item.GetAttribute<Attribute<ItemAmounts>>("Slots") != null
                   && itemCollection.HasItem((1, item));
        }

        /// <summary>
        /// Consume the item.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        protected override void InvokeActionInternal(ItemInfo itemInfo, ItemUser itemUser)
        {
            if (m_PickUpItemPrefab == null) {
                Debug.LogWarning("Item Pickup Prefab is null on the Open Item Action.");
                return;
            }

            var item = itemInfo.Item;
            var itemCollection = itemInfo.ItemCollection;
            var inventory = itemInfo.Inventory;
            itemCollection.RemoveItem(item);

            var slots = item.GetAttribute<Attribute<ItemAmounts>>("Slots").GetValue();
            if (slots == null) { return; }

            for (int i = 0; i < slots.Count; i++) {

                var itemPickup = ObjectPool.Instantiate(m_PickUpItemPrefab,
                    inventory.gameObject.transform.position + new Vector3(Random.value * 2 - 1, Random.value * 2, Random.value * 2 - 1),
                    Quaternion.identity).GetComponent<ItemPickup>();

                if (itemPickup == null) {
                    Debug.LogWarning("Item Pickup is null on the Open Item Action.");
                    return;
                }

                var itemObject = itemPickup.ItemObject;
                itemObject.SetItem((ItemInfo)
                    (InventorySystemManager.CreateItem(slots[i].Item), slots[i].Amount));
            }
        }
    }
}