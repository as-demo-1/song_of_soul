/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers
{
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.UI.Grid;
    using Opsive.UltimateInventorySystem.UI.Item;
    using Opsive.UltimateInventorySystem.UI.Item.ItemViewModules;
    using UnityEngine;
    using UnityEngine.Serialization;

    /// <summary>
    /// Set the panel position depending on the selected item view slot. 
    /// </summary>
    public class ItemViewSlotPanelToTooltip : DisplayPanelBinding, IItemViewSlotContainerBinding
    {
        [Tooltip("The transform to place next to the Item View selected/clicked.")]
        [SerializeField] protected RectTransform m_PanelToPlace;
        [Tooltip("The inventory grid to monitor.")]
        [SerializeField] internal ItemViewSlotsContainerBase m_ItemViewSlotContainer;
        [Tooltip("Set the anchor position of the panel each time it is placed.")]
        [SerializeField] protected bool m_SetAnchorPosition = true;
        [Tooltip("The anchor position of the panel to place (Only used if the SetAnchorPosition is true).")]
        [SerializeField] protected Vector2 m_AnchorPosition = new Vector2(0, 0.5f);
        [FormerlySerializedAs("m_AnchorOffset")]
        [Tooltip("The offset compared to the Item View anchor, (0|0) is the view center. (0.5|0.5) is top right.")]
        [SerializeField] protected Vector2 m_AnchorRelativeOffset = new Vector2(0.5f, 0);
        [Tooltip("A fixed offset added at to the relative offset, scales with the canvas scaler.")]
        [SerializeField] protected Vector2 m_PixelFixedOffset = new Vector2(0, 0);
        [Tooltip("move the panel so that it fits inside the panel bounds (keep null if the panel is unbound).")]
        [SerializeField] protected RectTransform m_PanelBounds;
        [Tooltip("Place the panel next to the box when clicked.")]
        [SerializeField] internal bool m_PlaceOnClick;
        [Tooltip("Activate/Deactivate the panel when clicked.")]
        [SerializeField] internal bool m_ShowOnClick;
        [Tooltip("Place the panel next to the box when selected.")]
        [SerializeField] internal bool m_PlaceOnSelect = true;
        [Tooltip("Activate/Deactivate the panel when selected.")]
        [SerializeField] internal bool m_ShowOnSelect = true;
        [Tooltip("Hide on deselect.")]
        [SerializeField] internal bool m_HideShowOnDeselect = true;

        protected Vector3[] m_BoundCorner;
        protected Canvas m_Canvas;
        protected bool m_TooltipInitialize;
        protected bool m_IsItemViewSlotContainerBound;

        public bool IsItemViewSlotContainerBound => m_IsItemViewSlotContainerBound;
        public ItemViewSlotsContainerBase ItemViewSlotsContainer => m_ItemViewSlotContainer;

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="display">The display panel.</param>
        /// <param name="force">Force Initialize.</param>
        public override void Initialize(DisplayPanel display, bool force)
        {
            var wasInitialized = m_IsInitialized;
            if (wasInitialized && !force) { return; }
            base.Initialize(display, force);

            Initialize(force);
        }

        /// <summary>
        /// Listen to the grid events.
        /// </summary>
        private void Awake()
        {
            Initialize(false);
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="force">Force initialize.</param>
        private void Initialize(bool force)
        {
            if (m_TooltipInitialize && !force) { return; }

            if (m_PanelToPlace == null) {
                m_PanelToPlace = transform as RectTransform;
            }

            if (m_PanelBounds != null) {
                m_BoundCorner = new Vector3[4];
                //Get the world corners bottom-left -> top-left -> top-right -> bottom-right.
                m_PanelBounds.GetWorldCorners(m_BoundCorner);
            }

            if (m_Canvas == null) {
                m_Canvas = GetComponentInParent<Canvas>();
                // The canvas can still be null if the component is disabled.
                if (m_Canvas == null) {
                    Transform ancestorSearch = transform;
                    while (m_Canvas == null && ancestorSearch.parent != null)
                    {
                        m_Canvas = ancestorSearch.parent.GetComponent<Canvas>();
                        if (m_Canvas != null) break;
                        ancestorSearch = ancestorSearch.parent;
                    }
                }
            }

            if (m_ItemViewSlotContainer == null) {
                Debug.LogError("An Item View Slot Container is missing on the panel placer.", gameObject);
            }

            BindItemViewSlotContainer();

            m_TooltipInitialize = true;
        }

        /// <summary>
        /// Bind the item view slot container.
        /// </summary>
        public virtual void BindItemViewSlotContainer()
        {
            BindItemViewSlotContainer(m_ItemViewSlotContainer);
        }

        /// <summary>
        /// Bind the item view slot container.
        /// </summary>
        /// <param name="itemViewSlotsContainer">The item view slot container to bind.</param>
        public virtual void BindItemViewSlotContainer(ItemViewSlotsContainerBase itemViewSlotsContainer)
        {
            UnbindItemViewSlotContainer();

            m_ItemViewSlotContainer = itemViewSlotsContainer;

            if (m_ItemViewSlotContainer == null) {
                m_IsItemViewSlotContainerBound = false;
                return;
            }

            m_ItemViewSlotContainer.OnItemViewSlotClicked += OnItemClicked;
            m_ItemViewSlotContainer.OnItemViewSlotDeselected += OnItemDeselected;
            m_ItemViewSlotContainer.OnItemViewSlotSelected += OnItemSelected;

            m_IsItemViewSlotContainerBound = true;
        }

        /// <summary>
        /// Unbind the item view slot container.
        /// </summary>
        public virtual void UnbindItemViewSlotContainer()
        {
            if (!m_IsItemViewSlotContainerBound) { return; }

            m_ItemViewSlotContainer.OnItemViewSlotClicked -= OnItemClicked;
            m_ItemViewSlotContainer.OnItemViewSlotDeselected -= OnItemDeselected;
            m_ItemViewSlotContainer.OnItemViewSlotSelected -= OnItemSelected;

            m_IsItemViewSlotContainerBound = false;
        }

        /// <summary>
        /// Handle the item deselected event.
        /// </summary>
        /// <param name="slotEventData">The slot event data.</param>
        private void OnItemDeselected(ItemViewSlotEventData slotEventData)
        {
            if (m_HideShowOnDeselect) { m_PanelToPlace.gameObject.SetActive(false); }
        }

        /// <summary>
        /// Get the rect transform that will be used as a relative anchor for that Item View Slot.
        /// </summary>
        /// <param name="slotEventData">The item view slot data.</param>
        /// <returns>The relative Item View Slot rect transform.</returns>
        protected virtual RectTransform GetItemViewSlotRelativeRectTransform(ItemViewSlotEventData slotEventData)
        {
            if (!(slotEventData.ItemViewSlotsContainer is ItemShapeGrid)) {
                return slotEventData.ItemViewSlot.transform as RectTransform;
            }
            
            ItemShapeItemView backgroundItemShapeView = slotEventData.ItemView.gameObject.GetCachedComponent<ItemShapeItemView>();
            return backgroundItemShapeView.ForegroundItemView.ShapeResizableContent;
        }

        /// <summary>
        /// An item was clicked.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="boxIndex">The box index.</param>
        private void OnItemClicked(ItemViewSlotEventData slotEventData)
        {
            if (m_ShowOnClick == false) { return; }

            var itemInfo = slotEventData.ItemViewSlot.ItemInfo;
            var show = m_ShowOnClick && itemInfo.Item != null;

            m_PanelToPlace.gameObject.SetActive(show);

            if (show == false) { return; }

            if (m_PlaceOnClick) {
                var rectTransform = GetItemViewSlotRelativeRectTransform(slotEventData);
                PlacePanel(rectTransform);
            }
        }

        /// <summary>
        /// The item was selected.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="boxIndex">The box index.</param>
        private void OnItemSelected(ItemViewSlotEventData slotEventData)
        {
            if (m_ShowOnSelect == false) { return; }

            var itemInfo = slotEventData.ItemViewSlot.ItemInfo;
            var show = m_ShowOnSelect && itemInfo.Item != null;

            m_PanelToPlace.gameObject.SetActive(show);

            if (show == false) { return; }

            if (m_PlaceOnSelect) {
                var rectTransform = GetItemViewSlotRelativeRectTransform(slotEventData);
                PlacePanel(rectTransform);
            }
        }

        /// <summary>
        /// Place the panel next to an Item View.
        /// </summary>
        /// <param name="rectTransform">The rect transform.</param>
        private void PlacePanel(RectTransform rectTransform)
        {
            var newAnchor = m_SetAnchorPosition ? m_AnchorPosition : m_PanelToPlace.pivot;
            var newPosition = rectTransform.position;

            var positionOffset = new Vector2(
                rectTransform.sizeDelta.x * m_AnchorRelativeOffset.x,
                rectTransform.sizeDelta.y * m_AnchorRelativeOffset.y);

            positionOffset = (positionOffset + m_PixelFixedOffset) * m_Canvas.scaleFactor;

            if (m_BoundCorner != null && m_BoundCorner.Length == 4) {

                var posRight = newPosition.x + positionOffset.x + m_PanelToPlace.sizeDelta.x * (1 - newAnchor.x);
                var posLeft = newPosition.x + positionOffset.x - m_PanelToPlace.sizeDelta.x * newAnchor.x;
                var posTop = newPosition.y + positionOffset.y + m_PanelToPlace.sizeDelta.y * (1 - newAnchor.y);
                var postBot = newPosition.y + positionOffset.y - m_PanelToPlace.sizeDelta.y * newAnchor.y;

                if (posRight > m_BoundCorner[2].x) {
                    newAnchor = new Vector2(1 - newAnchor.x, newAnchor.y);
                    positionOffset = new Vector2(-positionOffset.x, positionOffset.y);
                } else if (posLeft < m_BoundCorner[0].x) {
                    newAnchor = new Vector2(1 - newAnchor.x, newAnchor.y);
                    positionOffset = new Vector2(-positionOffset.x, positionOffset.y);
                }

                if (posTop > m_BoundCorner[2].y) {
                    newAnchor = new Vector2(newAnchor.x, 1 - newAnchor.y);
                    positionOffset = new Vector2(positionOffset.x, -positionOffset.y);
                } else if (postBot < m_BoundCorner[0].y) {
                    newAnchor = new Vector2(newAnchor.x, 1 - newAnchor.y);
                    positionOffset = new Vector2(positionOffset.x, -positionOffset.y);
                }
            }

            if (m_SetAnchorPosition) {
                m_PanelToPlace.anchorMax = newAnchor;
                m_PanelToPlace.anchorMin = newAnchor;
                m_PanelToPlace.pivot = newAnchor;
            }

            newPosition = new Vector3(
                newPosition.x + positionOffset.x,
                newPosition.y + positionOffset.y,
                newPosition.z);

            m_PanelToPlace.position = newPosition;
        }
    }
}