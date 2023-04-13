/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Grid
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Base class for Grid Navigators.
    /// </summary>
    public class GridNavigatorBase : MonoBehaviour
    {
        public event IndexChangeEvent OnIndexChange;

        [Tooltip("The grid.")]
        [SerializeField] internal GridBase m_Grid;
        [Tooltip("The tab control (this component is optional).")]
        [SerializeField] internal TabControl m_TabControl;
        [Tooltip("The previous button.")]
        [SerializeField] internal Button m_PreviousButton;
        [Tooltip("The next button.")]
        [SerializeField] internal Button m_NextButton;

        protected int m_Index;

        protected int GridElementCount => m_Grid.ElementCount;
        protected virtual int Step => 1;
        protected int StartIndex => m_Index * Step;
        protected virtual int MaxSliceIndex => GridElementCount <= m_Grid.GridSizeCount ? 0 : 1 + (GridElementCount - m_Grid.GridSizeCount - 1) / Step;

        protected bool UseTabControl => m_TabControl != null;

        protected bool m_IsInitialized = false;

        /// <summary>
        /// Initialize.
        /// </summary>
        private void Awake()
        {
            Initialize(false);
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="force">Force Initialize.</param>
        public virtual void Initialize(bool force)
        {
            if (m_IsInitialized && force == false) { return; }

            RegisterGridSystemMoves();

            m_Grid.OnElementsChanged += () =>
            {
                if (m_Index > MaxSliceIndex) {
                    var previousIndex = m_Index;
                    m_Index = MaxSliceIndex;
                    m_Grid.SetIndex(StartIndex);
                    OnIndexChange?.Invoke(previousIndex, m_Index);
                }

                RefreshArrows();
                RefreshTabPages();
            };

            if (m_PreviousButton != null) {
                m_PreviousButton.onClick.AddListener(() => PreviousSlice());
            }

            if (m_NextButton != null) {
                m_NextButton.onClick.AddListener(() => NextSlice());
            }

            m_IsInitialized = true;
            m_Grid.SetIndex(StartIndex);
        }

        /// <summary>
        /// Register to the events on the Grid Event System.
        /// </summary>
        protected virtual void RegisterGridSystemMoves()
        {
            m_Grid.GridEventSystem.OnUnavailableNavigationLeft += () => PreviousSlice();
            m_Grid.GridEventSystem.OnUnavailableNavigationRight += () => NextSlice();
        }

        /// <summary>
        /// Go to the next slice.
        /// </summary>
        /// <returns>True if the navigator moved.</returns>
        public virtual bool NextSlice()
        {
            if (IsNextSliceAvailableInternal()) { return NextSliceInternal(); }

            if (!UseTabControl) { return false; }

            var result = m_TabControl.NextTab();

            if (result) {
                SetIndexInternal(0);
            }

            return result;

        }

        /// <summary>
        /// Go to the previous slice.
        /// </summary>
        /// <returns>True if the Navigator moved.</returns>
        public virtual bool PreviousSlice()
        {
            if (IsPreviousSliceAvailableInternal()) { return PreviousSliceInternal(); }

            if (!UseTabControl) { return false; }

            var result = m_TabControl.PreviousTab();

            if (result) {
                SetIndexInternal(MaxSliceIndex);
            }

            return result;

        }

        /// <summary>
        /// Go to the next slice.
        /// </summary>
        /// <returns>True if the navigator moved.</returns>
        protected virtual bool NextSliceInternal()
        {
            SetIndexInternal(m_Index + 1);
            return true;
        }

        /// <summary>
        /// Go to the previous slice.
        /// </summary>
        /// <returns>True if the Navigator moved.</returns>
        protected virtual bool PreviousSliceInternal()
        {
            SetIndexInternal(m_Index - 1);
            return true;
        }

        /// <summary>
        /// Set the index of the navigator.
        /// </summary>
        /// <param name="newIndex">The new index.</param>
        protected void SetIndexInternal(int newIndex)
        {
            var previousIndex = m_Index;
            m_Index = newIndex;

            if (previousIndex == newIndex) {
                return;
            }

            m_Grid.SetIndex(StartIndex);
            OnIndexChange?.Invoke(previousIndex, m_Index);
            m_Grid.Draw();
            RefreshArrows();
            RefreshTabPages();
        }

        /// <summary>
        /// Refresh the tab page indexes.
        /// </summary>
        protected virtual void RefreshTabPages()
        {
            if (UseTabControl) {
                m_TabControl.CurrentTab.SetPageCount(MaxSliceIndex + 1);
                m_TabControl.CurrentTab.SetPageIndex(m_Index);
            }
        }

        /// <summary>
        /// Is the next page available.
        /// </summary>
        /// <returns>True if it is.</returns>
        public virtual bool IsNextSliceAvailable()
        {
            return IsNextSliceAvailableInternal() || (UseTabControl && m_TabControl.IsNextTabAvailable());
        }

        /// <summary>
        /// Is the previous page available.
        /// </summary>
        /// <returns>True if it is.</returns>
        public virtual bool IsPreviousSliceAvailable()
        {
            return IsPreviousSliceAvailableInternal() || (UseTabControl && m_TabControl.IsPreviousTabAvailable());
        }

        /// <summary>
        /// Is the next page available.
        /// </summary>
        /// <returns>True if it is.</returns>
        protected virtual bool IsNextSliceAvailableInternal()
        {
            return m_Index < MaxSliceIndex;
        }

        /// <summary>
        /// Is the previous page available.
        /// </summary>
        /// <returns>True if it is.</returns>
        protected virtual bool IsPreviousSliceAvailableInternal()
        {
            return m_Index > 0;
        }

        /// <summary>
        /// Refresh the arrows, hide if unavailable.
        /// </summary>
        public virtual void RefreshArrows()
        {
            if (m_PreviousButton != null) { m_PreviousButton.gameObject.SetActive(IsPreviousSliceAvailable()); }

            if (m_NextButton != null) { m_NextButton.gameObject.SetActive(IsNextSliceAvailable()); }
        }
    }
}