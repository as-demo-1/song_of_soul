/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Views
{
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.Utility;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Serialization;
    using Shared.Utility;
    

    /// <summary>
    /// The base class for a box drawer.
    /// </summary>
    public abstract class ViewDrawerBase : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("m_UseViewParent")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_UseBoxParent")]
        [Tooltip("If use box parent is true the Item Viewes will each be spawned under the next available box parent under this game object. If false they will spawn under this game object.")]
        [SerializeField] protected bool m_UseViewSlot = true;
        [UnityEngine.Serialization.FormerlySerializedAs("m_DisableViewParentImageComponent")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_DisableBoxParentImageComponent")]
        [Tooltip("Disables the parent box image component so that it may be used as to preview the grid in the editor, but not affect the boxes in game.")]
        [SerializeField] protected bool m_DisableViewSlotImageComponent = true;
        [Tooltip("Content Transform where the Item Viewes or there parents reside.")]
        [SerializeField] protected Transform m_Content;
        [UnityEngine.Serialization.FormerlySerializedAs("m_RemoveItemViewsOnInitialize")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_RemoveItemBoxesOnInitialize")]
        [Tooltip("Initialize this component on start?.")]
        [SerializeField] protected bool m_RemoveViewsOnInitialize = true;
        [UnityEngine.Serialization.FormerlySerializedAs("m_DrawEmptyItemViewsOnInitialize")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_DrawEmptyBoxesOnInitialize")]
        [Tooltip("Draw Empty boxes when initialized?.")]
        [SerializeField] protected bool m_DrawEmptyViewsOnInitialize = true;

        protected IViewSlot[] m_ViewSlots;

        public IViewSlot[] ViewSlots
        {
            get { return m_ViewSlots; }
            set { m_ViewSlots = value; }
        }

        protected bool m_IsInitialized = false;

        public Transform Content {
            get => m_Content;
            set => m_Content = value;
        }

        /// <summary>
        /// Initialize on start.
        /// </summary>
        private void Start()
        {
            Initialize(false);
        }

        /// <summary>
        /// Initialize the component.
        /// </summary>
        public virtual void Initialize(bool force)
        {
            if (m_IsInitialized && !force) { return; }

            if (m_Content == null) { m_Content = transform; }

            if (m_ViewSlots == null) {
                m_ViewSlots = m_Content.GetComponentsInChildren<IViewSlot>();
            }
            
            for (int i = 0; i < m_ViewSlots.Length; i++) {
                var viewSlot = m_ViewSlots[i];
                if (m_DisableViewSlotImageComponent) {
                    viewSlot.DisableImage();
                }
                if (m_RemoveViewsOnInitialize) {
                    RemoveChildrenFromTransform(viewSlot.transform);
                }
            }

            if (m_DrawEmptyViewsOnInitialize) {
                for (int i = 0; i < m_ViewSlots.Length; i++) {
                    DrawEmptyView(i, i);
                }
            }

            m_IsInitialized = true;
        }

        /// <summary>
        /// Remove boxes.
        /// </summary>
        protected virtual void RemoveBoxes()
        {
            if (m_UseViewSlot) {
                RemoveBoxesFromBoxParents();
            } else {
                RemoveChildrenFromTransform(m_Content);
            }
        }

        /// <summary>
        /// Remove boxes from parents.
        /// </summary>
        protected void RemoveBoxesFromBoxParents()
        {
            for (int i = 0; i < m_ViewSlots.Length; i++) {
                if (m_ViewSlots[i].View == null) { continue; }

                var obj = m_ViewSlots[i].View.gameObject;
                if (Application.isPlaying) {
                    if (ObjectPool.IsPooledObject(obj)) { ObjectPool.Destroy(obj); } else { Destroy(obj); }
                } else {
                    DestroyImmediate(obj);
                }

            }
        }

        /// <summary>
        /// Remove children from a transform.
        /// </summary>
        /// <param name="trans">The parent transform.</param>
        protected void RemoveChildrenFromTransform(Transform trans)
        {
            for (int i = trans.childCount - 1; i >= 0; i--) {
                var childObject = trans.GetChild(i).gameObject;
                if (Application.isPlaying) {
                    if (ObjectPool.IsPooledObject(childObject)) { ObjectPool.Destroy(childObject); } else { Destroy(childObject); }
                } else {
                    DestroyImmediate(childObject);
                }
            }
        }

        /// <summary>
        /// Remove the view.
        /// </summary>
        /// <param name="index">The index of the view to remove.</param>
        public void RemoveView(int index)
        {
            var objectToRemove = GetViewGameObject(index);

            if (objectToRemove == null) { return; }

            if (Application.isPlaying) {
                if (ObjectPool.IsPooledObject(objectToRemove)) { ObjectPool.Destroy(objectToRemove); } else { Destroy(objectToRemove); }
            } else {
                DestroyImmediate(objectToRemove);
            }
        }

        /// <summary>
        /// Get the View gameobject of the index.
        /// </summary>
        /// <param name="index">The index of the view.</param>
        /// <returns>The view gameobject.</returns>
        protected virtual GameObject GetViewGameObject(int index)
        {
            GameObject objectToRemove = null;
            if (m_UseViewSlot) {
                if (m_ViewSlots[index] != null && m_ViewSlots[index].View != null) {
                    objectToRemove = m_ViewSlots[index].View.gameObject;
                }
            } else {
                objectToRemove = m_Content.GetChild(index)?.gameObject;
            }

            return objectToRemove;
        }

        /// <summary>
        /// Draw an empty view.
        /// </summary>
        /// <param name="viewIndex">The view index.</param>
        /// <param name="elementIndex">The element Index.</param>
        /// <param name="removePreviousView">Remove from the previous view?</param>
        /// <returns>The view.</returns>
        protected abstract View DrawEmptyView(int viewIndex, int elementIndex);
    }

    /// <summary>
    /// The generic base class for Item Viewes.
    /// </summary>
    /// <typeparam name="T">The box object type.</typeparam>
    public abstract class ViewDrawer<T> : ViewDrawerBase
    {
        public event Action<View<T>, T> BeforeDrawing;
        public event Action<View<T>, T> AfterDrawing;

        protected List<View<T>> m_Views;

        public IReadOnlyList<View<T>> Views => m_Views;

        /// <summary>
        /// Initialize the component.
        /// </summary>
        public override void Initialize(bool force)
        {
            if (m_IsInitialized && !force) { return; }

            if (m_Views == null) {
                m_Views = new List<View<T>>();
            }
            base.Initialize(force);
        }

        /// <summary>
        /// Get a prefab for the Object type.
        /// </summary>
        /// <param name="item">The object.</param>
        /// <returns>The prefab.</returns>
        public abstract GameObject GetViewPrefabFor(T item);

        /// <summary>
        /// Draw the boxes using a set of the elements.
        /// </summary>
        /// <param name="startIndex">The start index of the list.</param>
        /// <param name="endIndex">The end index of the list.</param>
        /// <param name="elements">The list of elements.</param>
        public virtual void DrawViews(int startIndex, int endIndex, IReadOnlyList<T> elements)
        {
            if (startIndex < 0) {
                Debug.LogWarning($"The start index is {startIndex}, it should not be negative.");
                startIndex = 0;
            }

            if (startIndex > endIndex) { return; }

            var elementsEnd = Mathf.Min(endIndex, elements.Count);

            for (int i = startIndex; i < endIndex; i++) {

                if (i < elementsEnd) {
                    DrawView(i - startIndex, i, elements[i]);
                } else {
                    DrawView(i - startIndex, i, default);
                }
            }
        }

        /// <summary>
        /// Draw the box for an element.
        /// </summary>
        /// <param name="viewIndex">The box index.</param>
        /// <param name="elementIndex">The element index.</param>
        /// <param name="element">The element.</param>
        /// <returns>The box.</returns>
        public virtual View<T> DrawView(int viewIndex, int elementIndex, T element)
        {
            View<T> view = null;

            if (viewIndex >= 0 && viewIndex < m_Views.Count && m_Views[viewIndex] != null) {
                // A view already exists.
                var currentView = m_Views[viewIndex];
                var viewPrefab = GetViewPrefabFor(element);
                if (viewPrefab == ObjectPool.GetOriginalObject(currentView.gameObject)) {
                    view = currentView;
                }
            }

            if (view == null) {
                RemoveView(viewIndex);
                view = InstantiateView(viewIndex, elementIndex, element);

                if (view == null) {
                    Debug.LogWarning("The Box Drawer BoxUI Prefab does not have a BoxUI component or it is not of the correct Type");
                    return null;
                }
            }

            //Clear to remove any previous state of the pooled object.
            view.Clear();

            BeforeDrawingBox(elementIndex, view, element);

            DrawingView(elementIndex, view, element);

            AfterDrawingBox(elementIndex, view, element);

            m_Views.EnsureSize(viewIndex + 1);

            m_Views[viewIndex] = view;

            return view;
        }

        /// <summary>
        /// Draw Empty view.
        /// </summary>
        /// <param name="viewIndex">The view index.</param>
        /// <param name="elementIndex">The element index.</param>
        /// <param name="removePreviousView">Should the previous view be removed?</param>
        /// <returns>The view that was drawn.</returns>
        protected override View DrawEmptyView(int viewIndex, int elementIndex)
        {
            return DrawView(viewIndex, elementIndex, default);
        }

        /// <summary>
        /// Instantiate a box.
        /// </summary>
        /// <param name="boxIndex">The box index.</param>
        /// <param name="elementIndex">The element index.</param>
        /// <param name="element">The element.</param>
        /// <returns>The instantiated box.</returns>
        protected virtual View<T> InstantiateView(int boxIndex, int elementIndex, T element)
        {
            var itemViewPrefab = GetViewPrefabFor(element);
            if (itemViewPrefab == null) {
                Debug.LogWarning("View Prefab is null.");
                return null;
            }

            return m_UseViewSlot
                ? InstantiateViewWithSlot(itemViewPrefab, boxIndex, elementIndex, element)
                : InstantiateViewInThis(itemViewPrefab, boxIndex, elementIndex, element);
        }

        /// <summary>
        /// Instantiate a box under a parent.
        /// </summary>
        /// <param name="prefab">The box prefab.</param>
        /// <param name="boxIndex">The box index.</param>
        /// <param name="elementIndex">The element index.</param>
        /// <param name="element">The element.</param>
        /// <returns>The box ui.</returns>
        protected virtual View<T> InstantiateViewWithSlot(GameObject prefab, int boxIndex, int elementIndex, T element)
        {
            if (boxIndex >= m_ViewSlots.Length || m_ViewSlots[boxIndex] == null) {
                Debug.LogWarning($"There is no view parent for the index {boxIndex}. Please make sure there are enough box parents when using them.");
                return InstantiateViewInThis(prefab, boxIndex, elementIndex, element);
            }

            var view = ObjectPool.Instantiate(prefab, m_ViewSlots[boxIndex].transform).GetComponent<View<T>>();
            view.RectTransform.anchoredPosition = Vector2.zero;
            view.RectTransform.anchorMin = new Vector2(0f, 0f);
            view.RectTransform.anchorMax = new Vector2(1f, 1f);
            view.RectTransform.pivot = new Vector2(0.5f, 0.5f);
            view.RectTransform.sizeDelta = Vector2.zero;
            view.RectTransform.offsetMin = Vector2.zero;
            view.RectTransform.offsetMax = Vector2.zero;
            view.RectTransform.localPosition = Vector3.zero;
            view.RectTransform.localScale = Vector3.one;
            m_ViewSlots[boxIndex].SetView(view);
            return view;

        }

        /// <summary>
        /// Instantiate the view within this transform.
        /// </summary>
        /// <param name="prefab">The view prefab.</param>
        /// <param name="boxIndex">The view index.</param>
        /// <param name="elementIndex">The element index.</param>
        /// <param name="element">The element.</param>
        /// <returns>The box ui.</returns>
        protected virtual View<T> InstantiateViewInThis(GameObject prefab, int boxIndex, int elementIndex, T element)
        {
            return ObjectPool.Instantiate(prefab, m_Content).GetComponent<View<T>>();
        }

        /// <summary>
        /// Send an event before drawing box.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="view">The box.</param>
        /// <param name="element">The element.</param>
        protected virtual void BeforeDrawingBox(int index, View<T> view, T element)
        {
            BeforeDrawing?.Invoke(view, element);
        }

        /// <summary>
        /// Draw the box.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="view">The box.</param>
        /// <param name="element">The element.</param>
        protected virtual void DrawingView(int index, View<T> view, T element)
        {
            view.SetValue(element);
        }

        /// <summary>
        /// Send an event after drawing the box.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="view">The box.</param>
        /// <param name="element">The element.</param>
        protected virtual void AfterDrawingBox(int index, View<T> view, T element)
        {
            AfterDrawing?.Invoke(view, element);
        }

        /// <summary>
        /// Is the index available.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>True if it is available.</returns>
        public bool IsIndexAvailable(int index)
        {
            if (index >= m_Views.Count) { return false; }

            if (m_Views[index] == null) { return false; }

            if (m_Views[index].isActiveAndEnabled == false) { return false; }

            if (m_UseViewSlot && m_ViewSlots[index].isActiveAndEnabled == false) { return false; }

            return true;
        }

        /// <summary>
        /// Select the view at index.
        /// </summary>
        /// <param name="index">The index.</param>
        public void Select(int index)
        {
            for (int i = 0; i < m_Views.Count; i++) {
                if (m_Views[i] == null) { continue; }

                if (index == i) {
                    m_Views[index].Select(true);
                    continue;
                }

                if (m_Views[i].IsSelected) {
                    m_Views[i].Select(false);
                }
            }
        }

        /// <summary>
        /// Click a view at index.
        /// </summary>
        /// <param name="index">The index.</param>
        public void Click(int index)
        {
            if (index < 0 || index >= m_Views.Count) { return; }
            m_Views[index].Click();
        }
    }
}