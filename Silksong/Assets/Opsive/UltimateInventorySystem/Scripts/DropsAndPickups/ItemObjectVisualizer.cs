/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.DropsAndPickups
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using System;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// The Item pickup visual listener will swap out the ItemPickup mesh gameObject by one specified on the item.
    /// </summary>
    public class ItemObjectVisualizer : ItemVisualizerBase
    {
        protected ItemObject m_ItemObject;

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="force">Force the initialize.</param>
        protected override void Initialize(bool force)
        {
            if (m_Initialized && !force) { return; }

            base.Initialize(force);
            
            m_ItemObject = GetComponent<ItemObject>();
            if (m_ItemObject != null) { m_ItemObject.ValidateItem(); }

            m_Initialized = true;
        }

        /// <summary>
        /// Register to the events.
        /// </summary>
        protected override void OnEnable()
        {
            if (m_ItemObject != null) {
                EventHandler.RegisterEvent(m_ItemObject, EventNames.c_ItemObject_OnItemChanged, UpdateVisual);
            }
            
            base.OnEnable();
        }

        public override void UpdateVisual()
        {
            if (m_ItemObject == null) {
                UpdateVisual(ItemInfo.None);
                return;
            }
            UpdateVisual(m_ItemObject.ItemInfo);
        }

        /// <summary>
        /// Stop listening the events in disable.
        /// </summary>
        protected void OnDisable()
        {
            if (m_ItemObject == null) { return; }
            EventHandler.UnregisterEvent(m_ItemObject, EventNames.c_ItemObject_OnItemChanged, UpdateVisual);
        }
    }
}