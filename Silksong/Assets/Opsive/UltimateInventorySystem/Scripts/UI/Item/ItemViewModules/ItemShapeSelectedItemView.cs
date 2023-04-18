/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.Grid;
    using Opsive.UltimateInventorySystem.UI.Views;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// An Item View used to show the item being selected.
    /// </summary>
    public class ItemShapeSelectedItemView : ItemViewModuleSelectable
    {
        [Tooltip("The item shape item view.")]
        [SerializeField] protected ItemShapeItemView m_ItemShapeItemView;
        [Tooltip("The color filter image.")]
        [SerializeField] protected Image m_ColorFilter;
        [Tooltip("The preview color when no condition has passed.")]
        [SerializeField] protected Color m_SelectedColor;

        public ItemShapeGrid ItemShapeGrid => m_ItemShapeItemView.ItemShapeGrid;
        public ItemShapeGridData ItemShapeGridData => m_ItemShapeItemView.ItemShapeGrid.ItemShapeGridData;

        /// <summary>
        /// Initialize the view module.
        /// </summary>
        /// <param name="view">The view for this module.</param>
        public override void Initialize(View view)
        {
            base.Initialize(view);
            m_ItemShapeItemView.OnGridInfoSet -= OnGridInfoSet;
            m_ItemShapeItemView.OnGridInfoSet += OnGridInfoSet;
        }

        /// <summary>
        /// Handle the grid Info was set.
        /// </summary>
        private void OnGridInfoSet()
        {
            SetValue(m_View.CurrentValue);
        }

        /// <summary>
        /// Clear the item info.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
        }

        /// <summary>
        /// Set the item info value.
        /// </summary>
        /// <param name="info">The item info.</param>
        public override void SetValue(ItemInfo info)
        {
            if (ItemInfo.Item == null) {
                Clear();
            }
        }

        /// <summary>
        /// Select the item info.
        /// </summary>
        /// <param name="select">Select.</param>
        public override void Select(bool select)
        {
            if (ItemShapeGrid == null) { return; }

            if (select) {
                var itemInfo = m_ItemShapeItemView.IsItemSelectable ? ItemInfo : ItemInfo.None;
                UpdateFilter(itemInfo, m_ItemShapeItemView.AnchorPosition, m_SelectedColor, select);
            } else {
                UpdateFilter(ItemInfo, m_ItemShapeItemView.AnchorPosition, m_SelectedColor, select);
            }
        }

        /// <summary>
        /// Update the color of the selected item view shape.
        /// </summary>
        protected virtual void UpdateFilter(ItemInfo itemInfo, Vector2Int itemPosition, Color color, bool enableFilter)
        {
            if (itemInfo.Item == null ||
                itemInfo.Item.TryGetAttributeValue<ItemShape>(ItemShapeGridData.ShapeAttributeName, out var itemShape) == false
                || itemShape.Count <= 1) {

                // Item takes a 1x1 shape.
                m_ColorFilter.color = color;
                m_ColorFilter.enabled = enableFilter;
                return;
            }

            for (int row = 0; row < itemShape.Rows; row++) {
                for (int col = 0; col < itemShape.Cols; col++) {

                    if (itemShape.IsIndexOccupied(col, row) == false) { continue; }

                    var position = new Vector2Int(
                        itemPosition.x + col,
                        itemPosition.y + row);

                    if (ItemShapeGridData.IsPositionValid(position) == false) { continue; };

                    var index = ItemShapeGridData.TwoDTo1D(position);

                    if (index == -1) { continue; }

                    var view = ItemShapeGrid.GetItemViewAt(index);

                    if (view == null) { continue; }

                    var previewView = view.GetViewModule<ItemShapeSelectedItemView>();

                    previewView.m_ColorFilter.color = color;
                    previewView.m_ColorFilter.enabled = enableFilter;
                }
            }
        }
    }
}