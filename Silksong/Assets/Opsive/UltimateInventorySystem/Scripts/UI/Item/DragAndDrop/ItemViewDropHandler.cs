/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.DragAndDrop
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.Item.ItemViewModules;
    using UnityEngine;

    /// <summary>
    /// Interface for Item View Slot Drop Hover Selectable, used to preview that a drop could happen.
    /// </summary>
    public interface IItemViewSlotDropHoverSelectable
    {
        /// <summary>
        /// Select the slot with a drop handler.
        /// </summary>
        /// <param name="dropHandler">The drop handler.</param>
        void SelectWith(ItemViewDropHandler dropHandler);

        /// <summary>
        /// Deselect the slot with a drop handler.
        /// </summary>
        /// <param name="dropHandler">The drop handler.</param>
        void DeselectWith(ItemViewDropHandler dropHandler);
    }

    /// <summary>
    /// The Item View Slot Drop Handler Stream Data goes through the conditions and actions.
    /// </summary>
    public class ItemViewSlotDropHandlerStreamData
    {
        public ItemViewSlotEventData DragSlotEventData { get; set; }
        public ItemViewSlotsContainerBase SourceContainer { get; set; }
        public ItemViewSlot SourceItemViewSlot { get; set; }
        public ItemInfo SourceItemInfo { get; set; }
        public int SourceIndex { get; set; }

        public ItemViewSlotEventData DropSlotEventData { get; set; }
        public ItemViewSlotsContainerBase DestinationContainer { get; set; }
        public ItemViewSlot DestinationItemViewSlot { get; set; }
        public ItemInfo DestinationItemInfo { get; set; }
        public int DestinationIndex { get; set; }

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public ItemViewSlotDropHandlerStreamData()
        {
        }

        /// <summary>
        /// Reset the stream data.
        /// </summary>
        /// <param name="sourceItemViewSlot">The source item view slot.</param>
        /// <param name="dragSlotEventData">The drag slot event data.</param>
        /// <param name="dropSlotEventData">The drop slot event data.</param>
        public virtual void Reset(ItemViewSlot sourceItemViewSlot, ItemViewSlotEventData dragSlotEventData, ItemViewSlotEventData dropSlotEventData)
        {
            DragSlotEventData = dragSlotEventData;
            SourceContainer = dragSlotEventData.ItemViewSlotsContainer;
            SourceItemViewSlot = sourceItemViewSlot;
            SourceItemInfo = dragSlotEventData.ItemView.CurrentValue;
            SourceIndex = dragSlotEventData.Index;

            DropSlotEventData = dropSlotEventData;
            DestinationContainer = dropSlotEventData.ItemViewSlotsContainer;
            DestinationItemViewSlot = dropSlotEventData.ItemViewSlot;
            DestinationItemInfo = dropSlotEventData.ItemView.CurrentValue;
            DestinationIndex = dropSlotEventData.Index;
        }
    }

    /// <summary>
    /// The item view drop handler.
    /// </summary>
    public class ItemViewDropHandler : MonoBehaviour
    {
        [Tooltip("This field shows the index of the last condition that past.")]
        [SerializeField] protected int m_DebugPassedConditionIndex;
        [Tooltip("The cursor Manager ID, used to get the Cursor Manager from anywhere in the scene.")]
        [SerializeField] protected uint m_CursorManagerID = 1;
        [Tooltip("The item view cursor manager.")]
        [SerializeField] protected ItemViewSlotCursorManager m_ItemViewSlotCursorManager;
        [Tooltip("Should the item be dropped on the slot under the mouse or on the last selected ItemView which can be selected by keyboard or by code?")]
        [SerializeField] protected bool m_DropOnLastSelectedView;

        [Tooltip("The Item View Slot Drop Action Set.")]
        [SerializeField] internal ItemViewSlotDropActionSet m_ItemViewSlotDropActionSet;

        protected ItemViewSlotsContainerBase m_ViewSlotsContainer;
        protected bool m_IsInitialized = false;
        protected ItemViewSlotDropHandlerStreamData m_StreamData;
        protected ItemViewSlotEventData m_DropSlotEventData;

        public uint CursorManagerID => m_CursorManagerID;
        public ItemViewSlotCursorManager SlotCursorManager => m_ItemViewSlotCursorManager;
        public ItemViewSlotDropHandlerStreamData StreamData => m_StreamData;

        public ItemViewSlotsContainerBase SourceContainer => m_ItemViewSlotCursorManager.SlotEventData.ItemViewSlotsContainer;

        public ItemViewSlot SourceItemViewSlot => m_ItemViewSlotCursorManager.SourceItemViewSlot;

        public ItemInfo SourceItemInfo => m_ItemViewSlotCursorManager.SourceItemViewSlot.ItemInfo;

        public int SourceIndex => m_ItemViewSlotCursorManager.SlotEventData.Index;

        public ItemViewSlotEventData DropSlotEventData => m_DropSlotEventData;

        public ItemView DestinationItemView => m_DropSlotEventData?.ItemView;

        public ItemInfo DestinationItemInfo => DestinationItemView?.CurrentValue ?? (0, null, null);

        public ItemViewSlotsContainerBase DestinationContainer => m_DropSlotEventData?.ItemViewSlotsContainer;

        public int DestinationIndex => m_DropSlotEventData?.Index ?? -1;

        public ItemViewSlotDropActionSet ItemViewSlotDropActionSet {
            get => m_ItemViewSlotDropActionSet;
            set => m_ItemViewSlotDropActionSet = value;
        }

        public ItemViewSlotsContainerBase ViewSlotsContainer {
            get => m_ViewSlotsContainer;
            set => m_ViewSlotsContainer = value;
        }

        /// <summary>
        /// Initialize.
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

            if (m_ItemViewSlotCursorManager == null) {
                m_ItemViewSlotCursorManager = InventorySystemManager.GetItemViewSlotCursorManager(m_CursorManagerID);
                if (m_ItemViewSlotCursorManager == null) {
                    m_ItemViewSlotCursorManager = GetComponentInParent<ItemViewSlotCursorManager>();
                    if (m_ItemViewSlotCursorManager == null) {
                        Debug.LogWarning("The item view cursor manager is missing, please add one on your canvas.");
                    }
                }
            }

            if (m_ItemViewSlotDropActionSet != null) {
                m_ItemViewSlotDropActionSet.Initialize(false);
            }

            //Listen to the destination container events.
            m_ViewSlotsContainer = GetComponent<ItemViewSlotsContainerBase>();

            if (m_ViewSlotsContainer != null) {
                m_ViewSlotsContainer.OnItemViewSlotDropE += HandleItemViewSlotDrop;

                m_ViewSlotsContainer.OnItemViewSlotSelected += ItemViewSlotSelected;
                m_ViewSlotsContainer.OnItemViewSlotDeselected += ItemViewSlotDeselected;
            }

            m_StreamData = new ItemViewSlotDropHandlerStreamData();

            m_IsInitialized = true;
        }

        /// <summary>
        /// Handle the Item View Slot Drop.
        /// </summary>
        /// <param name="dropSlotEventData">The drop slot event data.</param>
        public void HandleItemViewSlotDrop(ItemViewSlotEventData dropSlotEventData)
        {
            var dragEventData = m_ItemViewSlotCursorManager.SlotEventData;

            if (dragEventData == null) {
                Debug.LogWarning("dragEventData == null");
                return;
            }

            //Only drop if the drag and drop pointer ID is the same.
            if (dragEventData.PointerID != dropSlotEventData.PointerID) {
                return;
            }

            if (m_DropOnLastSelectedView) {
                var previousItemViewSlotContainer = m_DropSlotEventData.ItemViewSlotsContainer;
                var previousIndex = m_DropSlotEventData.Index;
                m_DropSlotEventData = dropSlotEventData;
                m_DropSlotEventData.SetValues(previousItemViewSlotContainer, previousIndex);
            } else {
                m_DropSlotEventData = dropSlotEventData;
            }

            var sourceItemViewSlot = m_ItemViewSlotCursorManager.SourceItemViewSlot;

            m_StreamData.Reset(sourceItemViewSlot, dragEventData, m_DropSlotEventData);

            m_ItemViewSlotCursorManager.BeforeDrop();
            HandleItemViewSlotDropInternal();
            m_ItemViewSlotCursorManager.RemoveItemView();
        }

        /// <summary>
        /// Handle the Drop.
        /// </summary>
        protected virtual void HandleItemViewSlotDropInternal()
        {
            if (m_ItemViewSlotDropActionSet == null) { return; }
            m_ItemViewSlotDropActionSet.HandleItemViewSlotDrop(this);

            if (SourceContainer != null) {
                SourceContainer.Draw();
            }

            if (DestinationContainer != null && SourceContainer != DestinationContainer) {
                DestinationContainer.Draw();
            }
        }

        /// <summary>
        /// An Item View slot was selected.
        /// </summary>
        /// <param name="eventdata">The event data.</param>
        public void ItemViewSlotSelected(ItemViewSlotEventData eventdata)
        {
            if (m_ItemViewSlotCursorManager.IsMovingItemView == false) { return; }
            
            m_DropSlotEventData = eventdata;
            m_StreamData.Reset(m_ItemViewSlotCursorManager.SourceItemViewSlot, m_ItemViewSlotCursorManager.SlotEventData, eventdata);

            m_DebugPassedConditionIndex = m_ItemViewSlotDropActionSet.GetFirstPassingConditionIndex(this);

            var selectedItemView = eventdata.ItemView;

            // Select the item view.
            for (int i = 0; i < selectedItemView.Modules.Count; i++) {
                if (selectedItemView.Modules[i] is IItemViewSlotDropHoverSelectable module) {
                    module.SelectWith(this);
                }
            }
        }

        /// <summary>
        /// An item View slot was deselected.
        /// </summary>
        /// <param name="slotEventData">The event data.</param>
        public void ItemViewSlotDeselected(ItemViewSlotEventData slotEventData)
        {
            var itemView = slotEventData.ItemView;
            if (m_ItemViewSlotCursorManager.IsMovingItemView == false) { return; }

            for (int i = 0; i < itemView.Modules.Count; i++) {
                if (itemView.Modules[i] is IItemViewSlotDropHoverSelectable module) {
                    module.DeselectWith(this);
                }
            }
        }
    }
}