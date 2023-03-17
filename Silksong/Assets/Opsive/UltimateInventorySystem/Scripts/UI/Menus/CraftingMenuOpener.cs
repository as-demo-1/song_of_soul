/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Menus
{
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.Crafting;
    using Opsive.UltimateInventorySystem.UI.Menus.Crafting;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using System.Collections;
    using Opsive.Shared.UI;
    using UnityEngine;

    /// <summary>
    /// Crafting Menu Opener.
    /// </summary>
    public class CraftingMenuOpener : InventoryPanelOpener<CraftingMenuBase>
    {
        [Tooltip("The Crafter to bind to the menu.")]
        [SerializeField] protected Crafter m_Crafter;

        /// <summary>
        /// Open the menu on for an inventory.
        /// </summary>
        /// <param name="inventory">The inventory.</param>
        public override void Open(Inventory inventory)
        {
            m_Menu.BindInventory(inventory);
            m_Menu.SetCrafter(m_Crafter);
            m_Menu.DisplayPanel.SmartOpen();
        }
    }
}
