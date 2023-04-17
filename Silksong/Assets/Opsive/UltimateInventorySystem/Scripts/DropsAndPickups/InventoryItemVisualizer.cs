/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.DropsAndPickups
{
    using Opsive.Shared.Events;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using UnityEngine;

    /// <summary>
    /// The Item pickup visual listener will swap out the ItemPickup mesh gameObject by one specified on the item.
    /// </summary>
    public class InventoryItemVisualizer : ItemVisualizerBase
    {
        protected Inventory m_Inventory;

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="force">Force the initialize.</param>
        protected override void Initialize(bool force)
        {
            if (m_Initialized && !force) { return; }

            base.Initialize(force);
            
            m_Inventory = GetComponent<Inventory>();
            if (m_Inventory != null) { m_Inventory.Initialize(false); }

            m_Initialized = true;
        }

        /// <summary>
        /// Register to the events.
        /// </summary>
        protected override void OnEnable()
        {
            if (m_Inventory != null) {
                EventHandler.RegisterEvent(m_Inventory, EventNames.c_Inventory_OnUpdate, UpdateVisual);
            }
            
            base.OnEnable();
        }

        /// <summary>
        /// Update the visuals of the item.
        /// </summary>
        public override void UpdateVisual()
        {
            if (m_Inventory == null) {
                UpdateVisual(ItemInfo.None);
                return;
            }
            var itemInfos = m_Inventory.AllItemInfos;

            //Find the best itemInfo to show.
            ItemInfo itemInfo = ItemInfo.None;
            for (int i = 0; i < itemInfos.Count; i++) {
                var item = itemInfos[i].Item;
                if(item == null){continue;}

                if (itemInfo.Item == null) {
                    itemInfo = itemInfos[i];
                }

                if (item.GetAttribute<Attribute<GameObject>>(m_PrefabAttributeName) != null) {
                    itemInfo = itemInfos[i];
                    break;
                }
            }
            
            UpdateVisual(itemInfo);
        }

        /// <summary>
        /// Stop listening the events in disable.
        /// </summary>
        protected void OnDisable()
        {
            if (m_Inventory == null) { return; }
            EventHandler.UnregisterEvent(m_Inventory, EventNames.c_Inventory_OnUpdate, UpdateVisual);
        }
    }
}