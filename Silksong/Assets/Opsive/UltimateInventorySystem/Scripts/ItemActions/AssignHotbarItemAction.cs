/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.ItemActions
{
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using Opsive.UltimateInventorySystem.UI.Panels.ActionPanels;
    using Opsive.UltimateInventorySystem.UI.Panels.Hotbar;
    using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers;
    using UnityEngine;

    /// <summary>
    /// Demo item action used to assign items to the hotbar
    /// </summary>
    [System.Serializable]
    public class AssignHotbarItemAction : ItemActionWithAsyncFuncActionPanel<int>
    {
        [Tooltip("If the ID is 0 it will use the panel manager found on the parent panel.")]
        [SerializeField] protected uint m_DisplayPanelManagerID = 0;
        [Tooltip("The hotbar panel name must match the panel name exactly.")]
        [SerializeField] protected string m_HotbarPanelName = "Item Hotbar";
        [Tooltip("Toggle assign the items to the hotbar or assign them without the option to unassign.")]
        [SerializeField] protected bool m_ToggleAssign = false;
        [Tooltip("Show the item names within the list of options or show the index to equip the item to.")]
        [SerializeField] protected bool m_ShowItemNames = false;


        protected ItemHotbar m_ItemHotbar;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AssignHotbarItemAction()
        {
            m_Name = "Assign";
        }

        /// <summary>
        /// Invoke the action.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        protected override void InvokeActionInternal(ItemInfo itemInfo, ItemUser itemUser)
        {
            if (m_AsyncFuncActions.Count < m_ItemHotbar.SlotCount) {

                for (int i = m_AsyncFuncActions.Count; i < m_ItemHotbar.SlotCount; i++) {
                    var localIndex = i;

                    m_AsyncFuncActions.Add(new AsyncFuncAction<int>(
                        (localIndex + 1).ToString(),
                        () => localIndex));
                }
            } else if (m_AsyncFuncActions.Count > m_ItemHotbar.SlotCount) {
                m_AsyncFuncActions.Trim(m_ItemHotbar.SlotCount);
            }

            if (m_ShowItemNames) {
                for (int i = 0; i < m_ItemHotbar.SlotCount; i++) {

                    m_AsyncFuncActions[i].Name = m_ItemHotbar.GetItemAt(i).Item?.name ?? "Empty";
                }
            }

            base.InvokeActionInternal(itemInfo, itemUser);
        }

        /// <summary>
        /// Invoke action after waiting for index slot.
        /// </summary>
        /// <param name="itemInfo">The item.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        /// <param name="awaitedValue">The index slot.</param>
        protected override void InvokeWithAwaitedValue(ItemInfo itemInfo, ItemUser itemUser, int awaitedValue)
        {
            if (awaitedValue == -1 || m_AsyncFuncActionPanel.Canceled) {
                return;
            }
            if (m_ToggleAssign) {
                m_ItemHotbar.ToggleAssignItemToSlot(itemInfo, awaitedValue);
            } else {
                m_ItemHotbar.AddItem(itemInfo, awaitedValue);
            }
        }

        /// <summary>
        /// Can the action be invoked.
        /// </summary>
        /// <param name="itemInfo">The item Info.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        /// <returns>True if it can be invoked.</returns>
        protected override bool CanInvokeInternal(ItemInfo itemInfo, ItemUser itemUser)
        {
            var panelManager = InventorySystemManager.GetDisplayPanelManager(m_DisplayPanelManagerID);

            if (panelManager == null) {

                panelManager = m_PanelParentPanel?.Manager;

                if (panelManager == null) {
                    panelManager = GameObject.FindObjectOfType<DisplayPanelManager>();
                    if (panelManager == null) {
                        Debug.LogError("The Assign Hotbar Item Action could not find the display panel manager.");
                        return false;
                    }
                }
            }

            var hotbarPanel = panelManager.GetPanel(m_HotbarPanelName);

            if (hotbarPanel == null) {
                Debug.LogError($"The Assign Hotbar Item Action could not find the panel named : '{m_HotbarPanelName}' .");
                return false;
            }

            for (int i = 0; i < hotbarPanel.Bindings.Count; i++) {
                var binding = hotbarPanel.Bindings[i];
                if (binding is ItemViewSlotsContainerPanelBinding itemViewPanel) {
                    var itemViewSlotsContainer = itemViewPanel.ItemViewSlotsContainer;
                    m_ItemHotbar = itemViewSlotsContainer.gameObject.GetCachedComponent<ItemHotbar>();
                    break;
                }
            }

            if (m_ItemHotbar == null) {
                Debug.LogError($"The Assign Hotbar Item Action found the panel named : '{m_HotbarPanelName}' . But that panel did not have a Hotbar bound to it.", hotbarPanel);
                return false;
            }

            var item = itemInfo.Item;
            return itemInfo.ItemCollection.HasItem((1, item));
        }
    }
}