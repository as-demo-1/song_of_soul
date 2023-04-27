/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI
{
    using Opsive.UltimateInventorySystem.UI.Grid;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// The interface for a tab controller.
    /// </summary>
    public interface ITabControl
    {
        /// <summary>
        /// Select a tab toggle.
        /// </summary>
        /// <param name="tabIndex">The tab toggle to select.</param>
        void SetTabOn(int tabIndex);
    }

    /// <summary>
    /// A tab control.
    /// </summary>
    public class TabControl : MonoBehaviour, ITabControl
    {
        public event IndexChangeEvent OnTabChange;

        [Tooltip("The transform containing the TabToggles.")]
        [SerializeField] internal RectTransform m_Content;
        [Tooltip("The maximum number of pages per tab.")]
        [SerializeField] protected int m_MaxPageCountPerTab = 5;
        [Tooltip("The previous button.")]
        [SerializeField] protected Button m_PreviousButton;
        [Tooltip("The next button.")]
        [SerializeField] protected Button m_NextButton;

        protected TabToggle[] m_TabToggles;

        protected int m_TabIndex = -1;
        private bool m_Initialized = false;

        public IReadOnlyList<TabToggle> TabToggles => m_TabToggles;

        public int TabIndex => m_TabIndex;
        public int TabCount => m_TabToggles.Length;

        public bool ValidTabIndex(int index) => (index >= 0 && index < m_TabToggles.Length);
        public TabToggle CurrentTab => ValidTabIndex(m_TabIndex) ? m_TabToggles[m_TabIndex] : null;

        /// <summary>
        /// Initialize.
        /// </summary>
        private void Awake()
        {
            Initialize(false);
        }

        /// <summary>
        /// Initialize the component.
        /// </summary>
        public virtual void Initialize(bool force)
        {
            if (m_Initialized && force == false) { return; }

            if (m_Content == null) { m_Content = transform as RectTransform; }

            m_TabToggles = m_Content.GetComponentsInChildren<TabToggle>();

            for (int i = 0; i < m_TabToggles.Length; i++) {
                var tabToggle = m_TabToggles[i];
                tabToggle.SetControl(this, i);
                tabToggle.ToggleOff();
            }

            if (m_PreviousButton != null) {
                m_PreviousButton.onClick.AddListener(() => PreviousTab());
            }

            if (m_NextButton != null) {
                m_NextButton.onClick.AddListener(() => NextTab());
            }

            SetTabOn(0);

            m_Initialized = true;
        }

        /// <summary>
        /// Is the next tab available.
        /// </summary>
        /// <returns>True if it exists.</returns>
        public bool IsNextTabAvailable()
        {
            return m_TabIndex + 1 < m_TabToggles.Length;
        }

        /// <summary>
        /// Is the previous tab available.
        /// </summary>
        /// <returns>True if it exists.</returns>
        public bool IsPreviousTabAvailable()
        {
            return m_TabIndex > 0;
        }

        /// <summary>
        /// Got to the next tab.
        /// </summary>
        /// <returns>The new tab element.</returns>
        public bool NextTab()
        {
            if (!IsNextTabAvailable()) {
                return false;
            }

            SetTabOn(m_TabIndex + 1);
            return true;
        }

        /// <summary>
        /// Go to the previous tab.
        /// </summary>
        /// <returns>The previous tab element.</returns>
        public bool PreviousTab()
        {
            if (!IsPreviousTabAvailable()) {
                return false;
            }

            SetTabOn(m_TabIndex - 1);

            return true;
        }

        /// <summary>
        /// Set the selected tab
        /// </summary>
        /// <param name="tabToggle">The tab to toggle.</param>
        public void SelectTab(TabToggle tabToggle)
        {
            var newIndex = -1;
            for (int i = 0; i < m_TabToggles.Length; i++) {
                if (m_TabToggles[i] != tabToggle) { continue; }

                newIndex = i;
                break;
            }

            if (newIndex == -1) { return; }

            SetTabOn(newIndex);
        }

        /// <summary>
        /// Set the tab on.
        /// </summary>
        /// <param name="index">The tab index.</param>
        public void SetTabOn(int index)
        {
            if (index < 0 || index >= m_TabToggles.Length) { return; }

            var previousIndex = m_TabIndex;
            m_TabIndex = index;

            for (int i = 0; i < m_TabToggles.Length; i++) {
                if (index == i) {
                    m_TabToggles[i].ToggleOn();
                } else {
                    m_TabToggles[i].ToggleOff();
                }
            }

            RefreshArrows();
            OnTabChange?.Invoke(previousIndex, m_TabIndex);
        }

        /// <summary>
        /// Refresh the arrows, hide if unavailable.
        /// </summary>
        public void RefreshArrows()
        {
            if (m_PreviousButton != null) {
                m_PreviousButton.gameObject.SetActive(m_TabIndex != 0);
            }

            if (m_NextButton != null) {
                m_NextButton.gameObject.SetActive(m_TabIndex != TabCount - 1);
            }
        }
    }
}