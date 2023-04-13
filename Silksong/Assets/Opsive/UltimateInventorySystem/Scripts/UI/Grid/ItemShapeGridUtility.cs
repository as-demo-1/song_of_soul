/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Grid
{
    using Opsive.UltimateInventorySystem.UI.Item;
    using Opsive.UltimateInventorySystem.UI.Item.DragAndDrop;
    using UnityEngine;

    /// <summary>
    /// Item Shape Grid Utility script with static functions to help with shapes.
    /// </summary>
    public static class ItemShapeGridUtility
    {
        /// <summary>
        /// Get the destination anchor index for an item view drop handler.
        /// </summary>
        /// <param name="dropHandler">The item view drop handler.</param>
        /// <returns>The index of the destination anchor.</returns>
        public static int GetDestinationAnchorIndex(ItemViewDropHandler dropHandler)
        {
            if (!(dropHandler.DestinationContainer is ItemShapeGrid destinationItemShapeGrid)) {
                return -1;
            }

            var destinationItemShapeGridData = destinationItemShapeGrid.ItemShapeGridData;

            return destinationItemShapeGridData.TwoDTo1D(GetDestinationAnchorPos(dropHandler));
        }

        /// <summary>
        /// Get the destination anchor position for an item view drop handler.
        /// </summary>
        /// <param name="dropHandler">The item view drop handler.</param>
        /// <returns>The position of the destination anchor.</returns>
        public static Vector2Int GetDestinationAnchorPos(ItemViewDropHandler dropHandler)
        {
            if (!(dropHandler.DestinationContainer is ItemShapeGrid destinationItemShapeGrid)) {
                return new Vector2Int(-1, -1);
            }

            var destinationItemShapeGridData = destinationItemShapeGrid.ItemShapeGridData;

            var sourceItemInfo = dropHandler.SourceItemInfo;

            if (sourceItemInfo.Item == null ||
                sourceItemInfo.Item.TryGetAttributeValue<ItemShape>(destinationItemShapeGridData.ShapeAttributeName, out var itemShape) == false
                || itemShape.Count <= 1) {

                // Item takes a 1x1 shape.
                return destinationItemShapeGridData.OneDTo2D(dropHandler.DestinationIndex);
            }

            var destinationPos = destinationItemShapeGridData.OneDTo2D(dropHandler.DestinationIndex);

            Vector2Int sourceAnchorOffset = Vector2Int.zero;

            // If the item source comes from a shape grid the offset needs to be taken into account.
            if (dropHandler.SourceContainer is ItemShapeGrid sourceItemShapeGrid) {

                var sourceItemShapeGridData = sourceItemShapeGrid.ItemShapeGridData;
                var sourcePosition = sourceItemShapeGridData.OneDTo2D(dropHandler.SourceIndex);

                var hasAnchorOffset = sourceItemShapeGridData.TryGetAnchorOffset(
                    sourceItemInfo,
                    sourcePosition,
                    out sourceAnchorOffset);

                if (hasAnchorOffset == false) {
                    Debug.LogWarning("The source item anchor was not found for " + sourceItemInfo + " at position " + sourcePosition);
                    return new Vector2Int(-1, -1);
                }
            }

            return new Vector2Int(
                sourceAnchorOffset.x + destinationPos.x - itemShape.Anchor.x,
                sourceAnchorOffset.y + destinationPos.y - itemShape.Anchor.y);
        }

    }
}