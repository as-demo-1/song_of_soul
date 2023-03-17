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
    using System;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// The item pickup component.
    /// </summary>
    [RequireComponent(typeof(ItemObject))]
    public class ItemPickup : PickupBase
    {
        public event Action OnStateChange;
        [Tooltip("The item collection where the items should be added when picking up the item.")]
        [SerializeField] protected ItemCollectionID m_AddToItemCollection = ItemCollectionPurpose.Main;
        [Tooltip("Fail to pickup the item if the amount added isn't the full amount inside the pickup.")]
        [SerializeField] protected bool m_FailIfFullAmountDoesNotFit;


        protected ItemObject m_ItemObject;

        public ItemObject ItemObject => m_ItemObject;
        public ItemInfo ItemInfo => m_ItemObject.ItemInfo;

        /// <summary>
        /// Initialize the component.
        /// </summary>
        protected void Awake()
        {
            m_ItemObject = GetComponent<ItemObject>();
        }

        /// <summary>
        /// Register the events.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            if (m_ItemObject == null) { return; }

            Shared.Events.EventHandler.RegisterEvent(m_ItemObject, EventNames.c_ItemObject_OnItemChanged, () => UpdateState(m_ItemObject));
            UpdateState(m_ItemObject);
        }

        /// <summary>
        /// Update the item when reactivating.
        /// </summary>
        public override void Reactivate()
        {
            base.Reactivate();
            UpdateState(m_ItemObject);
        }

        /// <summary>
        /// Update the item state.
        /// </summary>
        /// <param name="itemObject">The item object.</param>
        protected virtual void UpdateState(ItemObject itemObject)
        {
            if (m_Interactable == null || itemObject == null ||
                (itemObject.Item != null && itemObject.Item.ItemDefinition != null)) {
                OnStateChange?.Invoke();
                return;
            }

            m_Interactable.SetIsInteractable(false);
            OnStateChange?.Invoke();
        }

        /// <summary>
        /// Pickup the item on interact.
        /// </summary>
        /// <param name="interactor">The interactor that picks up the item.</param>
        protected override void OnInteractInternal(IInteractor interactor)
        {
            if (!(interactor is IInteractorWithInventory interactorWithInventory)) { return; }

            var itemCollection = interactorWithInventory.Inventory.GetItemCollection(m_AddToItemCollection);

            if (itemCollection == null) {
                itemCollection = interactorWithInventory.Inventory.MainItemCollection;
            }

            TryAddItemToCollection(itemCollection);
        }

        /// <summary>
        /// Put the picked up item in a collection.
        /// </summary>
        /// <param name="itemCollection">The item collection.</param>
        protected virtual void TryAddItemToCollection(ItemCollection itemCollection)
        {
            var itemInfo = m_ItemObject.ItemInfo;
            var canAddResult = itemCollection.CanAddItem(itemInfo);
            if (canAddResult.HasValue == false 
                || canAddResult.Value.Amount == 0 
                || (m_FailIfFullAmountDoesNotFit && canAddResult.Value.Amount != itemInfo.Amount) ){
                NotifyPickupFailed();
                return;
            }
            
            itemCollection.AddItem(itemInfo);
            NotifyPickupSuccess();
        }

        /// <summary>
        /// Deactivate the pickup.
        /// </summary>
        public override void Deactivate()
        {
            UpdateState(m_ItemObject);
            base.Deactivate();
        }

        /// <summary>
        /// Send the item pickup back to the pool.
        /// </summary>
        protected override void DestroyPickup()
        {
            m_ItemObject.SetItem(null);
            base.DestroyPickup();
        }

        /// <summary>
        /// Unregister events.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventHandler.UnregisterEvent(m_ItemObject, EventNames.c_ItemObject_OnItemChanged, () => UpdateState(m_ItemObject));
        }
    }
}
