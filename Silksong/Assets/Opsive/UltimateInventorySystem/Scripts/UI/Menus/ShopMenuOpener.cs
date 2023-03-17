/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Menus
{
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.Exchange.Shops;
    using Opsive.UltimateInventorySystem.UI.Menus.Shop;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using UnityEngine;

    /// <summary>
    /// Shop interactable behavior.
    /// </summary>
    public class ShopMenuOpener : InventoryPanelOpener<ShopMenu>
    {
        [Tooltip("The shop.")]
        [SerializeField] protected ShopBase m_Shop;

        /// <summary>
        /// Open the menu on for an inventory.
        /// </summary>
        /// <param name="inventory">The inventory.</param>
        public override void Open(Inventory inventory)
        {
            m_Menu.BindInventory(inventory);
            m_Menu.SetShop(m_Shop);
            m_Menu.DisplayPanel.SmartOpen();
        }
    }
}