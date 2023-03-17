/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.ItemActions
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.Item;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Simple item action used to move an item from one slot to another within the same grid.
    /// </summary>
    [System.Serializable]
    public abstract class ItemViewSlotsContainerItemAction : ItemAction, IActionWithPanel
    {
        protected ItemViewSlotsContainerBase m_ItemViewSlotsContainer;
        protected int m_ItemViewSlotIndex;

        protected DisplayPanel m_OriginDisplayPanel;
        protected Selectable m_OriginSelectable;

        /// <summary>
        /// Check if the action can be invoked.
        /// </summary>
        /// <param name="itemInfo">The item.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        /// <returns>True if the action can be invoked.</returns>
        protected override bool CanInvokeInternal(ItemInfo itemInfo, ItemUser itemUser)
        {
            return m_ItemViewSlotsContainer != null;
        }

        /// <summary>
        /// Set the Item View Slots Container.
        /// </summary>
        /// <param name="itemViewSlotsContainer">The item View Slots Container.</param>
        /// <param name="itemViewSlotIndex">The index of the selected Item View Slot.</param>
        public virtual void SetViewSlotsContainer(ItemViewSlotsContainerBase itemViewSlotsContainer, int itemViewSlotIndex)
        {
            m_ItemViewSlotsContainer = itemViewSlotsContainer;
            m_ItemViewSlotIndex = itemViewSlotIndex;
        }

        /// <summary>
        /// Set the Parent Display Panel
        /// </summary>
        /// <param name="parentDisplayPanel">The parent Display Panel.</param>
        /// <param name="previousSelectable">The previous selectable.</param>
        /// <param name="parentTransform">The parent Transform.</param>
        public virtual void SetParentPanel(DisplayPanel parentDisplayPanel, Selectable previousSelectable, Transform parentTransform)
        {
            m_OriginDisplayPanel = parentDisplayPanel;
            m_OriginSelectable = previousSelectable;
        }
    }
}