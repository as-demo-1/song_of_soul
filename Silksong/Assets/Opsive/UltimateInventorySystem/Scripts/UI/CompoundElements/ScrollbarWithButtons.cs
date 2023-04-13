/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.CompoundElements
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Scrollbar with buttons to increment the scroll bar index.
    /// </summary>
    public class ScrollbarWithButtons : MonoBehaviour
    {
        public event Action<int> OnScrollIndexChanged;
        public event Action<float> OnScrollValueChanged;

        [Tooltip("The scrollbar.")]
        [SerializeField] protected Scrollbar m_Scrollbar;
        [Tooltip("Set the steps to discrete.")]
        [SerializeField] protected bool m_DiscreteSteps = true;
        [Tooltip("The button to scroll up.")]
        [SerializeField] protected Button m_PositiveButton;
        [Tooltip("The button to scroll down.")]
        [SerializeField] protected Button m_NegativeButton;

        protected int m_CurrentIndex;
        protected int m_MaxIndex;

        [System.NonSerialized] protected bool m_Initialized = false;

        public Button PositiveButton => m_PositiveButton;
        public Button NegativeButton => m_NegativeButton;
        public Scrollbar Scrollbar => m_Scrollbar;

        public bool DiscreteSteps {
            get => m_DiscreteSteps;
            set => m_DiscreteSteps = value;
        }

        /// <summary>
        /// Initialize the component.
        /// </summary>
        private void OnEnable()
        {
            if (m_Initialized) { return; }

            m_PositiveButton.onClick.AddListener(PositivePressed);
            m_NegativeButton.onClick.AddListener(NegativePressed);
            m_Scrollbar.onValueChanged.AddListener(ScrollValueChanged);

            m_Initialized = true;
        }

        /// <summary>
        /// The positive button was pressed.
        /// </summary>
        public void PositivePressed()
        {
            ScrollIndexChangedInternal(m_CurrentIndex + 1, true, false);
        }

        /// <summary>
        /// The negative button was pressed.
        /// </summary>
        public void NegativePressed()
        {
            ScrollIndexChangedInternal(m_CurrentIndex - 1, true, false);
        }

        /// <summary>
        /// The scroll value changed.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        private void ScrollValueChanged(float newValue)
        {
            var step = newValue * (m_MaxIndex + 1);
            ScrollIndexChangedInternal((int)step, m_DiscreteSteps, false);
            OnScrollValueChanged?.Invoke(newValue);
        }

        /// <summary>
        /// Set the scroll step.
        /// </summary>
        /// <param name="step">The scroll step.</param>
        public void SetScrollStep(int step, bool setScrollValue, bool keepOffset)
        {
            ScrollIndexChangedInternal(step, setScrollValue, keepOffset);
        }

        /// <summary>
        /// The scroll value changed.
        /// </summary>
        /// <param name="newValue">The new scroll value.</param>
        protected void ScrollIndexChangedInternal(int newValue, bool setScrollValue, bool keepOffset)
        {
            var previousValue = m_CurrentIndex;
            m_CurrentIndex = Mathf.Clamp(newValue, 0, m_MaxIndex);

            if (setScrollValue) {
                var normalizedStep = m_MaxIndex == 0 ? 0 : (float)m_CurrentIndex / m_MaxIndex;

                var newScrollValue = normalizedStep;

                if (keepOffset) {
                    var oneStep = m_MaxIndex == 0 ? 0 : (float)1 / m_MaxIndex;
                    var offset = m_Scrollbar.value % oneStep;
                    newScrollValue += offset;
                }

                m_Scrollbar.SetValueWithoutNotify(Mathf.Clamp(newScrollValue, 0f, 1f));
            }

            if (m_CurrentIndex == previousValue) { return; }

            OnScrollIndexChanged?.Invoke(m_CurrentIndex);
        }

        /// <summary>
        /// Set the max number of steps for the scrollbar.
        /// </summary>
        /// <param name="maxSize">The max size.</param>
        public void SetupScrollbar(int maxSize)
        {
            if (maxSize <= 0) { maxSize = 0; }

            m_MaxIndex = maxSize;
            m_Scrollbar.numberOfSteps = m_DiscreteSteps ? m_MaxIndex + 1 : 0;
            m_Scrollbar.size = 1f / (m_MaxIndex + 1);
        }
    }
}