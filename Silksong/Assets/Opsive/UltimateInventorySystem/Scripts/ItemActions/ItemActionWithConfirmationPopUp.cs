/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.ItemActions
{
    using System;
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// An item action which can nest other item actions and uses a confirmation pop up to decide which ones to invoke.
    /// </summary>
    [Serializable]
    public class ItemActionWithConfirmationPopUp : ItemAction, IActionWithPanel
    {
        [Tooltip("The Confirmation Pop up prefab that will be spawned.")]
        [SerializeField] protected ConfirmationPopUp m_ConfirmationPopUpPrefab;
        [Tooltip("The Item Action Set to be invoked when confirming.")]
        [SerializeField] protected ItemActionSet m_ItemActionSetOnConfirm;
        [Tooltip("The Item Action Set to be invoked when canceling.")]
        [SerializeField] protected ItemActionSet m_ItemActionSetOnCancel;
        
        protected DisplayPanel m_ParentPanel;
        protected Selectable m_PreviousSelectable;
        protected Transform m_ParentTransform;
        
        protected ConfirmationPopUp m_ConfirmationPopUp;
        protected Action m_OnConfirm;
        protected Action m_OnCancel;

        protected ItemInfo m_ItemInfo;
        protected ItemUser m_ItemUser;

        /// <summary>
        /// Initialize the item action.
        /// </summary>
        /// <param name="force">Force initialize?</param>
        public override void Initialize(bool force)
        {
            var wasInitialized = m_Initialized;
            base.Initialize(force);
            
            if(wasInitialized && !force){ return; }
            
            m_ItemActionSetOnConfirm?.ItemActionCollection.Initialize(force);
            m_ItemActionSetOnCancel?.ItemActionCollection.Initialize(force);
            
            if (Application.isPlaying) {
                if (m_ConfirmationPopUpPrefab == null) {
                    Debug.LogWarning($"The item Action '{Name}' is missing a reference to a Confirmation pop up prefab.");
                }
            }

            m_OnConfirm = OnConfirm;
            m_OnCancel = OnCancel;

        }

        /// <summary>
        /// Set the parent panel.
        /// </summary>
        /// <param name="parentDisplayPanel">The parent panel.</param>
        /// <param name="previousSelectable">THe previous selectable.</param>
        /// <param name="parentTransform">The parent transform.</param>
        public void SetParentPanel(DisplayPanel parentDisplayPanel, Selectable previousSelectable, Transform parentTransform)
        {
            m_ParentPanel = parentDisplayPanel;
            m_PreviousSelectable = previousSelectable;
            m_ParentTransform = parentTransform;
        }

        /// <summary>
        /// Can the action be invoked?
        /// </summary>
        /// <param name="itemInfo">The item Info.</param>
        /// <param name="itemUser">The item user.</param>
        /// <returns>Always true.</returns>
        protected override bool CanInvokeInternal(ItemInfo itemInfo, ItemUser itemUser)
        {
            return true;
        }

        /// <summary>
        /// Invoke the item action opens up the confirmation panel.
        /// </summary>
        /// <param name="itemInfo">The item Info.</param>
        /// <param name="itemUser">The item user.</param>
        protected override void InvokeActionInternal(ItemInfo itemInfo, ItemUser itemUser)
        {
            m_ItemInfo = itemInfo;
            m_ItemUser = itemUser;
            
            var instance = ObjectPool.Instantiate(m_ConfirmationPopUpPrefab.gameObject, m_ParentTransform);
            m_ConfirmationPopUp = instance.GetCachedComponent<ConfirmationPopUp>();
            m_ConfirmationPopUp.Setup(m_ParentPanel.Manager, false);
            
            m_ConfirmationPopUp.SetConfirmAction(m_OnConfirm);
            m_ConfirmationPopUp.SetCancelAction(m_OnCancel);
            m_ConfirmationPopUp.Open(m_ParentPanel, m_PreviousSelectable);
        }

        /// <summary>
        /// Executed when canceled is clicked.
        /// </summary>
        private void OnCancel()
        {
            if (m_ItemActionSetOnCancel != null) {
                for (int i = 0; i < m_ItemActionSetOnCancel.ItemActionCollection.Count; i++) {
                    var itemAction = m_ItemActionSetOnCancel.ItemActionCollection[i];
                    itemAction.InvokeAction(m_ItemInfo,m_ItemUser);
                }
            }

            ObjectPool.Destroy(m_ConfirmationPopUp.gameObject);
        }

        /// <summary>
        /// Executed when confirm is clicked.
        /// </summary>
        private void OnConfirm()
        {
            if (m_ItemActionSetOnConfirm != null) {
                for (int i = 0; i < m_ItemActionSetOnConfirm.ItemActionCollection.Count; i++) {
                    var itemAction = m_ItemActionSetOnConfirm.ItemActionCollection[i];
                    itemAction.InvokeAction(m_ItemInfo,m_ItemUser);
                }
            }

            ObjectPool.Destroy(m_ConfirmationPopUp.gameObject);
        }
    }
}