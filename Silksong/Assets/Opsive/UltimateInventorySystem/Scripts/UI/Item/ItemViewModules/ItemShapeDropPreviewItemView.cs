/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.Grid;
    using Opsive.UltimateInventorySystem.UI.Item.DragAndDrop;
    using Opsive.UltimateInventorySystem.UI.Views;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Item Shape Drop Preview.
    /// </summary>
    public class ItemShapeDropPreviewItemView : ItemViewModule, IItemViewSlotDropHoverSelectable, IViewModuleMovable
    {
        [Tooltip("The item shape item view.")]
        [SerializeField] protected ItemShapeItemView m_ItemShapeItemView;
        [Tooltip("The color filter image.")]
        [SerializeField] protected Image m_ColorFilter;
        [Tooltip("The preview color when no condition has passed.")]
        [SerializeField] protected Color m_NoConditionsPassed;
        [Tooltip("The preview condition when at least one condition has passed.")]
        [SerializeField] protected Color m_ConditionsPassed;
        [Tooltip("Clear Item View of the moving source.")]
        [SerializeField] protected bool m_ClearMovingSource;

        public ItemShapeGrid ItemShapeGrid => m_ItemShapeItemView.ItemShapeGrid;
        public ItemShapeGridData ItemShapeGridData => m_ItemShapeItemView.ItemShapeGrid.ItemShapeGridData;

        /// <summary>
        /// Set the item info.
        /// </summary>
        /// <param name="info">The item .</param>
        public override void SetValue(ItemInfo info)
        {
            if (info.Item == null || info.Item.IsInitialized == false) {
                Clear();
                return;
            }
            ResetIconColor();
        }

        /// <summary>
        /// Clear the component.
        /// </summary>
        public override void Clear()
        {
            m_ColorFilter.enabled = false;
            ResetIconColor();
        }

        private void ResetIconColor()
        {
            m_ItemShapeItemView.Icon.color = Color.white;
            if (m_ItemShapeItemView.ForegroundItemView != null) {
                m_ItemShapeItemView.ForegroundItemView.Icon.color = Color.white;
            }
        }

        /// <summary>
        /// Same Container, can the item be moved?
        /// </summary>
        /// <param name="dropHandler">The drop handler.</param>
        /// <returns>Can the items be moved.</returns>
        public bool SameContainerCanMove(ItemViewDropHandler dropHandler)
        {
            var sourcePos = ItemShapeGridData.OneDTo2D(dropHandler.SourceIndex);
            var destinationPos = ItemShapeGridData.OneDTo2D(dropHandler.DestinationIndex);

            if (dropHandler.SourceContainer == dropHandler.DestinationContainer) {
                return ItemShapeGridData.CanMoveIndex(sourcePos, destinationPos);
            }

            return false;
        }

        /// <summary>
        /// Select with an item view drop handler.
        /// </summary>
        /// <param name="dropHandler">The drop handler.</param>
        public virtual void SelectWith(ItemViewDropHandler dropHandler)
        {
            ClearGridColorPreview();

            PreviewColor(dropHandler);
        }

        /// <summary>
        /// Clear the grid color preview.
        /// </summary>
        protected void ClearGridColorPreview()
        {
            for (int i = 0; i < ItemShapeGrid.ItemViewSlots.Count; i++) {
                var view = ItemShapeGrid.GetItemViewAt(i);
                var itemShapePreview = view.GetViewModule<ItemShapeDropPreviewItemView>();

                itemShapePreview.m_ColorFilter.enabled = false;
            }
        }

        /// <summary>
        /// Preview the condition color.
        /// </summary>
        /// <param name="dropHandler">The item view drop handler.</param>
        private void PreviewColor(ItemViewDropHandler dropHandler)
        {
            var dropActionAndCondition =
                dropHandler.ItemViewSlotDropActionSet.GetFirstPassingCondition(dropHandler);
            var canDrop = dropActionAndCondition != null;
            var color = canDrop ? m_ConditionsPassed : m_NoConditionsPassed;

            var sourceItemInfo = dropHandler.SourceItemInfo;

            if (sourceItemInfo.Item == null ||
                sourceItemInfo.Item.TryGetAttributeValue<ItemShape>(ItemShapeGridData.ShapeAttributeName, out var itemShape) == false
                || itemShape.Count <= 1) {

                // Item takes a 1x1 shape.
                m_ColorFilter.color = color;
                m_ColorFilter.enabled = true;
                return;
            }

            var destinationAnchorPos = ItemShapeGridUtility.GetDestinationAnchorPos(dropHandler);

            for (int row = 0; row < itemShape.Rows; row++) {
                for (int col = 0; col < itemShape.Cols; col++) {

                    if (itemShape.IsIndexOccupied(col, row) == false) { continue; }

                    var position = new Vector2Int(
                        destinationAnchorPos.x + col,
                        destinationAnchorPos.y + row);

                    if (ItemShapeGridData.IsPositionValid(position) == false) { continue; };

                    var index = ItemShapeGridData.TwoDTo1D(position);

                    var view = ItemShapeGrid.GetItemViewAt(index);
                    var previewView = view.GetViewModule<ItemShapeDropPreviewItemView>();

                    previewView.m_ColorFilter.color = color;
                    previewView.m_ColorFilter.enabled = true;
                }
            }
        }

        /// <summary>
        /// Deselect the item.
        /// </summary>
        /// <param name="dropHandler">The drop handler.</param>
        public virtual void DeselectWith(ItemViewDropHandler dropHandler)
        {
            ClearGridColorPreview();
        }

        /// <summary>
        /// Set this modules as moving.
        /// </summary>
        public void SetAsMoving()
        {
        }

        /// <summary>
        /// Set this view as the moving source or not.
        /// </summary>
        /// <param name="movingSource">True if it is the moving source.</param>
        public void SetAsMovingSource(bool movingSource)
        {
            if (movingSource) {
                if (m_ClearMovingSource) {
                    m_ItemShapeItemView.ForegroundItemView.Clear();
                } else {
                    m_ItemShapeItemView.ForegroundItemView.SetAsItemSelectable(false);
                }
            }
        }
    }
}