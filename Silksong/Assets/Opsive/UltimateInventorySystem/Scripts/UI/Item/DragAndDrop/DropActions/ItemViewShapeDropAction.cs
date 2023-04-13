/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.DragAndDrop.DropActions
{
    using Opsive.UltimateInventorySystem.UI.Grid;
    using System;
    using UnityEngine;

    /// <summary>
    /// The Item View Drop Shape Condition.
    /// </summary>
    [Serializable]
    public class ItemViewShapeDropCondition : ItemViewDropCondition
    {
        /// <summary>
        /// Can the item be dropped.
        /// </summary>
        /// <param name="itemViewDropHandler">The Item View Drop Handler.</param>
        /// <returns>True if it can be dropped.</returns>
        public override bool CanDrop(ItemViewDropHandler itemViewDropHandler)
        {

            if (itemViewDropHandler.SourceContainer == itemViewDropHandler.DestinationContainer) { return false; }

            if (!(itemViewDropHandler.DestinationContainer is ItemShapeGrid destinationItemShapeGrid)) { return false; }

            var sourceCanGive = itemViewDropHandler.SourceContainer.CanGiveItem(
                itemViewDropHandler.SourceItemInfo,
                itemViewDropHandler.SourceIndex);

            if (sourceCanGive == false) { return false; }

            var destinationIndex = ItemShapeGridUtility.GetDestinationAnchorIndex(itemViewDropHandler);

            return destinationItemShapeGrid.CanAddItem(itemViewDropHandler.SourceItemInfo,
                destinationIndex);

        }
    }

    /// <summary>
    /// The Item View Drop Shape Action.
    /// </summary>
    [Serializable]
    public class ItemViewShapeDropAction : ItemViewDropAction
    {

        /// <summary>
        /// Drop Action.
        /// </summary>
        /// <param name="itemViewDropHandler">The Item View Drop Handler.</param>
        public override void Drop(ItemViewDropHandler itemViewDropHandler)
        {
            if (!(itemViewDropHandler.DestinationContainer is ItemShapeGrid destinationItemShapeGrid)) {
                Debug.LogError("Cannot drop Item Shape if the destination is not an Item Shape Grid.");

                return;
            }

            var destinationIndex = ItemShapeGridUtility.GetDestinationAnchorIndex(itemViewDropHandler);

            itemViewDropHandler.StreamData.SourceItemInfo =
                itemViewDropHandler.SourceContainer.RemoveItem(
                    itemViewDropHandler.StreamData.SourceItemInfo,
                    itemViewDropHandler.SourceIndex);

            itemViewDropHandler.DestinationContainer.AddItem(
                itemViewDropHandler.StreamData.SourceItemInfo,
                destinationIndex);
        }
    }
}