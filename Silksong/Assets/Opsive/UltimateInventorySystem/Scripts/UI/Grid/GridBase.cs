/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Grid
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Input;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.UI.CompoundElements;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using Opsive.UltimateInventorySystem.UI.Views;
    using Opsive.UltimateInventorySystem.Utility;
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.Serialization;
    using UnityEngine.UI;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// The bas class for a grid.
    /// </summary>
    public abstract class GridBase : MonoBehaviour
    {
        public event Action OnElementsChanged;
        public event Action OnBeforeDraw;
        public event Action OnAfterDraw;

        [Tooltip("The Grid ID is used only when multiple grids should share some data (i.e item indexes), -1 means it is unique.")]
        [SerializeField] protected int m_GridID = -1;
        [Tooltip("The canvas where this grid exist.")]
        [SerializeField] protected Canvas m_Canvas;
        [Tooltip("The grid size.")]
        [SerializeField] protected Vector2Int m_GridSize = new Vector2Int(5, 4);
        [FormerlySerializedAs("m_BoxDrawerBase")]
        [Tooltip("The box drawer.")]
        [SerializeField] internal ViewDrawerBase m_ViewDrawerBase;
        [Tooltip("The grid event system.")]
        [SerializeField] internal GridEventSystem m_GridEventSystem;
        [Tooltip("The grid index slicer (optional).")]
        [SerializeField] internal GridNavigatorBase m_GridNavigator;
        [Tooltip("The grid tab control (optional).")]
        [SerializeField] internal TabControl m_TabControl;
        [Tooltip("Refresh the grid on enable.")]
        [SerializeField] protected bool m_RefreshOnEnable;
        [Tooltip("Input name for Next tab.")]
        [SerializeField] protected string m_NextTabInput = "Next";
        [Tooltip("Input name for Previous tab.")]
        [SerializeField] protected string m_PreviousTabInput = "Previous";

        protected DisplayPanel m_ParentPanel;
        protected PlayerInput m_PlayerInput;
        protected bool m_IsInitialized;

        protected int m_StartIndex;
        protected int m_EndIndex;

        public int StartIndex => m_StartIndex;
        public int EndIndex => m_EndIndex;
        public abstract int ElementCount { get; }

        public Vector2Int GridSize {
            get { return m_GridSize; }
            internal set { m_GridSize = value; }
        }

        public int GridSizeCount => m_GridSize.x * m_GridSize.y;
        public GridEventSystem GridEventSystem => m_GridEventSystem;
        public DisplayPanel ParentPanel => m_ParentPanel;
        public ViewDrawerBase ViewDrawerBase => m_ViewDrawerBase;
        public TabControl TabControl => m_TabControl;

        public bool IsInitialized => m_IsInitialized;
        public int GridID => m_GridID;
        public GridLayoutGroup GridLayoutGroup => m_GridEventSystem.GridLayoutGroup;

        /// <summary>
        /// Set the grid buttons when changing the grid size.
        /// </summary>
        internal virtual void OnValidate()
        {
            if (OnValidateUtility.IsPrefab(this)) { return; }

            if (m_GridEventSystem == null) { return; }

            var gridLayout = m_GridEventSystem.GridLayoutGroup;
            if (gridLayout != null) {
                if (gridLayout.constraintCount != m_GridSize.x) { gridLayout.constraintCount = m_GridSize.x; }
            }

            if (m_GridEventSystem.transform.childCount != m_GridSize.x * m_GridSize.y) {
                if (m_GridEventSystem.ButtonPrefab != null) {
                    m_GridEventSystem.SpawnButtons(m_GridSize);
                }
            }

            if (m_Canvas == null) { m_Canvas = GetComponentInParent<Canvas>(); }
        }

        /// <summary>
        /// Initialize the grid.
        /// </summary>
        public virtual void Initialize(bool force)
        {
            if (m_IsInitialized && !force) { return; }

            m_EndIndex = GridSizeCount;

            if (m_Canvas == null) { m_Canvas = GetComponentInParent<Canvas>(); }

            m_GridEventSystem.Initialize(m_GridSize);
            m_GridEventSystem.SelectButton(0);

            m_GridEventSystem.OnGridElementClickE += ViewClicked;
            m_GridEventSystem.OnGridElementSelectedE += ViewSelected;
            m_GridEventSystem.OnGridElementPointerDownE += ViewOnPointerDown;
            m_GridEventSystem.OnGridElementBeginDragE += ViewBeginDrag;
            m_GridEventSystem.OnGridElementEndDragE += ViewEndDrag;
            m_GridEventSystem.OnGridElementDragE += ViewDrag;
            m_GridEventSystem.OnGridElementDropE += ViewDrop;

            if (m_TabControl == null) { m_TabControl = GetComponent<TabControl>(); }

            if (m_TabControl != null) {
                m_TabControl.Initialize(force);
            }

            if (m_GridNavigator == null) { m_GridNavigator = GetComponent<GridNavigatorBase>(); }

            if (m_GridNavigator != null) {
                m_GridNavigator.Initialize(force);
            }

            var bindings = GetComponents<GridBinding>();
            for (int i = 0; i < bindings.Length; i++) {
                bindings[i].Bind(this);
            }

            m_IsInitialized = true;
        }

        /// <summary>
        /// Refreshes the grid.
        /// </summary>
        protected virtual void OnEnable()
        {
            Initialize(false);

            if (m_RefreshOnEnable) { Refresh(); }
        }

        /// <summary>
        /// Redraw the views and update the tab control and grid navigator.
        /// </summary>
        public virtual void Refresh()
        {
            if (m_TabControl != null) {
                m_TabControl.SetTabOn(m_TabControl.TabIndex);
            } else {
                Draw();
            }

            if (m_GridNavigator != null) {
                m_GridNavigator.RefreshArrows();
            }
        }

        /// <summary>
        /// Set the parent panel.
        /// </summary>
        /// <param name="parentPanel">The parent panel.</param>
        public void SetParentPanel(DisplayPanel parentPanel)
        {
            if (m_ParentPanel == parentPanel) { return; }
            Initialize(false);

            m_PlayerInput = null;
            m_ParentPanel = parentPanel;

            if (m_ParentPanel != null) {
                m_PlayerInput = m_ParentPanel.Manager.PanelOwner?.gameObject?.GetCachedComponent<PlayerInput>();
            }
        }

        /// <summary>
        /// Check for the input.
        /// </summary>
        protected virtual void Update()
        {
            if(m_PlayerInput == null){ return; }
            
            if (m_PlayerInput.GetButtonDown(m_NextTabInput)) {
                NextTab();
            }

            if (m_PlayerInput.GetButtonDown(m_PreviousTabInput)) {
                PreviousTab();
            }
        }

        /// <summary>
        /// Select the next tab.
        /// </summary>
        private void NextTab()
        {
            if (isActiveAndEnabled == false) { return; }

            if (m_ParentPanel.Manager.SelectedDisplayPanel != m_ParentPanel) { return; }

            m_TabControl?.NextTab();
        }

        /// <summary>
        /// Select the previous tab.
        /// </summary>
        private void PreviousTab()
        {
            if (isActiveAndEnabled == false) { return; }
            if (m_ParentPanel.Manager.SelectedDisplayPanel != m_ParentPanel) { return; }

            m_TabControl?.PreviousTab();
        }

        /// <summary>
        /// Set the index.
        /// </summary>
        /// <param name="start">The start index.</param>
        public virtual void SetIndex(int start)
        {
            m_StartIndex = start;
            m_EndIndex = start + GridSizeCount;
        }

        /// <summary>
        /// Set the index.
        /// </summary>
        /// <param name="start">The start index.</param>
        /// <param name="end">The end index.</param>
        public virtual void SetIndex(int start, int end)
        {
            m_StartIndex = start;
            m_EndIndex = end;
        }

        /// <summary>
        /// Select a box.
        /// </summary>
        /// <param name="index">The index.</param>
        protected abstract void ViewSelected(int index);

        /// <summary>
        /// Click a box.
        /// </summary>
        /// <param name="index">The index.</param>
        protected abstract void ViewClicked(int index);

        protected abstract void ViewOnPointerDown(int index, PointerEventData eventData);

        protected abstract void ViewBeginDrag(int index, PointerEventData eventData);

        protected abstract void ViewEndDrag(int index, PointerEventData eventData);

        protected abstract void ViewDrag(int index, PointerEventData eventData);

        protected abstract void ViewDrop(int index, PointerEventData eventData);

        /// <summary>
        /// Select a button.
        /// </summary>
        /// <param name="index">The index.</param>
        public virtual void SelectButton(int index)
        {
            m_GridEventSystem.SelectButton(index);
        }

        /// <summary>
        /// Get the selected button.
        /// </summary>
        /// <returns>The selected button.</returns>
        public virtual ActionButton GetSelectedButton()
        {
            return m_GridEventSystem.GetSelectedButton();
        }

        /// <summary>
        /// Get the button.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The button at the index.</returns>
        public virtual ActionButton GetButton(int index)
        {
            return m_GridEventSystem.GetButton(index);
        }

        /// <summary>
        /// Draw the grid.
        /// </summary>
        protected abstract void DrawInternal();

        /// <summary>
        /// Draw the grid, send events before and after. 
        /// </summary>
        public virtual void Draw()
        {
            if (!m_IsInitialized) { return; }

            BeforeDrawing();
            DrawInternal();
            AfterDrawing();
        }

        /// <summary>
        /// A method called before the grid is drawn.
        /// </summary>
        protected virtual void BeforeDrawing()
        {
            OnBeforeDraw?.Invoke();
        }

        /// <summary>
        /// A method called before the grid is drawn.
        /// </summary>
        protected virtual void AfterDrawing()
        {
            OnAfterDraw?.Invoke();
        }

        /// <summary>
        /// Send event that the elements were changed.
        /// </summary>
        protected virtual void ElementsUpdated()
        {
            OnElementsChanged?.Invoke();
        }

        /// <summary>
        /// Stop listening to the events.
        /// </summary>
        protected void OnDestroy()
        {
            if (m_ParentPanel?.Manager?.PanelOwner == null) {
                return;
            }

            m_PlayerInput = null;
        }
    }
}