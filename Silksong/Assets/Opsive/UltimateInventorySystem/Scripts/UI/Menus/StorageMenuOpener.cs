/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.Interaction.Interactables
{
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.Demo.UI.Menus.Storage;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using UnityEngine;

    /// <summary>
    /// Storage interactable behavior.
    /// </summary>
    public class StorageMenuOpener : InventoryPanelOpener<StorageMenu>
    {
        [Tooltip("The storage inventory.")]
        [SerializeField] protected Inventory m_StorageInventory;

        /// <summary>
        /// Open the menu.
        /// </summary>
        /// <param name="interactor">The inventory.</param>
        public override void Open(Inventory inventory)
        {
            m_Menu.BindInventory(inventory);
            m_Menu.SetStorageInventory(m_StorageInventory);
            m_Menu.DisplayPanel.SmartOpen();
        }
    }
}