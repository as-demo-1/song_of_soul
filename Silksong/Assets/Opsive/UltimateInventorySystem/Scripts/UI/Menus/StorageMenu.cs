/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.UI.Menus.Storage
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.UI.Item;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers;
    using System;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.UI;
    using Text = Opsive.Shared.UI.Text;

    /// <summary>
    /// Storage menu is used to exchange items between to inventories.
    /// </summary>
    public class StorageMenu : InventoryPanelBinding
    {
        public enum MenuOpenOptions
        {
            OpenStoreSubMenu,
            OpenRetrieveSubMenu,
            OpenNoSubMenu
        }
        
        [Tooltip("The menu title text.")]
        [SerializeField] protected Text m_MenuTitle;
        [Tooltip("The storage Inventory.")]
        [SerializeField] protected Inventory m_StorageInventory;
        [Tooltip("The client inventory Grid.")]
        [SerializeField] internal InventoryGrid m_ClientInventoryGrid;
        [Tooltip("The storage inventory Grid.")]
        [SerializeField] internal InventoryGrid m_StorageInventoryGrid;
        [Tooltip("The quantity picker panel.")]
        [SerializeField] internal QuantityPickerPanel m_QuantityPickerPanel;

        [Tooltip("The store button.")]
        [SerializeField] protected Button m_StoreButton;
        [Tooltip("The retrieve button.")]
        [SerializeField] protected Button m_RetrieveButton;
        [Tooltip("The close menu button.")]
        [SerializeField] protected Button m_CloseButton;
        
        [Tooltip("Choose which submenu is opened on Main Menu Open.")]
        [SerializeField] protected MenuOpenOptions m_MenuOpenOptions;

        protected ItemInfo m_SelectedItemInfo;

        public InventoryGrid ClientInventoryGrid => m_ClientInventoryGrid;
        public InventoryGrid StorageInventoryGrid => m_StorageInventoryGrid;

        /// <summary>
        /// Initialize the storage menu.
        /// </summary>
        /// <param name="display">The display panel.</param>
        /// <param name="force">Force the initialize.</param>
        public override void Initialize(DisplayPanel display, bool force)
        {
            m_ClientInventoryGrid.Initialize(false);
            m_StorageInventoryGrid.Initialize(false);

            base.Initialize(display, force);

            m_StoreButton.onClick.RemoveAllListeners();
            m_StoreButton.onClick.AddListener(StoreClicked);

            m_RetrieveButton.onClick.RemoveAllListeners();
            m_RetrieveButton.onClick.AddListener(RetrieveClicked);

            m_CloseButton.onClick.RemoveAllListeners();
            m_CloseButton.onClick.AddListener(() => m_DisplayPanel.Close(true));

            if (m_StorageInventory != null) { SetStorageInventory(m_StorageInventory); }

            m_ClientInventoryGrid.OnItemViewSlotClicked += ClientItemClicked;
            m_StorageInventoryGrid.OnItemViewSlotClicked += StorageItemClicked;
        }

        /// <summary>
        /// Set the client inventory.
        /// </summary>
        protected override void OnInventoryBound()
        {
            m_ClientInventoryGrid.SetInventory(m_Inventory);
        }

        /// <summary>
        /// Set the storage inventory.
        /// </summary>
        /// <param name="inventory">The inventory.</param>
        public void SetStorageInventory(Inventory inventory)
        {
            m_StorageInventory = inventory;
            m_StorageInventoryGrid.SetInventory(m_StorageInventory);
        }

        /// <summary>
        /// Handle the On Open event.
        /// </summary>
        public override void OnOpen()
        {
            base.OnOpen();

            m_ClientInventoryGrid.Panel.Open();
            m_StorageInventoryGrid.Panel.Open();

            switch (m_MenuOpenOptions) {
                case MenuOpenOptions.OpenStoreSubMenu:
                    StoreClicked();
                    break;
                case MenuOpenOptions.OpenRetrieveSubMenu:
                    RetrieveClicked();
                    break;
                case MenuOpenOptions.OpenNoSubMenu:
                    StoreClicked();
                    m_ClientInventoryGrid.Panel.Close();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Open the store sub panel.
        /// </summary>
        private void StoreClicked()
        {
            m_MenuTitle.text = "Storage - Store";

            m_ClientInventoryGrid.Panel.Open(m_DisplayPanel, m_StoreButton);
            DrawInventories();
            m_ClientInventoryGrid.SelectSlot(0);
        }

        /// <summary>
        /// Open the retrieve sub panel.
        /// </summary>
        private void RetrieveClicked()
        {
            m_MenuTitle.text = "Storage - Retrieve";

            m_StorageInventoryGrid.Panel.Open(m_DisplayPanel, m_RetrieveButton);
            DrawInventories();
            m_StorageInventoryGrid.SelectSlot(0);
        }

        /// <summary>
        /// Draw both inventories.
        /// </summary>
        private void DrawInventories()
        {
            m_ClientInventoryGrid.Draw();
            m_StorageInventoryGrid.Draw();
        }


        /// <summary>
        /// An item in the storage was clicked.
        /// </summary>
        /// <param name="inventoryGrid">The inventory grid UI.</param>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="index">The index.</param>
        private void StorageItemClicked(ItemViewSlotEventData slotEventData)
        {
            m_SelectedItemInfo = slotEventData.ItemViewSlot.ItemInfo;
            m_QuantityPickerPanel.Open(m_StorageInventoryGrid.Panel, slotEventData.ItemViewSlot);

            m_QuantityPickerPanel.QuantityPicker.MinQuantity = 0;
            m_QuantityPickerPanel.QuantityPicker.MaxQuantity = m_SelectedItemInfo.Amount;

            m_QuantityPickerPanel.ConfirmCancelPanel.SetConfirmText("Retrieve");
            m_QuantityPickerPanel.QuantityPicker.SetQuantity(1);

#pragma warning disable 4014
            WaitForQuantityDecision(false);
#pragma warning restore 4014
        }

        /// <summary>
        /// The clients item was clicked.
        /// </summary>
        /// <param name="inventoryGrid">The inventory grid ui.</param>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="index">The index.</param>
        private void ClientItemClicked(ItemViewSlotEventData slotEventData)
        {
            m_SelectedItemInfo = slotEventData.ItemViewSlot.ItemInfo;
            m_QuantityPickerPanel.Open(m_ClientInventoryGrid.Panel, slotEventData.ItemViewSlot);

            m_QuantityPickerPanel.QuantityPicker.MinQuantity = 0;
            m_QuantityPickerPanel.QuantityPicker.MaxQuantity = m_SelectedItemInfo.Amount;

            m_QuantityPickerPanel.ConfirmCancelPanel.SetConfirmText("Store");
            m_QuantityPickerPanel.QuantityPicker.SetQuantity(1);

#pragma warning disable 4014
            WaitForQuantityDecision(true);
#pragma warning restore 4014
        }

        /// <summary>
        /// Wait for the player to choose a quantity.
        /// </summary>
        /// <param name="store">Store or Retrieve.</param>
        /// <returns>The task.</returns>
        private async Task WaitForQuantityDecision(bool store)
        {
            var quantity = await m_QuantityPickerPanel.WaitForQuantity();

            if (quantity < 1) { return; }

            if (store) {
                var clientItemCollection = m_SelectedItemInfo.Inventory == (IInventory)m_Inventory
                    ? m_SelectedItemInfo.ItemCollection
                    : m_Inventory.MainItemCollection;

                clientItemCollection.GiveItem(
                    (quantity, m_SelectedItemInfo),
                    m_StorageInventory.MainItemCollection,
                    (info => clientItemCollection.AddItem(info, info.ItemStack)));
            } else {
                m_StorageInventory.MainItemCollection.GiveItem(
                    (quantity, m_SelectedItemInfo),
                    m_Inventory.MainItemCollection,
                    (info => m_StorageInventory.MainItemCollection.AddItem(info, info.ItemStack)));
            }

            m_StorageInventoryGrid.Draw();
            m_ClientInventoryGrid.Draw();
        }
    }
}
