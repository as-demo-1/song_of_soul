/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.UI.Menus.Upgrade
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Demo.CharacterControl;
    using Opsive.UltimateInventorySystem.Demo.CharacterControl.Player;
    using Opsive.UltimateInventorySystem.Input;
    using Opsive.UltimateInventorySystem.UI.Item;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers;
    using UnityEngine;

    /// <summary>
    /// The upgrade menu used in the demo to upgrade items.
    /// </summary>
    public class UpgradeMenu : InventoryPanelBinding
    {

        [Tooltip("The upgradable items inventory grid.")]
        [SerializeField] protected InventoryGrid m_UpgradableItemsInventoryGrid;
        [Tooltip("The item upgrade panel.")]
        [SerializeField] protected ItemUpgradePanel m_ItemUpgradePanel;

        [Tooltip("The upgrade items inventory grid panel.")]
        [SerializeField] protected ItemViewSlotsContainerPanelBinding m_UpgradesInventoryGridPanelBinding;
        [Tooltip("The upgradable item description.")]
        [SerializeField] protected ItemDescriptionPanelBinding m_ItemDescriptionPanel;
        [Tooltip("The upgradable item description preview.")]
        [SerializeField] protected ItemDescriptionPanelBinding m_ItemPreviewDescriptionPanel;

        protected ItemInfo m_SelectedUpgradableItemInfo;
        protected int m_SelectedUpgradeSlot;
        protected CharacterStats m_CharacterStats;

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="display">The display panel.</param>
        /// <param name="force"></param>
        public override void Initialize(DisplayPanel display, bool force)
        {
            base.Initialize(display, force);

            m_UpgradableItemsInventoryGrid.Initialize(false);

            m_UpgradableItemsInventoryGrid.SetDisplayPanel(m_DisplayPanel);
            m_UpgradableItemsInventoryGrid.OnItemViewSlotClicked += OnItemClicked;
            m_ItemUpgradePanel.OnUpgradeSlotClicked += OnUpgradeSlotClicked;
            m_UpgradesInventoryGridPanelBinding.ItemViewSlotsContainer.OnItemViewSlotClicked += OnUpgradeItemClicked;
            m_UpgradesInventoryGridPanelBinding.ItemViewSlotsContainer.OnItemViewSlotSelected += OnUpgradeItemSelected;
            m_UpgradesInventoryGridPanelBinding.DisplayPanel.OnClose += ClosingUpgradeableInventory;

            m_ItemUpgradePanel.OnClose += () => m_ItemDescriptionPanel.DisplayPanel.Close();
            m_ItemUpgradePanel.OnItemChanged += UpgradeItemChanged;
        }

        /// <summary>
        /// Upgrade item has changed.
        /// </summary>
        private void UpgradeItemChanged()
        {
            m_ItemDescriptionPanel.ItemDescription.Refresh();
            m_UpgradableItemsInventoryGrid.Draw();
            m_UpgradesInventoryGridPanelBinding.ItemViewSlotsContainer.Draw();
        }

        /// <summary>
        /// An Inventory was bound to this object.
        /// </summary>
        protected override void OnInventoryBound()
        {
            m_CharacterStats = m_Inventory.GetComponent<PlayerCharacter>().CharacterStats;
            m_UpgradableItemsInventoryGrid.Initialize(false);
            m_UpgradableItemsInventoryGrid.SetInventory(m_Inventory);
            m_UpgradesInventoryGridPanelBinding.ItemViewSlotsContainer.Initialize(false);
            m_ItemUpgradePanel.UpgradeItemsInventory = m_Inventory;
        }

        /// <summary>
        /// The panel was opened.
        /// </summary>
        public override void OnOpen()
        {
            base.OnOpen();
            m_UpgradableItemsInventoryGrid.Draw();
            m_UpgradableItemsInventoryGrid.SelectSlot(0);
        }

        /// <summary>
        /// An upgradable item was clicked.
        /// </summary>
        /// <param name="inventoryGrid">The inventory grid.</param>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="index">The index.</param>
        protected virtual void OnItemClicked(ItemViewSlotEventData slotEventData)
        {
            m_SelectedUpgradableItemInfo = slotEventData.ItemViewSlot.ItemInfo;
            m_ItemUpgradePanel.ResetFromPreview();
            m_ItemUpgradePanel.SetUpgradeableItemInfo(m_SelectedUpgradableItemInfo);
            m_ItemUpgradePanel.Open(m_DisplayPanel, slotEventData.ItemViewSlot);
            m_ItemUpgradePanel.GetSlot(0).Select();
            m_ItemDescriptionPanel.DisplayPanel.Open();
            m_ItemDescriptionPanel.ItemDescription.SetValue(m_SelectedUpgradableItemInfo);
            m_UpgradesInventoryGridPanelBinding.DisplayPanel.Close(false);
        }

        /// <summary>
        /// A upgrade slot was clicked.
        /// </summary>
        /// <param name="item">The item in the slot.</param>
        /// <param name="index">The index.</param>
        private void OnUpgradeSlotClicked(Item item, int index)
        {
            m_ItemUpgradePanel.ResetFromPreview();
            m_SelectedUpgradeSlot = index;

            m_UpgradesInventoryGridPanelBinding.DisplayPanel.Open(m_ItemUpgradePanel, m_ItemUpgradePanel.GetSlot(index));
            m_UpgradesInventoryGridPanelBinding.ItemViewSlotsContainer.Draw();
            m_UpgradesInventoryGridPanelBinding.ItemViewSlotsContainer.SelectSlot(0);

            m_ItemPreviewDescriptionPanel.DisplayPanel.Open();
        }

        /// <summary>
        /// Closing the inner panel.
        /// </summary>
        private void ClosingUpgradeableInventory()
        {
            m_ItemPreviewDescriptionPanel.DisplayPanel.Close();
            m_ItemUpgradePanel.ResetFromPreview();
        }

        /// <summary>
        /// Upgrade item was clicked.
        /// </summary>
        /// <param name="inventoryGrid">The inventory grid.</param>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="index">The index.</param>
        private void OnUpgradeItemClicked(ItemViewSlotEventData slotEventData)
        {
            m_ItemUpgradePanel.SetUpgradeInSlot(slotEventData.ItemViewSlot.ItemInfo.Item, m_SelectedUpgradeSlot);
        }

        /// <summary>
        /// Upgrade item was selected.
        /// </summary>
        /// <param name="inventoryGrid">The inventory grid.</param>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="index">The index.</param>
        private void OnUpgradeItemSelected(ItemViewSlotEventData slotEventData)
        {
            var itemInfo = slotEventData.ItemViewSlot.ItemInfo;
            //Set the upgrade to get a preview
            m_ItemUpgradePanel.SetUpgradePreviewInSlot(itemInfo.Item, m_SelectedUpgradeSlot);
            m_ItemPreviewDescriptionPanel.ItemDescription.SetValue(m_SelectedUpgradableItemInfo);
            m_ItemUpgradePanel.ResetFromPreview();
        }

        /// <summary>
        /// The panel was closed.
        /// </summary>
        public override void OnClose()
        {
            base.OnClose();
            if (m_CharacterStats == null) {
                m_CharacterStats = m_Inventory.GetComponent<PlayerCharacter>().CharacterStats;
            }
            m_CharacterStats.UpdateStats();
        }
    }
}
