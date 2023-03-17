/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using Text = Opsive.Shared.UI.Text;

    /// <summary>
    /// A confirmation panel.
    /// </summary>
    public class ConfirmationPopUp : DisplayPanel
    {
        [Tooltip("The title text.")]
        [SerializeField] protected Text m_Title;
        [Tooltip("The confirm button.")]
        [SerializeField] protected Button m_ConfirmButton;
        [Tooltip("The cancel button.")]
        [SerializeField] protected Button m_CancelButton;
        [Tooltip("The text was changed")]
        [SerializeField] protected StringUnityEvent m_OnTitleTextChange;

        protected Action m_ConfirmAction;
        protected Action m_CancelAction;

        public StringUnityEvent OnTitleTextChange => m_OnTitleTextChange;

        /// <summary>
        /// Set the events to the buttons.
        /// </summary>
        private void Awake()
        {
            m_ConfirmButton.onClick.AddListener(OnConfirmClick);
            m_CancelButton.onClick.AddListener(OnCancelClick);
        }

        /// <summary>
        /// Configure the popup with the actions.
        /// </summary>
        /// <param name="titleText">The title text.</param>
        /// <param name="confirmAction">The confirm action.</param>
        /// <param name="cancelAction">The cancel action.</param>
        public void ConfigurePopup(string titleText, Action confirmAction, Action cancelAction)
        {
            SetTitle(titleText);
            SetConfirmAction(cancelAction);
            SetCancelAction(cancelAction);
        }

        /// <summary>
        /// Cancel Button is clicked.
        /// </summary>
        private void OnCancelClick()
        {
            if (m_CancelAction != null) {
                m_CancelAction.Invoke();
            }
            Close();
        }

        /// <summary>
        /// Confirm button is clicked.
        /// </summary>
        private void OnConfirmClick()
        {
            if (m_ConfirmAction != null) {
                m_ConfirmAction.Invoke();
            }
            Close();
        }

        /// <summary>
        /// Set the title of the panel.
        /// </summary>
        /// <param name="title">The title.</param>
        public void SetTitle(string title)
        {
            m_Title.text = title;
            m_OnTitleTextChange?.Invoke(title);
        }

        /// <summary>
        /// Set the confirmation action.
        /// </summary>
        /// <param name="confirmAction">The confirmation action.</param>
        public void SetConfirmAction(Action confirmAction)
        {
            m_ConfirmAction = confirmAction;
        }

        /// <summary>
        /// Set the cancel action.
        /// </summary>
        /// <param name="cancelAction">The cancel action.</param>
        public void SetCancelAction(Action cancelAction)
        {
            m_CancelAction = cancelAction;
        }
    }
}
