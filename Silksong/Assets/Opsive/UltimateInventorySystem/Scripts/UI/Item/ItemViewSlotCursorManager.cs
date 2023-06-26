/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.DataContainers;
    using Opsive.UltimateInventorySystem.UI.Item.ItemViewModules;
    using Opsive.UltimateInventorySystem.UI.Views;
    using UnityEngine;

    /// <summary>
    /// The item view slot cursor manager.
    /// </summary>
    public class ItemViewSlotCursorManager : MonoBehaviour, IObjectWithIDReadOnly
    {
        [Tooltip("Use an ID to differentiate the Item View Slot Cursor Managers in a multiplayer setting.")]
        [SerializeField] protected uint m_ID = 1;
        [Tooltip("The canvas where this grid exist.")]
        [SerializeField] protected Canvas m_Canvas;
        [Tooltip("The position offset always added on the position.")]
        [SerializeField] protected Vector2 m_PositionOffset;
        [Tooltip("Should the moving Item View be spawned using the container or the local Category Item View Set.")]
        [SerializeField] protected bool m_UseContainerToSpawnItemView = true;
        [Tooltip("The Category Item View Set used to spawn the moving Item View.")]
        [SerializeField] protected CategoryItemViewSet m_CategoryItemViewSet;

        protected bool m_IsInitialized = false;

        protected ItemViewSlotsContainerBase m_SourceContainer;
        protected ItemViewSlot m_SourceItemViewSlot;
        protected ItemView m_FloatingItemView;
        protected bool m_IsMoving = false;

        protected ItemViewSlotEventData m_SlotEventData;

        public uint ID => m_ID;

        public bool IsMovingItemView => m_IsMoving;
        public ItemView FloatingItemView => m_FloatingItemView;
        public ItemViewSlot SourceItemViewSlot => m_SourceItemViewSlot;

        public ItemViewSlotEventData SlotEventData => m_SlotEventData;

        public bool UseContainerToSpawnItemView {
            get => m_UseContainerToSpawnItemView;
            set => m_UseContainerToSpawnItemView = value;
        }

        public CategoryItemViewSet CategoryItemViewSet {
            get => m_CategoryItemViewSet;
            set => m_CategoryItemViewSet = value;
        }

        public Vector2 PositionOffset {
            get => m_PositionOffset;
            set => m_PositionOffset = value;
        }

        /// <summary>
        /// Awake.
        /// </summary>
        private void Awake()
        {
            Initialize();
        }

        /// <summary>
        /// Initialize the grid.
        /// </summary>
        public virtual void Initialize()
        {
            if (m_IsInitialized) { return; }

            InventorySystemManager.ItemViewSlotCursorManagerRegister.Register(this);

            if (m_Canvas == null) { m_Canvas = GetComponentInParent<Canvas>(); }

            m_IsInitialized = true;
        }

        /// <summary>
        /// Can the Manager be used to move an item view.
        /// </summary>
        /// <returns>True if it can move.</returns>
        public bool CanMove()
        {
            return m_IsMoving == false && m_FloatingItemView == null;
        }

        /// <summary>
        /// Start to move and Item.
        /// </summary>
        /// <param name="sourceSlotEventData">The slot event data of the source.</param>
        /// <param name="position">The position where the moving item view should.</param>
        public void StartMove(ItemViewSlotEventData sourceSlotEventData, Vector2 position)
        {
            if (CanMove() == false) { return; }

            StartMove(sourceSlotEventData);
            SetPosition(position);
        }

        /// <summary>
        /// Set the source Item view slot.
        /// </summary>
        /// <param name="slotEventData">The slot event data.</param>
        public void StartMove(ItemViewSlotEventData slotEventData)
        {
            if (CanMove() == false) { return; }

            m_SlotEventData = slotEventData;
            StartMove(slotEventData.ItemViewSlot, slotEventData.ItemViewSlotsContainer);
        }

        /// <summary>
        /// The source item view slot.
        /// </summary>
        /// <param name="itemViewSlot">The item view slot.</param>
        /// <param name="viewSlotsContainer">The view slot container.</param>
        protected void StartMove(ItemViewSlot itemViewSlot, ItemViewSlotsContainerBase viewSlotsContainer)
        {
            if (CanMove() == false) { return; }
            
            m_SourceItemViewSlot = itemViewSlot;
            m_SourceContainer = viewSlotsContainer;

            SetItemViewAsMovingSource(m_SourceItemViewSlot, true);

            if (m_SourceContainer != null) {
                m_FloatingItemView = SetMovingItemView(m_SourceContainer, m_SourceItemViewSlot.ItemInfo);
                // Cancel the drag if the panel is closed.
                m_SourceContainer.OnDisableEvent += HandleSourceViewSlotContainerDisabled;
            }

            EventHandler.ExecuteEvent(gameObject, EventNames.c_ItemViewSlotCursorManagerGameobject_StartMove);
        }

        /// <summary>
        /// Cancel the drag event.
        /// </summary>
        protected virtual void HandleSourceViewSlotContainerDisabled()
        {
            CancelMove();
        }

        /// <summary>
        /// Cancel the drag event.
        /// </summary>
        public virtual void CancelMove()
        {
            if (m_SourceContainer != null) {
                m_SourceContainer.CancelDragEvent();
            }
            DragEnded();
        }

        /// <summary>
        /// Set the floating item view.
        /// </summary>
        /// <param name="viewSlotsContainer">The item view slot container.</param>
        /// <param name="element">The item info.</param>
        /// <returns>The item view created for displaying th item moving.</returns>
        protected ItemView SetMovingItemView(ItemViewSlotsContainerBase viewSlotsContainer, ItemInfo element)
        {
            var itemViewPrefab = m_UseContainerToSpawnItemView ?
                viewSlotsContainer.GetViewPrefabFor(element) :
                m_CategoryItemViewSet?.FindItemViewPrefabForItem(element.Item);

            if (itemViewPrefab == null) {
                Debug.LogWarning("View Drawer View Prefab is null.");
                return null;
            }

            var itemView = ObjectPool.Instantiate(itemViewPrefab, m_Canvas.transform).GetComponent<ItemView>();

            var size = itemView.RectTransform.rect.size;

            itemView.RectTransform.anchorMin = Vector2.zero;
            itemView.RectTransform.anchorMax = Vector2.zero;

            itemView.RectTransform.sizeDelta = size;

            itemView.SetValue(element);
            SetItemViewAsMoving(itemView);

            m_FloatingItemView = itemView;
            m_IsMoving = true;

            return itemView;
        }

        /// <summary>
        /// Set the position of the moving Item View.
        /// </summary>
        /// <param name="newPosition">The new position.</param>
        public void SetPosition(Vector2 newPosition)
        {
            m_FloatingItemView.RectTransform.anchoredPosition = (newPosition + PositionOffset) / m_Canvas.scaleFactor;
        }

        /// <summary>
        /// Add a delta position to the item view.
        /// </summary>
        /// <param name="deltaPosition">The delta position.</param>
        /// <param name="autoScaleFactor">Should the offset auto scale with the canvas?</param>
        public void AddDeltaPosition(Vector2 deltaPosition, bool autoScaleFactor = true)
        {
            if (autoScaleFactor) {
                m_FloatingItemView.RectTransform.anchoredPosition += deltaPosition / m_Canvas.scaleFactor;
            } else {
                m_FloatingItemView.RectTransform.anchoredPosition += deltaPosition;
            }
        }

        /// <summary>
        /// Before dropping the item.
        /// </summary>
        public void BeforeDrop()
        {
            m_IsMoving = false;
        }

        /// <summary>
        /// Remove the Moving Item View once it is done.
        /// </summary>
        public void RemoveItemView()
        {
            if (m_SourceItemViewSlot != null) {
                SetItemViewAsMovingSource(m_SourceItemViewSlot, false);
                m_SourceItemViewSlot = null;
            }

            m_SlotEventData = null;

            if (m_FloatingItemView != null) {
                ObjectPool.Destroy(m_FloatingItemView.gameObject);
                m_FloatingItemView = null;
            }
            m_IsMoving = false;
        }

        /// <summary>
        /// Set the Item View as Moving, triggering the Modules.
        /// </summary>
        /// <param name="itemView">The item view to set as moving.</param>
        public virtual void SetItemViewAsMoving(ItemView itemView)
        {
            if (itemView.CanvasGroup == null) {
                Debug.LogWarning("Draggable Item Viewes MUST have a CanvasGroup component to be dropped.");
            } else {
                itemView.CanvasGroup.interactable = false;
                itemView.CanvasGroup.blocksRaycasts = false;
            }

            for (int i = 0; i < itemView.Modules.Count; i++) {
                if (itemView.Modules[i] is IViewModuleMovable movable) {
                    movable.SetAsMoving();
                }
            }
        }

        /// <summary>
        /// Set the Item View as the source.
        /// </summary>
        /// <param name="itemViewSlot">The source item view slot.</param>
        /// <param name="movingSource">Set as the source?</param>
        public virtual void SetItemViewAsMovingSource(ItemViewSlot itemViewSlot, bool movingSource)
        {
            for (int i = 0; i < itemViewSlot.ItemView.Modules.Count; i++) {
                if (itemViewSlot.ItemView.Modules[i] is IViewModuleMovable movable) {
                    movable.SetAsMovingSource(movingSource);
                }
            }
        }

        /// <summary>
        /// Drag stopped.
        /// </summary>
        public void DragEnded()
        {
            EventHandler.ExecuteEvent(gameObject, EventNames.c_ItemViewSlotCursorManagerGameobject_EndMove);

            RemoveItemView();
            if (m_SourceContainer != null) {
                m_SourceContainer.Draw();
                m_SourceContainer.OnDisableEvent -= HandleSourceViewSlotContainerDisabled;
            }
        }
    }
}