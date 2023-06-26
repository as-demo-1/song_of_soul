/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;

    /// <summary>
    /// An interface for components that have an inventory.
    /// </summary>
    public interface IInventoryDependent
    {
        /// <summary>
        /// Get and Set the inventory.
        /// </summary>
        Inventory Inventory { get; set; }
    }
}