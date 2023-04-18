/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.CompoundElements
{
    using Opsive.UltimateInventorySystem.Input;
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using Text = Opsive.Shared.UI.Text;

    /// <summary>
    /// A quantity picker UI.
    /// </summary>
    public class QuantityPicker : MonoBehaviour
    {
        public event Action<int> OnAmountChanged;
        public event Action OnMainButtonClicked;

        [Tooltip("The main action button, press when confirm quantity.")]
        [SerializeField] protected ActionButton m_MainButton;
        [Tooltip("The button to add quantity.")]
        [SerializeField] protected Button m_PlusQuantity;
        [Tooltip("The button to lower quantity.")]
        [SerializeField] protected Button m_MinusQuantity;
        [Tooltip("The button that sets the value to the max value.")]
        [SerializeField] protected Button m_MaxQuantityButton;
        [Tooltip("The button that sets the value to the min value.")]
        [SerializeField] protected Button m_MinQuantityButton;
        [Tooltip("The quantity text.")]
        [SerializeField] protected Text m_QuantityText;
        [Tooltip("The start quantity.")]
        [SerializeField] protected int m_StartQuantity = 1;
        [Tooltip("The minimum quantity.")]
        [SerializeField] protected int m_MinQuantity = 1;
        [Tooltip("The maximum quantity.")]
        [SerializeField] protected int m_MaxQuantity = 99;

        protected int m_Quantity;

        public ActionButton MainButton => m_MainButton;

        public int Quantity => m_Quantity;
        public int MinQuantity {
            get => m_MinQuantity;
            set => m_MinQuantity = value;
        }
        public int MaxQuantity {
            get => m_MaxQuantity;
            set => m_MaxQuantity = value;
        }

        /// <summary>
        /// Initialize the component.
        /// </summary>
        void Start()
        {
            if (m_PlusQuantity != null) {
                m_PlusQuantity.onClick.AddListener(() => AdjustAmount(1));
            }

            if (m_MinusQuantity != null) {
                m_MinusQuantity.onClick.AddListener(() => AdjustAmount(-1));
            }

            if (m_MaxQuantityButton != null) {
                m_MaxQuantityButton.onClick.AddListener(() => SetQuantity(m_MaxQuantity));
            }

            if (m_MinQuantityButton != null) {
                m_MinQuantityButton.onClick.AddListener(() => SetQuantity(m_MinQuantity));
            }

            m_MainButton.OnSubmitE += () => OnMainButtonClicked?.Invoke();
            m_MainButton.OnMoveE += OnMove;

            SetQuantity(m_StartQuantity);
        }

        /// <summary>
        /// On move event.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <param name="moved">Did it move.</param>
        protected virtual void OnMove(AxisEventData eventData, bool moved)
        {
            switch (eventData.moveDir) {
                case MoveDirection.Left:
                    if (m_MinQuantityButton != null) { SetQuantity(m_MinQuantity); }
                    break;
                case MoveDirection.Up:
                    AdjustAmount(1);
                    break;
                case MoveDirection.Right:
                    if (m_MaxQuantityButton != null) { SetQuantity(m_MaxQuantity); }
                    break;
                case MoveDirection.Down:
                    AdjustAmount(-1);
                    break;
            }
        }

        /// <summary>
        /// Adjust the amount.
        /// </summary>
        /// <param name="delta">The delta difference.</param>
        public virtual void AdjustAmount(int delta)
        {
            var newAmount = Mathf.Clamp(m_Quantity + delta, m_MinQuantity, m_MaxQuantity);
            QuantityChanged(newAmount);
        }

        /// <summary>
        /// Set the amount.
        /// </summary>
        /// <param name="newQuantity">The new amount.</param>
        public virtual void SetQuantity(int newQuantity)
        {
            newQuantity = Mathf.Clamp(newQuantity, m_MinQuantity, m_MaxQuantity);
            QuantityChanged(newQuantity);
        }

        /// <summary>
        /// Call the quantity changed event.
        /// </summary>
        /// <param name="newQuantity">The new amount.</param>
        protected virtual void QuantityChanged(int newQuantity)
        {
            if (m_Quantity == newQuantity) { return; }

            m_Quantity = newQuantity;
            m_QuantityText.text = m_Quantity.ToString();
            OnAmountChanged?.Invoke(m_Quantity);
        }

        /// <summary>
        /// Select the main Button.
        /// </summary>
        public void SelectMainButton()
        {
            m_MainButton.Select();
        }
    }

}

