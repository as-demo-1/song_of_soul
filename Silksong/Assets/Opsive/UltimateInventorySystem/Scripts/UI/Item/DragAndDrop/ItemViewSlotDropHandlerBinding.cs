/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.DragAndDrop
{
    using Opsive.Shared.Events;
    using Opsive.UltimateInventorySystem.Core;
    using UnityEngine;
    using UnityEngine.EventSystems;

    /// <summary>
    /// Item View Slot Drop Handler Binding allows you to use a drop handler for a single Item View Slot without a Item View Slot Container.
    /// </summary>
    public class ItemViewSlotDropHandlerBinding : MonoBehaviour
    {
        [Tooltip("Enable this gameobject only while a drag is taking place.")]
        [SerializeField] protected bool m_OnlyEnableWhileDrag = true;
        [Tooltip("The item view slot to bind with the drop handler.")]
        [SerializeField] protected ItemViewSlot m_ItemViewSlot;
        [Tooltip("The item view drop handler to bind to the item view slot.")]
        [SerializeField] protected ItemViewDropHandler m_DropHandler;

        protected ItemViewSlotPointerEventData m_ItemViewSlotPointerEventData;

        /// <summary>
        /// Listen to the events.
        /// </summary>
        private void Awake()
        {
            if (m_ItemViewSlot == null) {
                m_ItemViewSlot = GetComponent<ItemViewSlot>();
            }

            if (m_DropHandler == null) {
                m_DropHandler = GetComponent<ItemViewDropHandler>();
            }

            if (m_ItemViewSlot == null || m_DropHandler == null) {
                Debug.LogWarning("The item View Slot or the Item View Drop Handler is missing", this);
            }

            m_ItemViewSlotPointerEventData = new ItemViewSlotPointerEventData();
            m_ItemViewSlotPointerEventData.SetValues(m_ItemViewSlot);

            m_ItemViewSlot.OnDropE += HandleItemViewSlotDrop;
            m_ItemViewSlot.OnSelectE += ItemViewSlotSelected;
            m_ItemViewSlot.OnDeselectE += ItemViewSlotDeselected;

            m_DropHandler.Initialize();
            var dropHandlerSlotCursorManager = m_DropHandler.SlotCursorManager;
            EventHandler.RegisterEvent(dropHandlerSlotCursorManager.gameObject,
                EventNames.c_ItemViewSlotCursorManagerGameobject_StartMove, HandleOnMoveStart);
            EventHandler.RegisterEvent(dropHandlerSlotCursorManager.gameObject,
                EventNames.c_ItemViewSlotCursorManagerGameobject_EndMove, HandleOnMoveEnd);

            if (m_OnlyEnableWhileDrag) {
                gameObject.SetActive(false);
            }

        }

        private void HandleOnMoveStart()
        {
            if (m_OnlyEnableWhileDrag == false) { return; }
            gameObject.SetActive(true);
        }

        private void HandleOnMoveEnd()
        {
            if (m_OnlyEnableWhileDrag == false) { return; }
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Handler a drop event.
        /// </summary>
        /// <param name="pointerEventData">The pointer event data.</param>
        protected virtual void HandleItemViewSlotDrop(PointerEventData pointerEventData)
        {
            m_ItemViewSlotPointerEventData.PointerEventData = pointerEventData;
            m_DropHandler.HandleItemViewSlotDrop(m_ItemViewSlotPointerEventData);

            m_ItemViewSlot.ItemView.Clear();
        }

        /// <summary>
        /// Handle the Item View Slot being selected.
        /// </summary>
        protected virtual void ItemViewSlotSelected()
        {
            m_DropHandler.ItemViewSlotSelected(m_ItemViewSlotPointerEventData);
        }

        /// <summary>
        /// Handle the Item View Slot being Deselected.
        /// </summary>
        protected virtual void ItemViewSlotDeselected()
        {
            m_DropHandler.ItemViewSlotDeselected(m_ItemViewSlotPointerEventData);
        }
    }
}