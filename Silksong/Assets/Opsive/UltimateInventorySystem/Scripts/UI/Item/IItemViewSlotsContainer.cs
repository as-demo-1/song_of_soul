/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item
{
    using Opsive.UltimateInventorySystem.UI.Item.ItemViewModules;
    using UnityEngine.EventSystems;

    public delegate void ItemViewSlotEventHandler(ItemViewSlotEventData slotEventData);

    /// <summary>
    /// Item View Slot Event Data for submit, select, and other event types without mouse/pointer.
    /// </summary>
    public class ItemViewSlotEventData
    {
        protected int m_PointerID;
        protected ItemViewSlotsContainerBase m_ItemViewSlotsContainer;
        protected ItemViewSlot m_ItemViewSlot;
        protected ItemView m_ItemView;
        protected int m_Index;

        public virtual int PointerID => m_PointerID;
        public ItemViewSlotsContainerBase ItemViewSlotsContainer => m_ItemViewSlotsContainer;
        public ItemViewSlot ItemViewSlot => m_ItemViewSlot;//{ get; set; }
        public ItemView ItemView => m_ItemView;
        public int Index => m_Index;

        /// <summary>
        /// Reset the values to default.
        /// </summary>
        public virtual void Reset()
        {
            m_PointerID = int.MinValue; // -1,-2, -3 are used for mouse clicks, positive numbers are used for touch
            m_ItemViewSlotsContainer = null;
            m_ItemView = null;
            m_ItemViewSlot = null;
            m_Index = -1;
        }

        /// <summary>
        /// Set the event data values.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="index"></param>
        public void SetValues(ItemViewSlotsContainerBase container, int index)
        {
            Reset();
            m_PointerID = -4; // -1,-2, -3 are used for mouse clicks, positive numbers are used for touch
            m_ItemViewSlotsContainer = container;
            m_Index = index;
            m_ItemViewSlot = ItemViewSlotsContainer.GetItemViewSlot(index);
            m_ItemView = m_ItemViewSlot.ItemView;
        }

        /// <summary>
        /// Set the event data values.
        /// </summary>
        /// <param name="itemViewSlot"></param>
        public void SetValues(ItemViewSlot itemViewSlot)
        {
            Reset();
            m_PointerID = -4; // -1,-2, -3 are used for mouse clicks, positive numbers are used for touch
            m_ItemViewSlotsContainer = null;
            m_Index = 0;
            m_ItemViewSlot = itemViewSlot;
            m_ItemView = m_ItemViewSlot.ItemView;
        }
    }

    public delegate void ItemViewSlotPointerEventHandler(ItemViewSlotPointerEventData eventData);

    /// <summary>
    /// Item View Slot Event Data for click, select, drag and other types of events.
    /// </summary>
    public class ItemViewSlotPointerEventData : ItemViewSlotEventData
    {
        public PointerEventData PointerEventData { get; set; }
        public override int PointerID => PointerEventData?.pointerId ?? m_PointerID;

        /// <summary>
        /// Reset the event data.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            PointerEventData = null;
        }
    }
}