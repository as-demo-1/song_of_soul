/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.DropsAndPickups
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.Interactions;
    using UnityEngine;

    /// <summary>
    /// A pickup that uses the inventory component.
    /// </summary>
    [RequireComponent(typeof(Inventory))]
    [RequireComponent(typeof(Interactable))]
    public class InventoryPickup : PickupBase
    {
        [Tooltip("The item collection where the items should be added when picking up the item.")]
        [SerializeField] protected ItemCollectionID m_AddToItemCollection = ItemCollectionPurpose.Main;

        protected Inventory m_Inventory;

        public Inventory Inventory => m_Inventory;

        /// <summary>
        /// Initialize the component.
        /// </summary>
        protected void Awake()
        {
            m_Inventory = GetComponent<Inventory>();
        }

        /// <summary>
        /// The the inventory content to the interactors inventory.
        /// </summary>
        protected override void OnInteractInternal(IInteractor interactor)
        {
            if (!(interactor is IInteractorWithInventory interactorWithInventory)) { return; }
            
            var itemCollection = interactorWithInventory.Inventory.GetItemCollection(m_AddToItemCollection);

            if (itemCollection == null) {
                itemCollection = interactorWithInventory.Inventory.MainItemCollection;
            }

            AddPickupToCollection(itemCollection);
        }

        /// <summary>
        /// Add the pickup to the collection specified.
        /// </summary>
        /// <param name="itemCollection">The item Collection.</param>
        protected virtual void AddPickupToCollection(ItemCollection itemCollection)
        {
            var pickupItems = m_Inventory.MainItemCollection.GetAllItemStacks();
            
            var atLeastOneCanBeAdded = false;
            for (int i = 0; i < pickupItems.Count; i++) {
                var itemAmount = pickupItems[i];
                var canAddResult =  itemCollection.CanAddItem((ItemInfo)itemAmount);
                if (canAddResult.HasValue && canAddResult.Value.Amount != 0) {
                    atLeastOneCanBeAdded = true;
                }
            }

            if (atLeastOneCanBeAdded == false) {
                NotifyPickupFailed();
                return;
            }
            
            Shared.Events.EventHandler.ExecuteEvent(m_Inventory.gameObject, "OnItemPickupStartPickup");
            itemCollection.AddItems((pickupItems, 0));
            NotifyPickupSuccess();
            Shared.Events.EventHandler.ExecuteEvent(m_Inventory.gameObject, "OnItemPickupStopPickup");
        }
    }
}