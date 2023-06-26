/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.ItemObjectBehaviours
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.ItemActions;

    /// <summary>
    /// Interface for a usable item object.
    /// </summary>
    public interface IItemObjectBehaviourHandler
    {
        /// <summary>
        /// The item object.
        /// </summary>
        ItemObject ItemObject { get; }

        /// <summary>
        /// Use an item directly.
        /// </summary>
        /// <param name="itemUser">The item user.</param>
        /// <param name="itemActionIndex">The item action index to use.</param>
        void UseItem(ItemUser itemUser, int itemActionIndex);
    }
}