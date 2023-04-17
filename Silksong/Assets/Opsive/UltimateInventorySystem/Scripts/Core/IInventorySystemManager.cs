/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core
{
    /// <summary>
    /// The interface for the inventory system manager.
    /// </summary>
    public interface IInventorySystemManager
    {

        /// <summary>
        /// Check if the InventorySystemManager is initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Get the inventory system register.
        /// </summary>
        InventorySystemRegister Register { get; }

        /// <summary>
        /// Get the inventory system factory.
        /// </summary>
        InventorySystemFactory Factory { get; }
    }
}