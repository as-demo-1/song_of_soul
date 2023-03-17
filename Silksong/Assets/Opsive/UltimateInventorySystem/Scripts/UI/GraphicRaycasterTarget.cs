/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI
{
    using UnityEngine.UI;
    using Opsive.UltimateInventorySystem.Input;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    /// <summary>
    /// A component that detects graphic raycasting without drawing anything on screen.
    /// </summary>
    public class GraphicRaycasterTarget : Graphic, IPointerClickHandler
    {
        [Tooltip("Let the user click through the background image?")]
        [SerializeField] protected bool m_ClickPassThrough = false;

        protected List<RaycastResult> m_RaycastResults;

        public bool ClickPassThrough {
            get => m_ClickPassThrough;
            set => m_ClickPassThrough = value;
        }

        // Prevent the graphic bas class from drawing anything.
        public override void SetMaterialDirty() { return; }
        public override void SetVerticesDirty() { return; }

        /// <summary>
        /// Probably not necessary since the chain of calls `Rebuild()`->`UpdateGeometry()`->`DoMeshGeneration()`->`OnPopulateMesh()` won't happen; so here really just as a fail-safe.
        /// </summary>
        /// <param name="vh">The vertex helper.</param>
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            return;
        }

        /// <summary>
        /// The target was clicked.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (!m_ClickPassThrough || raycastTarget == false) { return; }

            PassThroughClick(eventData);
        }

        /// <summary>
        /// Pass through click allows to click through the target.
        /// </summary>
        /// <param name="eventData">The event Data.</param>
        protected virtual void PassThroughClick(PointerEventData eventData)
        {
            if (m_RaycastResults == null) { m_RaycastResults = new List<RaycastResult>(); } else { m_RaycastResults.Clear(); }

            var eventSystem = EventSystemManager.GetEvenSystemFor(gameObject); 
            eventSystem.RaycastAll(eventData, m_RaycastResults);

            if (m_RaycastResults.Count <= 0) { return; }

            var count = 0;
            var result = m_RaycastResults[0];
            var clickableGameObject = GetClickableGameObject(result);

            while ((count < m_RaycastResults.Count - 1) && !IsPassthroughGameObjectValid(clickableGameObject)) {
                count++;
                result = m_RaycastResults[count];
                clickableGameObject = GetClickableGameObject(result);
            }

            if (!IsPassthroughGameObjectValid(clickableGameObject)) { return; }

            ExecuteEvents.Execute(clickableGameObject, eventData, ExecuteEvents.pointerEnterHandler);
            ExecuteEvents.Execute(clickableGameObject, eventData, ExecuteEvents.selectHandler);
            ExecuteEvents.Execute(clickableGameObject, eventData, ExecuteEvents.updateSelectedHandler);
            ExecuteEvents.Execute(clickableGameObject, eventData, ExecuteEvents.pointerClickHandler);

        }

        /// <summary>
        /// Is the passthrough gameobject valid
        /// </summary>
        /// <param name="otherGameObject">The other gameobject.</param>
        /// <returns></returns>
        protected virtual bool IsPassthroughGameObjectValid(GameObject otherGameObject)
        {
            return !(otherGameObject == null || otherGameObject == gameObject || gameObject.transform.IsChildOf(otherGameObject.transform));
        }

        /// <summary>
        /// Get the clickable gameobject.
        /// </summary>
        /// <param name="result">The raycast result.</param>
        /// <returns>The clickable gameobject.</returns>
        protected virtual GameObject GetClickableGameObject(RaycastResult result)
        {
            var selectable = result.gameObject.GetComponentInParent<Selectable>();
            var clickableGameObject = selectable?.gameObject;
            if (clickableGameObject == null) {
                var clickable = result.gameObject.GetComponentInParent<IPointerClickHandler>();
                clickableGameObject = (clickable as MonoBehaviour)?.gameObject;
            }

            return clickableGameObject;
        }
    }
}
