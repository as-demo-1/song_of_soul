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
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class ItemShapeItemView : ItemViewModule, IViewModuleMovable
    {
        public event Action OnGridInfoSet;

        [Tooltip("The shape attribute name.")]
        [SerializeField] protected string m_ShapeAttributeName = "Shape";
        [Tooltip("The size of a box.")]
        [SerializeField] protected Vector2 m_DefaultSlotSize = new Vector2(100, 100);
        [Tooltip("Resize the content to one cell size. Useful for background effects.")]
        [SerializeField] protected RectTransform m_CellResizableContent;
        [Tooltip("Resize the content to the shape size. Used for the parent transform of the icon.")]
        [SerializeField] protected RectTransform m_ShapeResizableContent;
        [Tooltip("The icon image.")]
        [SerializeField] protected Image m_Icon;
        [Tooltip("The shape attribute name.")]
        [SerializeField] protected string m_ShapeIconAttributeName = "Icon";
        [Tooltip("Should the default Icon be used if the Shape Icon is null?")]
        [SerializeField] protected bool m_UseBackupIcon = true;
        [Tooltip("The missing item icon sprite.")]
        [SerializeField] protected Sprite m_MissingIcon;
        [Tooltip("Disable the image component if item is null.")]
        [SerializeField] protected bool m_DisableOnClear;

        [Tooltip("The size of a box.")]
        [SerializeField] protected GameObject[] m_ForegroundActive;
        [Tooltip("The size of a box.")]
        [SerializeField] protected GameObject[] m_BackgroundActive;
        [Tooltip("The color when the item can be selected.")]
        [SerializeField] protected Color m_SelectableColor = Color.white;
        [Tooltip("The color when the item cannot be selected.")]
        [SerializeField] protected Color m_UnselectableColor = new Color(0.8f, 0.8f, 0.8f, 0.3f);

        protected bool m_IsItemSelectable;

        protected ItemShapeItemView m_ForegroundItemView;
        protected ItemShapeGrid m_ItemShapeGrid;
        protected int m_Index;
        protected bool m_Foreground;

        public Image Icon => m_Icon;

        public RectTransform CellResizableContent => m_CellResizableContent; 
        public RectTransform ShapeResizableContent => m_ShapeResizableContent; 
        public bool IsItemSelectable => m_IsItemSelectable && (ForegroundItemView?.m_IsItemSelectable ?? true);

        public string ShapeAttributeName => m_ShapeAttributeName;
        public Vector2 CellSize => m_ItemShapeGrid?.ItemShapeSize ?? m_DefaultSlotSize;
        public ItemShapeGrid ItemShapeGrid => m_ItemShapeGrid;
        public int Index => m_Index;
        public int AnchorIndex => m_ItemShapeGrid?.ItemShapeGridData?.GetAnchorIndex(m_Index) ?? -1;
        public Vector2Int Position => m_ItemShapeGrid?.ItemShapeGridData?.OneDTo2D(m_Index) ?? new Vector2Int(-1, -1);
        public Vector2Int AnchorPosition => m_ItemShapeGrid?.ItemShapeGridData?.GetAnchorPosition(m_Index) ?? new Vector2Int(-1, -1);

        public ItemShapeItemView ForegroundItemView {
            get {
                if (m_Foreground) {
                    return this;
                }

                if (IsAnchor) {
                    return m_ForegroundItemView;
                }

                if (m_ItemShapeGrid == null || m_ItemShapeGrid.ItemShapeGridData == null) {
                    return null;
                }

                if (m_ItemShapeGrid.ItemShapeGridData.TryFindAnchorForItem(
                    (ItemInfo)m_ItemShapeGrid.ItemShapeGridData.GetElementAt(m_Index).ItemStack,
                    out var anchorPos)) {

                    var anchorItemView = m_ItemShapeGrid.GetItemViewAt(
                        m_ItemShapeGrid.ItemShapeGridData.TwoDTo1D(anchorPos));

                    if (anchorItemView == View) {
                        Debug.LogWarning("The anchorItemView is not set as the anchor", gameObject);
                        return m_ForegroundItemView;
                    }

                    return anchorItemView.GetViewModule<ItemShapeItemView>().ForegroundItemView;
                }

                return m_ForegroundItemView;
            }

            set { m_ForegroundItemView = value; }
        }

        public bool IsAnchor => (m_ItemShapeGrid?.ItemShapeGridData?.GetElementAt(m_Index).IsAnchor ?? false);

        /// <summary>
        /// Set the item info.
        /// </summary>
        /// <param name="info">The item info.</param>
        public override void SetValue(ItemInfo info)
        {

            if (info.Item == null || info.Item.IsInitialized == false) {
                Clear();
                return;
            }

            SetAsItemSelectable(true);
            SetIcon(info);
        }

        /// <summary>
        /// Set the icon for the item info.
        /// </summary>
        /// <param name="info">The item info.</param>
        public void SetIcon(ItemInfo info)
        {
            if (info.Item == null) {
                ClearIcon();
                return;
            }

            m_Icon.enabled = true;
            Sprite iconToSet = null;
            if (info.Item.TryGetAttributeValue<Sprite>(m_ShapeIconAttributeName, out var shapeIcon)) {
                iconToSet = shapeIcon;
            }

            if (m_UseBackupIcon && iconToSet == null) {
                if (info.Item.TryGetAttributeValue<Sprite>("Icon", out var icon)) {
                    iconToSet = icon;
                }
            }

            if (iconToSet == null) {
                iconToSet = m_MissingIcon;
            }

            m_Icon.sprite = iconToSet;

            ResizeToItemShape(info);
        }

        /// <summary>
        /// Clear the icon.
        /// </summary>
        public void ClearIcon()
        {
            m_Icon.sprite = m_MissingIcon;
            if (m_DisableOnClear) { m_Icon.enabled = false; }
            ResizeToItemShape(ItemInfo.None);
        }

        /// <summary>
        /// Set the color of the icon to show if it is selectable.
        /// </summary>
        /// <param name="isSelectable"></param>
        public void SetAsItemSelectable(bool isSelectable)
        {
            m_IsItemSelectable = isSelectable;
            m_Icon.color = isSelectable ? m_SelectableColor : m_UnselectableColor;
        }

        /// <summary>
        /// Resize the view to the Item Shape size.
        /// </summary>
        /// <param name="info">The item info.</param>
        protected virtual void ResizeToItemShape(ItemInfo info)
        {
            //Debug.Log("About to resize item "+info);
            m_CellResizableContent.sizeDelta = CellSize;

            if (info.Item == null || info.Item.TryGetAttributeValue<ItemShape>(m_ShapeAttributeName, out var itemShape) == false) {
                m_ShapeResizableContent.anchoredPosition = Vector2.zero;
                m_ShapeResizableContent.sizeDelta = CellSize;
                return;
            }

            //Debug.Log(info+" size: "+itemShape.Size);

            m_ShapeResizableContent.anchoredPosition = new Vector2(
                -itemShape.Anchor.x * CellSize.x,
                itemShape.Anchor.y * CellSize.y);

            m_ShapeResizableContent.sizeDelta = new Vector2(CellSize.x * itemShape.Cols, CellSize.y * itemShape.Rows);
        }

        /// <summary>
        /// Clear the component.
        /// </summary>
        public override void Clear()
        {
            //Debug.Log("Clear Item shape");
            SetAsItemSelectable(true);
            ClearIcon();
            ChangeLayerState(false);

            m_Index = -1;
            m_ItemShapeGrid = null;
        }

        /// <summary>
        /// Set the grid info.
        /// </summary>
        /// <param name="itemShapeGrid">The item shape grid.</param>
        /// <param name="index">The index within the grid.</param>
        /// <param name="foreground">Is this the foreground or background view?</param>
        public virtual void SetGridInfo(ItemShapeGrid itemShapeGrid, int index, bool foreground)
        {
            m_Index = index;
            m_ItemShapeGrid = itemShapeGrid;

            ChangeLayerState(foreground);

            if (m_ItemShapeGrid.ItemShapeGridData.GetElementAt(index).IsAnchor) {
                ResizeToItemShape(m_View.CurrentValue);
            } else {
                ClearIcon();
            }

            OnGridInfoSet?.Invoke();
        }

        /// <summary>
        /// Change the layer state between foreground and background.
        /// </summary>
        /// <param name="foreground">Is it foreground or background?</param>
        protected virtual void ChangeLayerState(bool foreground)
        {
            m_Foreground = foreground;

            if (m_Foreground) {
                m_ForegroundItemView = null;
            }

            m_View.CanvasGroup.interactable = !foreground;
            m_View.CanvasGroup.blocksRaycasts = !foreground;

            for (int i = 0; i < m_ForegroundActive.Length; i++) {
                m_ForegroundActive[i].SetActive(m_Foreground);
            }

            for (int i = 0; i < m_BackgroundActive.Length; i++) {
                m_BackgroundActive[i].SetActive(m_Foreground == false);
            }
        }

        /// <summary>
        /// Offset the Image by a certain discrete amount (scaled by the cell size).
        /// </summary>
        /// <param name="offset">The offset.</param>
        public virtual void DiscreteOffsetImage(Vector2Int offset)
        {
            //var center= new Vector2(-CellSize.x/2f, CellSize.y/2f);
            var fullOffset = new Vector2(offset.x, -offset.y) * CellSize;
            m_ShapeResizableContent.anchoredPosition += fullOffset;
        }

        public void SetAsMoving()
        {
            ChangeLayerState(true);
        }

        public void SetAsMovingSource(bool movingSource)
        {
            //nothing
        }
    }
}