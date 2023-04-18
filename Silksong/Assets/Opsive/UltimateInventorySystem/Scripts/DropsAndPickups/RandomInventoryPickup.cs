/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.DropsAndPickups
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.Interactions;
    using UnityEngine;

    /// <summary>
    /// Random Inventory pickup. Use the Inventory as a probability table.
    /// </summary>
    [RequireComponent(typeof(Inventory))]
    [RequireComponent(typeof(Interactable))]
    public class RandomInventoryPickup : InventoryPickup
    {
        [Tooltip("The minimum amount of item that can be picked up.")]
        [SerializeField] protected int m_MinAmount = 1;
        [Tooltip("The maximum amount of item that can be picked up.")]
        [SerializeField] protected int m_MaxAmount = 2;

        protected ItemAmountProbabilityTable m_ItemAmountProbabilityTable;

        /// <summary>
        /// Initialize the probability table.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            m_ItemAmountProbabilityTable = new ItemAmountProbabilityTable((m_Inventory.MainItemCollection.GetAllItemStacks(), 0));
        }

        /// <summary>
        /// Add a random set of item amounts to the item Collection.
        /// </summary>
        /// <param name="itemCollection">The item collection.</param>
        protected override void AddPickupToCollection(ItemCollection itemCollection)
        {

            if (m_ItemAmountProbabilityTable.Count == 0) {
                NotifyPickupFailed();
                return;
            }

            var randomItemAmounts = m_ItemAmountProbabilityTable.GetRandomItemAmounts(m_MinAmount, m_MaxAmount);
            
            var atLeastOneCanBeAdded = false;
            for (int i = 0; i < randomItemAmounts.Count; i++) {
                var itemAmount = randomItemAmounts[i];
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
            itemCollection.AddItems((randomItemAmounts, 0));
            NotifyPickupSuccess();
            Shared.Events.EventHandler.ExecuteEvent(m_Inventory.gameObject, "OnItemPickupStopPickup");

        }
    }
}