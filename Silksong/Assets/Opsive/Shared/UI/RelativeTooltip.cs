namespace Opsive.Shared.UI
{
    using System;
    using UnityEngine;

    [Serializable]
    public class RelativeTooltip
    {
        [Tooltip("The transform to place next to the Item View selected/clicked.")]
        [SerializeField] protected RectTransform m_PanelToPlace;
        [Tooltip("Set the anchor position of the panel each time it is placed.")]
        [SerializeField] protected bool m_SetAnchorPosition = true;
        [Tooltip("The anchor position of the panel to place (Only used if the SetAnchorPosition is true).")]
        [SerializeField] protected Vector2 m_AnchorPosition = new Vector2(0, 0.5f);
        [Tooltip("The offset compared to the Item View anchor, (0|0) is the view center. (0.5|0.5) is top right.")]
        [SerializeField] protected Vector2 m_AnchorRelativeOffset = new Vector2(0.5f, 0);
        [Tooltip("A fixed offset added at to the relative offset, scales with the canvas scaler.")]
        [SerializeField] protected Vector2 m_PixelFixedOffset = new Vector2(0, 0);
        [Tooltip("move the panel so that it fits inside the panel bounds (keep null if the panel is unbound).")]
        [SerializeField] protected RectTransform m_PanelBounds;

        protected Vector3[] m_BoundCorner;
        protected Canvas m_Canvas;
        protected bool m_TooltipInitialize;

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="force">Force initialize.</param>
        public void Initialize(bool force)
        {
            if (m_TooltipInitialize && !force) { return; }

            if (m_PanelBounds != null) {
                m_BoundCorner = new Vector3[4];
                //Get the world corners bottom-left -> top-left -> top-right -> bottom-right.
                m_PanelBounds.GetWorldCorners(m_BoundCorner);
            }

            if (m_Canvas == null && m_PanelToPlace != null) {
                m_Canvas = m_PanelToPlace.GetComponentInParent<Canvas>();
            }

            m_TooltipInitialize = true;
        }

        /// <summary>
        /// Handle the item deselected event.
        /// </summary>
        /// <param name="slotEventData">The slot event data.</param>
        public virtual void Hide()
        {
            m_PanelToPlace.gameObject.SetActive(false);
        }

        /// <summary>
        /// An item was clicked.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="boxIndex">The box index.</param>
        public virtual void Show()
        {
            m_PanelToPlace.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// An item was clicked.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="boxIndex">The box index.</param>
        public virtual void ShowAndPlace(RectTransform anchorRectTransform)
        {
            Show();
            PlacePanel(anchorRectTransform);
        }

        /// <summary>
        /// Place the panel next to an Item View.
        /// </summary>
        /// <param name="anchorRectTransform">The rect transform.</param>
        public void PlacePanel(RectTransform anchorRectTransform)
        {
            var newAnchor = m_SetAnchorPosition ? m_AnchorPosition : m_PanelToPlace.pivot;
            var newPosition = anchorRectTransform.position;

            var positionOffset = new Vector2(
                anchorRectTransform.sizeDelta.x * m_AnchorRelativeOffset.x,
                anchorRectTransform.sizeDelta.y * m_AnchorRelativeOffset.y);

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