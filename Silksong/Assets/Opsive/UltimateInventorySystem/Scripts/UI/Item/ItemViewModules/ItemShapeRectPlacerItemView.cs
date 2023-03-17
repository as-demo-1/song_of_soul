/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.Views;
    using UnityEngine;

    /// <summary>
    /// An Item View used to place a rect transform within the Item Shape.
    /// </summary>
    public class ItemShapeRectPlacerItemView : ItemViewModule
    {
        [Tooltip("The item shape Item view.")]
        [SerializeField] protected ItemShapeItemView m_ItemShapeItemView;
        [Tooltip("If true the rects will be place in the Item Shape Anchor?")]
        [SerializeField] protected bool m_PlaceOnAnchor;
        [Tooltip("Start searching a position top-left or bottom-right?")]
        [SerializeField] protected bool m_StartTopLeft;
        [Tooltip("Start searching a position horizontally or vertically?")]
        [SerializeField] protected bool m_HorizontalSearch;
        [Tooltip("The rect transforms to place within the item shape.")]
        [SerializeField] protected RectTransform[] m_RectsToPlace;

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
        /// Clear the view module.
        /// </summary>
        public override void Clear()
        {
            PlaceWithUnitOffset(new Vector2Int(0, 0));
        }

        /// <summary>
        /// Set the value for this item info.
        /// </summary>
        /// <param name="info">The info.</param>
        public override void SetValue(ItemInfo info)
        {
            if (info.Item == null) {
                Clear();
                return;
            }

            if (info.Item.TryGetAttributeValue<ItemShape>(m_ItemShapeItemView.ShapeAttributeName, out var itemShape) == false) {
                PlaceWithUnitOffset(new Vector2Int(0, 0));
                return;
            }

            if (m_PlaceOnAnchor) {
                PlaceWithUnitOffset(itemShape.Anchor);
                return;
            }

            if (m_StartTopLeft) {
                if (m_HorizontalSearch) {
                    for (int row = 0; row < itemShape.Rows; row++) {
                        for (int col = 0; col < itemShape.Cols; col++) {
                            if (itemShape.IsIndexOccupied(col, row)) {
                                PlaceWithUnitOffset(new Vector2Int(col, row));
                                return;
                            };
                        }
                    }
                } else {
                    for (int col = 0; col < itemShape.Cols; col++) {
                        for (int row = 0; row < itemShape.Rows; row++) {
                            if (itemShape.IsIndexOccupied(col, row)) {
                                PlaceWithUnitOffset(new Vector2Int(col, row));
                                return;
                            }

                            ;
                        }
                    }
                }
            } else {
                if (m_HorizontalSearch) {
                    for (int row = itemShape.Rows - 1; row >= 0; row--) {
                        for (int col = itemShape.Cols - 1; col >= 0; col--) {
                            if (itemShape.IsIndexOccupied(col, row)) {
                                PlaceWithUnitOffset(new Vector2Int(col, row));
                                return;
                            };
                        }
                    }
                } else {
                    for (int col = itemShape.Cols - 1; col >= 0; col--) {
                        for (int row = itemShape.Rows - 1; row >= 0; row--) {
                            if (itemShape.IsIndexOccupied(col, row)) {
                                PlaceWithUnitOffset(new Vector2Int(col, row));
                                return;
                            };
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Place the transform at a certain offset.
        /// </summary>
        /// <param name="pos">The offset position.</param>
        private void PlaceWithUnitOffset(Vector2Int pos)
        {
            var cellSize = m_ItemShapeItemView.CellSize;
            var position = new Vector2(pos.x * cellSize.x, -pos.y * cellSize.y);

            for (int i = 0; i < m_RectsToPlace.Length; i++) {
                var rect = m_RectsToPlace[i];

                rect.sizeDelta = cellSize;
                rect.anchoredPosition = position;
            }
        }
    }
}