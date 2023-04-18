/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.InventoryCollections
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;

    /// <summary>
    /// Interface for item collection restrictions.
    /// </summary>
    public interface IItemRestriction
    {
        /// <summary>
        /// Initialize the item collection.
        /// </summary>
        /// <param name="inventory">The inventory.</param>
        /// <param name="force">Force to initialize.</param>
        void Initialize(IInventory inventory, bool force);

        /// <summary>
        /// Can the item be added to the item
        /// </summary>
        /// <param name="itemInfo">The item info to add.</param>
        /// <param name="receivingCollection">The receiving item Collection.</param>
        /// <returns>The itemInfo that can be added (can be null).</returns>
        ItemInfo? CanAddItem(ItemInfo itemInfo, ItemCollection receivingCollection);

        /// <summary>
        /// Can the item be removed from the itemCollection.
        /// </summary>
        /// <param name="itemInfo">The item info to remove (contains the itemCollection).</param>
        /// <returns>The item info that can be removed (can be null).</returns>
        ItemInfo? CanRemoveItem(ItemInfo itemInfo);
    }
}