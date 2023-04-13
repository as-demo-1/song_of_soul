/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.DropsAndPickups
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using UnityEngine;

    /// <summary>
    /// An item dropper component.
    /// </summary>
    public class ItemDropper : Dropper
    {
        [Tooltip("The inventory with the items to drop.")]
        [SerializeField] protected Inventory m_Inventory;
        [Tooltip("The itemCollection within the inventory with the items to drop.")]
        [SerializeField]
        protected ItemCollectionID m_ItemCollectionID
            = new ItemCollectionID("ItemDrops", ItemCollectionPurpose.Drop);

        // Item pickup or Inventory pickup
        protected bool m_IsItemPickup;

        public Inventory Inventory
        {
            get => m_Inventory;
            set => m_Inventory = value;
        }

        public ItemCollectionID ItemCollectionID
        {
            get => m_ItemCollectionID;
            set => m_ItemCollectionID = value;
        }

        /// <summary>
        /// Initialize the component.
        /// </summary>
        protected virtual void Awake()
        {
            if (m_PickUpPrefab == null) {
                Debug.LogError("The PickUp Prefab is null, a Prefab with the scripts Item Pickup or Inventory Pickup must be specified.",gameObject);
                return;
            }

            if (m_PickUpPrefab.GetComponent<ItemPickup>() != null) {
                m_IsItemPickup = true;
                return;
            }

            if (m_PickUpPrefab.GetComponent<InventoryPickup>() != null) {
                m_IsItemPickup = false;
                return;
            }

            Debug.LogError("The PickUp Prefab is missing an Item Pickup or Inventory Pickup component.");
        }

        /// <summary>
        /// Drop the item.
        /// </summary>
        public override void Drop()
        {
            var collection = m_Inventory.GetItemCollection(m_ItemCollectionID);

            var pooledItemInfos = GenericObjectPool.Get<ItemInfo[]>();
            DropItemsInternal(collection.GetAllItemInfos(ref pooledItemInfos));
            GenericObjectPool.Return(pooledItemInfos);
        }

        /// <summary>
        /// Drop an inventory pickup.
        /// </summary>
        /// <param name="itemList">The item collection to drop.</param>
        protected virtual void DropItemsInternal(ListSlice<ItemInfo> itemList)
        {
            if (m_IsItemPickup) {
                for (int i = 0; i < itemList.Count; i++) {
                    DropItemPickup(itemList[i]);
                }
            } else {
                DropInventoryPickup(itemList);
            }
        }

        /// <summary>
        /// Drop an inventory pickup.
        /// </summary>
        /// <param name="itemList">The item collection to drop.</param>
        public virtual void DropInventoryPickup(ListSlice<ItemInfo> itemList)
        {
            var inventoryPickup = ObjectPool.Instantiate(m_PickUpPrefab,
                m_DropTransform.position + DropOffset(),
                Quaternion.identity).GetComponent<InventoryPickup>();

            if (inventoryPickup == null) {
                Debug.LogWarning("The pickup prefab specified in the item dropper does not have an itemPickup component or a inventoryItemPickup component");
                return;
            }

            inventoryPickup.Inventory.MainItemCollection.RemoveAll();
            inventoryPickup.Inventory.MainItemCollection.AddItems(itemList);
        }

        /// <summary>
        /// Drop an item pickup.
        /// </summary>
        /// <param name="itemInfo">The item amount to drop.</param>
        public virtual void DropItemPickup(ItemInfo itemInfo)
        {
            var itemPickup = ObjectPool.Instantiate(m_PickUpPrefab,
                m_DropTransform.position + DropOffset(), Quaternion.identity).GetComponent<ItemPickup>();

            if (itemPickup == null) {
                Debug.LogWarning(
                    "The pickup prefab specified in the item dropper does not have an itemPickup component or a inventoryItemPickup component");
                return;
            }

            var itemObject = itemPickup.ItemObject;
            if (m_DropCopies) {
                var itemCopy = InventorySystemManager.Factory.CreateItem(itemInfo.Item);
                itemObject.SetItem((ItemInfo)(itemInfo.Amount, itemCopy));
            } else {
                itemObject.SetItem(itemInfo);
            }

        }
    }
}
