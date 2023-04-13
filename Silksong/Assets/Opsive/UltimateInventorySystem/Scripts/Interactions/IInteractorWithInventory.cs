/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Interactions
{
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;

    /// <summary>
    /// An interactable with an inventory.
    /// </summary>
    public interface IInteractorWithInventory : IInteractor
    {
        /// <summary>
        /// Get the inventory.
        /// </summary>
        Inventory Inventory { get; }
    }
}