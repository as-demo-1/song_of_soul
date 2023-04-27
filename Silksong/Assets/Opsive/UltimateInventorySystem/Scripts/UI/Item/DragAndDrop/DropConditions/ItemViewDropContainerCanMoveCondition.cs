/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.DragAndDrop.DropConditions
{
    using System;

    /// <summary>
    /// The Item View Drop Container Can Move Condition.
    /// </summary>
    [Serializable]
    public class ItemViewDropContainerCanMoveCondition : ItemViewDropCondition
    {

        /// <summary>
        /// Can the item be dropped.
        /// </summary>
        /// <param name="itemViewDropHandler">The Item View Drop Handler.</param>
        /// <returns>True if it can be dropped.</returns>
        public override bool CanDrop(ItemViewDropHandler itemViewDropHandler)
        {
            if (itemViewDropHandler.SourceContainer != itemViewDropHandler.DestinationContainer) { return false; }

            return itemViewDropHandler.SourceContainer.CanMoveItem(
                itemViewDropHandler.SourceIndex, itemViewDropHandler.DestinationIndex);
        }
    }
}