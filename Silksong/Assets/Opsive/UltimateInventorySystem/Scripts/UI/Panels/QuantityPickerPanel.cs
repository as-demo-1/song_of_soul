/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels
{
    using Opsive.UltimateInventorySystem.UI.CompoundElements;
    using System;
    using System.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// The Quantity picker panel.
    /// </summary>
    public class QuantityPickerPanel : DisplayPanel
    {
        public event Action<int> OnAmountChanged;

        [Tooltip("The quantity picker.")]
        [SerializeField] protected QuantityPicker m_QuantityPicker;
        [Tooltip("The confirm and cancel panel.")]
        [SerializeField] protected ConfirmCancelPanel m_ConfirmCancelPanel;

        protected bool m_QuantityButtonEnabled = true;

        public QuantityPicker QuantityPicker => m_QuantityPicker;
        public ConfirmCancelPanel ConfirmCancelPanel => m_ConfirmCancelPanel;

        /// <summary>
        /// Set up the panel.
        /// </summary>
        public override void Setup(DisplayPanelManager manager, bool force)
        {
            if (m_IsSetup) { return; }

            base.Setup(manager, force);

            m_ConfirmCancelPanel.Setup(manager, force);

            m_QuantityPicker.OnAmountChanged += (x) => OnAmountChanged?.Invoke(x);
            m_QuantityPicker.OnMainButtonClicked += QuantityPickerClicked;

            m_ConfirmCancelPanel.OnClose += () => Close();

            EnableQuantityButton();
        }

        /// <summary>
        /// Enable the quantity buttons.
        /// </summary>
        public void EnableQuantityButton()
        {
            m_QuantityButtonEnabled = true;
        }

        /// <summary>
        /// Disable the quantity buttons.
        /// </summary>
        public void DisableQuantityButton()
        {
            m_QuantityButtonEnabled = false;
        }

        /// <summary>
        /// The Quantity picker was clicked, open the confirm/cancel panel.
        /// </summary>
        private void QuantityPickerClicked()
        {
            if (!m_QuantityButtonEnabled) { return; }
            m_ConfirmCancelPanel.Open(null, null);
        }

        /// <summary>
        /// Wait for the player to chose a quantity.
        /// </summary>
        /// <returns>The task.</returns>
        public async Task<int> WaitForQuantity()
        {
            await Task.Yield();

            if (m_IsOpen == false || gameObject.activeInHierarchy == false) {
                return -1;
            }

            var confirm = await m_ConfirmCancelPanel.WaitForConfirmAsync();

            if (confirm) { return QuantityPicker.Quantity; }

            return -1;

        }
    }
}
