/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item
{
    using System;
    using Opsive.UltimateInventorySystem.UI.Item.DragAndDrop;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers;
    using System.Collections.Generic;
    using Opsive.UltimateInventorySystem.UI.Item.ItemViewModules;
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// The Item View Slot move cursor, used to Move items without a pointer.
    /// </summary>
    public class ItemViewSlotMoveCursor : ItemViewSlotsContainerBinding
    {
        [Tooltip("The Item view Drop Handler.")]
        [SerializeField] internal ItemViewDropHandler m_DropHandler;
        [Tooltip("Can be a hidden display, it is required to allow cancelling the move action without closing the other panels.")]
        [SerializeField] internal DisplayPanel m_MoveDisplayPanel;
        [Tooltip("Unbind ItemAction while moving.")]
        [SerializeField] internal ItemViewSlotsContainerBinding[] m_UnbindWhileMoving;
        [Tooltip("Unbind ItemAction while moving.")]
        [SerializeField] internal GameObject[] m_UnbindInterfaceWhileMoving;
        [Tooltip("Event on start move.")]
        [SerializeField] protected UnityEvent m_OnMoveStart;
        [Tooltip("Event on start move.")]
        [SerializeField] protected UnityEvent m_OnMoveEnd;

        protected bool m_IsMoving;
        protected ItemViewSlotEventData m_ItemViewSlotEventData;
        protected List<IItemViewSlotContainerBinding> m_UnbindItemViewSlotContainersWhileMoving;
        protected Vector2 m_ItemShapeOffset;

        private ItemViewSlotCursorManager ItemViewSlotCursorManager =>
            m_ItemViewSlotsContainer.ItemViewSlotCursor;
        
        public UnityEvent OnMoveStart {
            get => m_OnMoveStart;
            set => m_OnMoveStart = value;
        }
        
        public UnityEvent OnMoveEnd {
            get => m_OnMoveEnd;
            set => m_OnMoveEnd = value;
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="force">Force the initialize.</param>
        public override void Initialize(bool force)
        {
            if (m_IsInitialized && !force) { return; }
            base.Initialize(force);

            m_UnbindItemViewSlotContainersWhileMoving = new List<IItemViewSlotContainerBinding>();
            m_UnbindItemViewSlotContainersWhileMoving.AddRange(m_UnbindWhileMoving);
            for (int i = 0; i < m_UnbindInterfaceWhileMoving.Length; i++) {
                m_UnbindItemViewSlotContainersWhileMoving.AddRange(m_UnbindInterfaceWhileMoving[i].GetComponents<IItemViewSlotContainerBinding>());
            }
        }

        /// <summary>
        /// A slot container was bound.
        /// </summary>
        protected override void OnBindItemViewSlotContainer()
        {
            if (m_DropHandler == null) {
                m_DropHandler = GetComponent<ItemViewDropHandler>();
                if (m_DropHandler == null) {
                    Debug.LogError("The Drop Handler is missing", gameObject);
                    return;
                }
            }

            if (m_ItemViewSlotEventData == null) {
                m_ItemViewSlotEventData = new ItemViewSlotEventData();
            }
            m_ItemViewSlotsContainer.OnItemViewSlotSelected += ItemViewSlotSelected;
            m_ItemViewSlotsContainer.OnItemViewSlotClicked += ItemViewSlotClicked;
            if (m_MoveDisplayPanel != null) {
                m_MoveDisplayPanel.OnClose += CancelMove;
            }
        }

        /// <summary>
        /// The slot container was unbound.
        /// </summary>
        protected override void OnUnbindItemViewSlotContainer()
        {
            m_ItemViewSlotsContainer.OnItemViewSlotSelected -= ItemViewSlotSelected;
            m_ItemViewSlotsContainer.OnItemViewSlotClicked -= ItemViewSlotClicked;
            if (m_MoveDisplayPanel != null) {
                m_MoveDisplayPanel.OnClose -= CancelMove;
            }
        }

        /// <summary>
        /// An Item View slot was selected.
        /// </summary>
        /// <param name="eventdata">The event data.</param>
        private void ItemViewSlotSelected(ItemViewSlotEventData eventdata)
        {
            if (m_IsMoving == false || ItemViewSlotCursorManager.IsMovingItemView == false) { return; }

            var basePosition = eventdata.ItemViewSlot.transform.position;
            var viewPosition = new Vector2(basePosition.x, basePosition.y) - m_ItemShapeOffset;
            
            ItemViewSlotCursorManager.SetPosition(viewPosition);
        }

        /// <summary>
        /// The item view slot was clicked.
        /// </summary>
        /// <param name="eventdata">The event data.</param>
        private void ItemViewSlotClicked(ItemViewSlotEventData eventdata)
        {
            if (m_IsMoving == false || ItemViewSlotCursorManager.IsMovingItemView == false) { return; }
            m_DropHandler.HandleItemViewSlotDrop(eventdata);

            if (m_MoveDisplayPanel != null) {
                m_MoveDisplayPanel.Close(true);
            }

            EndMove();
        }

        /// <summary>
        /// Cancel the move.
        /// </summary>
        private void CancelMove()
        {
            ItemViewSlotCursorManager.RemoveItemView();
            EndMove();
        }

        /// <summary>
        /// The move ended.
        /// </summary>
        protected void EndMove()
        {
            m_IsMoving = false;
            for (int i = 0; i < m_UnbindItemViewSlotContainersWhileMoving.Count; i++) {
                m_UnbindItemViewSlotContainersWhileMoving[i]?.BindItemViewSlotContainer(m_ItemViewSlotsContainer);
            }
            
            OnMoveEnd?.Invoke();
        }

        /// <summary>
        /// Start moving the item view slot.
        /// </summary>
        /// <param name="index">The index of the item view slot.</param>
        public void StartMove(int index)
        {
            if (ItemViewSlotCursorManager.CanMove() == false) { return; }

            var itemViewSlot = m_ItemViewSlotsContainer.GetItemViewSlot(index);

            if (m_MoveDisplayPanel != null) {
                m_MoveDisplayPanel.Open(m_MoveDisplayPanel.Manager.SelectedDisplayPanel,
                    null, false);
            }

            m_IsMoving = true;

            m_ItemViewSlotEventData.SetValues(m_ItemViewSlotsContainer, index);

            var basePosition = itemViewSlot.transform.position;
            var viewPosition = basePosition;
            for (int i = 0; i < itemViewSlot.ItemView.Modules.Count; i++) {
                var module = itemViewSlot.ItemView.Modules[i];
                if (module is ItemShapeItemView itemShapeItemView) {
                    viewPosition = itemShapeItemView.ForegroundItemView.View.transform.position;
                }
            }

            m_ItemShapeOffset = basePosition - viewPosition;
            
            ItemViewSlotCursorManager.StartMove(m_ItemViewSlotEventData, viewPosition);

            for (int i = 0; i < m_UnbindItemViewSlotContainersWhileMoving.Count; i++) {
                m_UnbindItemViewSlotContainersWhileMoving[i]?.UnbindItemViewSlotContainer();
            }
            
            OnMoveStart?.Invoke();
        }
    }
}