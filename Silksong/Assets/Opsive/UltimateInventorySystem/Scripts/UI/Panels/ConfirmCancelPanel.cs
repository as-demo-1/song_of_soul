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
    using UnityEngine.Events;

    [Serializable]
    public class StringUnityEvent : UnityEvent<string>{ }
    
    /// <summary>
    /// A panel used to confirm or cancel an action.
    /// </summary>
    public class ConfirmCancelPanel : DisplayPanel
    {
        public event Action OnConfirm;
        public event Action OnCancel;

        [Tooltip("The confirm button and text.")]
        [SerializeField] protected ButtonWithText m_Confirm;
        [Tooltip("The cancel button and text.")]
        [SerializeField] protected ButtonWithText m_Cancel;

        [Tooltip("The text was changed")]
        [SerializeField] protected StringUnityEvent m_OnConfirmTextChange;
        [Tooltip("The text was changed")]
        [SerializeField] protected StringUnityEvent m_OnCancelTextChange;
        
        public StringUnityEvent OnConfirmTextChange => m_OnConfirmTextChange;
        public StringUnityEvent OnCancelTextChange => m_OnCancelTextChange;

        protected bool m_WaitForInput = true;
        protected bool m_PressedConfirm = false;

        /// <summary>
        /// Set up the panel.
        /// </summary>
        public override void Setup(DisplayPanelManager manager, bool force)
        {
            if (m_IsSetup == true) { return; }
            base.Setup(manager, force);

            m_Confirm.AddClickEvent(ClickedConfirm);
            m_Cancel.AddClickEvent(ClickedCancel);
        }

        /// <summary>
        /// The confirm button was clicked.
        /// </summary>
        private void ClickedConfirm()
        {
            m_PressedConfirm = true;
            OnConfirm?.Invoke();
            Close();
            m_WaitForInput = false;
        }

        /// <summary>
        /// The cancel button was clicked.
        /// </summary>
        private void ClickedCancel()
        {
            m_PressedConfirm = false;
            OnCancel?.Invoke();
            Close();
            m_WaitForInput = false;
        }

        /// <summary>
        /// Open the panel.
        /// </summary>
        protected override void OpenInternal()
        {
            base.OpenInternal();
            m_PressedConfirm = false;
            m_WaitForInput = true;
        }

        /// <summary>
        /// Wait for the cancel or confirm button to be pressed.
        /// </summary>
        /// <returns>The task.</returns>
        public virtual async Task<bool> WaitForConfirmAsync()
        {
            while (m_WaitForInput) {
                if (gameObject.activeInHierarchy == false) {
                    m_WaitForInput = true;
                    return m_PressedConfirm;
                }
                await Task.Yield();
            }

            m_WaitForInput = true;
            return m_PressedConfirm;
        }

        /// <summary>
        /// Set the text for the confirm button.
        /// </summary>
        /// <param name="text">The text.</param>
        public void SetConfirmText(string text)
        {
            m_Confirm.Text.SetText(text);
            m_OnConfirmTextChange?.Invoke(text);
        }

        /// <summary>
        /// Set the text for the cancel button.
        /// </summary>
        /// <param name="text">The text.</param>
        public void SetCancelText(string text)
        {
            m_Cancel.Text.SetText(text);
            m_OnCancelTextChange?.Invoke(text);
        }

        /// <summary>
        /// Enable or disable the confirm button.
        /// </summary>
        /// <param name="text">Should the confirm button be enabled or disabled?</param>
        public void EnableConfirm(bool enable)
        {
            m_Confirm.EnableButton(enable);
        }

    }
}