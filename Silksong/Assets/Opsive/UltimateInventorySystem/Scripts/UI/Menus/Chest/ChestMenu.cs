/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Menus.Chest
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.Item;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// The Chest menu is used to display and retrieve items from a chest. 
    /// </summary>
    public class ChestMenu : InventoryPanelBinding
    {
        [Tooltip("The inventory Grid.")]
        [SerializeField] internal InventoryGrid m_InventoryGrid;
        [Tooltip("The take all button.")]
        [SerializeField] protected Button m_TakeAllButton;
        [Tooltip("The cancel button.")]
        [SerializeField] protected Button m_CancelButton;
        [Tooltip("The Quantity picker panel.")]
        [SerializeField] internal QuantityPickerPanel m_QuantityPickerPanel;

        protected IChest m_Chest;

        public InventoryGrid InventoryGrid => m_InventoryGrid;

        /// <summary>
        /// Setup the component.
        /// </summary>
        public override void Initialize(DisplayPanel display, bool force)
        {
            var wasInitialized = m_IsInitialized;
            if (wasInitialized && !force) { return; }
            base.Initialize(display, force);

            if (wasInitialized == false) {
                //Do it only once even if forced.
                m_TakeAllButton.onClick.AddListener(TakeAll);
                m_CancelButton.onClick.AddListener(() => m_DisplayPanel.Close());
            }

            m_InventoryGrid.Initialize(force);
            m_InventoryGrid.SetDisplayPanel(m_DisplayPanel);
            m_InventoryGrid.OnItemViewSlotClicked -= ItemClicked;
            m_InventoryGrid.OnItemViewSlotClicked += ItemClicked;

            FindAndBindChests();
        }

        /// <summary>
        /// Handle the inventory being bound.
        /// </summary>
        protected override void OnInventoryBound()
        { }

        /// <summary>
        /// Find and reference all the chests in the scene.
        /// </summary>
        protected virtual void FindAndBindChests()
        {
            var allChests = FindObjectsOfType<Chest>();
            for (int i = 0; i < allChests.Length; i++) {
                if(allChests[i].ChestMenu != null){ continue; }
                allChests[i].ChestMenu = this;
            }
        }

        /// <summary>
        /// Take all the items from the chest.
        /// </summary>
        private void TakeAll()
        {
            m_DisplayPanel.Close();

            m_Chest.Inventory.MainItemCollection.GiveAllItems(
                m_Inventory.MainItemCollection, null);
        }

        /// <summary>
        /// Handle the On Open event.
        /// </summary>
        public override void OnOpen()
        {
            base.OnOpen();

            m_QuantityPickerPanel.Close(false);

            m_InventoryGrid.SetInventory(m_Chest.Inventory);
            m_InventoryGrid.Draw();
            m_InventoryGrid.Grid.SelectButton(0);
        }

        /// <summary>
        /// Set the chest.
        /// </summary>
        /// <param name="chest">The chest.</param>
        public void SetChest(IChest chest)
        {
            m_Chest = chest;
        }

        /// <summary>
        /// Click an item amount.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="index">The index.</param>
        private void ItemClicked(ItemViewSlotEventData slotEventData)
        {
            var itemInfo = slotEventData.ItemViewSlot.ItemInfo;
            m_QuantityPickerPanel.Open(m_DisplayPanel, slotEventData.ItemViewSlot);

            m_QuantityPickerPanel.QuantityPicker.MinQuantity = 0;
            m_QuantityPickerPanel.QuantityPicker.MaxQuantity = itemInfo.Amount;

            m_QuantityPickerPanel.ConfirmCancelPanel.SetConfirmText("Take");
            m_QuantityPickerPanel.QuantityPicker.SetQuantity(itemInfo.Amount);

#pragma warning disable 4014
            WaitForQuantityDecision(itemInfo.Item);
#pragma warning restore 4014
        }

        /// <summary>
        /// Wait for the quantity picker to return an amount.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The task to await.</returns>
        private async Task WaitForQuantityDecision(Item item)
        {
            var quantity = await m_QuantityPickerPanel.WaitForQuantity();

            if (quantity < 1) { return; }

            m_Chest.Inventory.MainItemCollection.GiveItem(
                (ItemInfo)(item, quantity),
                m_Inventory.MainItemCollection,
                null);

            m_InventoryGrid.Draw();
        }

        /// <summary>
        /// Close the UI panel.
        /// </summary>
        public override void OnClose()
        {
            base.OnClose();

            if (m_Chest.Inventory.MainItemCollection.GetAllItemStacks().Count != 0) {
                m_Chest.Close();
            }
        }
    }
}
