/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.ItemActions
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.Item;
    using Opsive.UltimateInventorySystem.UI.Item.DragAndDrop;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers;
    using UnityEngine;

    /// <summary>
    /// Simple item action used to move an item from one slot to another within the same grid.
    /// </summary>
    [System.Serializable]
    public class OpenOtherItemViewSlotContainerItemAction : ItemViewSlotsContainerItemAction
    {
        [Tooltip("The Display Panel Unique Name.")]
        [SerializeField] protected string m_DisplayUniqueName;

        protected DisplayPanel m_TargetDisplayPanel;
        protected ItemViewSlotsContainerBase m_TargetItemViewSlotsContainer;
        protected ItemViewSlotActionEvent m_ClickedAction;
        protected ItemViewDropHandler m_ItemViewDropHandler;
        protected ItemViewSlotEventData m_ItemViewSlotEventData;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public OpenOtherItemViewSlotContainerItemAction()
        {
            m_Name = "Select ExternalItemAction";
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="force">Force initialize.</param>
        public override void Initialize(bool force)
        {
            if (m_Initialized && !force ) { return; }

            base.Initialize(force);

            m_ItemViewSlotEventData = new ItemViewSlotEventData();
            m_ClickedAction = new ItemViewSlotActionEvent(HandleItemViewSlotClicked, true);
        }

        /// <summary>
        /// Set the Item View Slots Container.
        /// </summary>
        /// <param name="itemViewSlotsContainer">The item view slots container.</param>
        /// <param name="itemViewSlotIndex">The index of the selected Item View Slot.</param>
        public override void SetViewSlotsContainer(ItemViewSlotsContainerBase itemViewSlotsContainer, int itemViewSlotIndex)
        {
            base.SetViewSlotsContainer(itemViewSlotsContainer, itemViewSlotIndex);
            m_ItemViewDropHandler = m_ItemViewSlotsContainer.GetComponent<ItemViewDropHandler>();
        }

        /// <summary>
        /// Check if the action can be invoked.
        /// </summary>
        /// <param name="itemInfo">The item.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        /// <returns>True if the action can be invoked.</returns>
        protected override bool CanInvokeInternal(ItemInfo itemInfo, ItemUser itemUser)
        {
            var canInvoke = base.CanInvokeInternal(itemInfo, itemUser);
            if (canInvoke == false) { return false; }

            if (m_OriginDisplayPanel == null) { return false; }

            m_TargetDisplayPanel = m_OriginDisplayPanel.Manager.GetPanel(m_DisplayUniqueName);

            if (m_TargetDisplayPanel == null) { return false; }

            var panelBinding = m_TargetDisplayPanel.GetBinding<ItemViewSlotsContainerPanelBinding>();

            if (panelBinding == null) { return false; }

            m_TargetItemViewSlotsContainer = panelBinding.ItemViewSlotsContainer;

            return true;
        }

        /// <summary>
        /// Invoke the action.
        /// </summary>
        /// <param name="itemInfo">The item.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        protected override void InvokeActionInternal(ItemInfo itemInfo, ItemUser itemUser)
        {
            m_TargetDisplayPanel.OnClose += RemoveListeners;
            m_TargetDisplayPanel.Open(m_OriginDisplayPanel, m_OriginSelectable);

            if (m_ItemViewDropHandler != null) {
                m_ItemViewSlotEventData.SetValues(m_ItemViewSlotsContainer, m_ItemViewSlotIndex);
                m_ItemViewSlotsContainer.ItemViewSlotCursor.StartMove(m_ItemViewSlotEventData);
            }

            m_TargetItemViewSlotsContainer.SetOneTimeClickAction(m_ClickedAction);
            m_TargetItemViewSlotsContainer.OnItemViewSlotSelected += ItemViewSlotSelected;

            m_ItemViewSlotsContainer.ItemViewSlotCursor.SetPosition(m_TargetItemViewSlotsContainer.GetSelectedSlot().transform.position);
        }

        /// <summary>
        /// Remove the listeners.
        /// </summary>
        private void RemoveListeners()
        {
            m_TargetDisplayPanel.OnClose -= RemoveListeners;
            m_TargetItemViewSlotsContainer.OnItemViewSlotSelected -= ItemViewSlotSelected;
            m_TargetItemViewSlotsContainer.SetOneTimeClickAction(null);
            m_ItemViewSlotsContainer.ItemViewSlotCursor.RemoveItemView();
        }

        /// <summary>
        /// Handle the ITem View Slot clicked.
        /// </summary>
        /// <param name="sloteventdata">The item view slot event data.</param>
        private void HandleItemViewSlotClicked(ItemViewSlotEventData sloteventdata)
        {
            if (m_ItemViewDropHandler == null) {
                m_ItemViewSlotsContainer.AddItem(sloteventdata.ItemViewSlot.ItemInfo, m_ItemViewSlotIndex);
            } else {
                m_ItemViewDropHandler.HandleItemViewSlotDrop(sloteventdata);
            }

            m_ItemViewSlotsContainer.ItemViewSlotCursor.RemoveItemView();
            m_TargetDisplayPanel.Close();

            m_TargetItemViewSlotsContainer.OnItemViewSlotSelected -= ItemViewSlotSelected;
        }

        /// <summary>
        /// Th Item View Slot was selected.
        /// </summary>
        /// <param name="eventdata">The item view slot event data.</param>
        private void ItemViewSlotSelected(ItemViewSlotEventData eventdata)
        {
            if (m_ItemViewSlotsContainer.ItemViewSlotCursor.IsMovingItemView == false) { return; }
            m_ItemViewSlotsContainer.ItemViewSlotCursor.SetPosition(eventdata.ItemView.transform.position);
        }
    }
}