/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.ActionPanels
{
    using Opsive.UltimateInventorySystem.ItemActions;
    using Opsive.UltimateInventorySystem.UI.CompoundElements;
    using Opsive.UltimateInventorySystem.UI.Grid;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Action panel is used to easily assign actions to a list of buttons in a panel.
    /// </summary>
    /// <typeparam name="T">The type of the Actions.</typeparam>
    public class ActionPanel<T> : DisplayPanel where T : ActionElement
    {
        public event Action<int> OnBeforeAnyItemActionInvoke;
        public event Action<int> OnAfterAnyItemActionInvoke;

        [Tooltip("The action button prefab.")]
        [SerializeField] internal GameObject m_ActionButtonPrefab;
        [Tooltip("The buttons container transform.")]
        [SerializeField] protected Transform m_ButtonsParent;
        [Tooltip("Add a cancel option to close the panel.")]
        [SerializeField] protected bool m_AddCancelOption = true;

        protected List<ActionButton> m_ItemActionButtons = new List<ActionButton>();
        protected LayoutGroupNavigation m_LayoutNavigation;
        protected IList<T> m_Actions;

        /// <summary>
        /// Assign the actions that the buttons should have.
        /// </summary>
        /// <param name="actions">The actions.</param>
        public void AssignActions(IList<T> actions)
        {
            m_Actions = actions;
        }

        /// <summary>
        /// Setup the buttons each time the panel is opened.
        /// </summary>
        protected override void OpenInternal()
        {
            base.OpenInternal();

            if (m_ButtonsParent == null) { m_ButtonsParent = transform; }

            if (m_LayoutNavigation == null) {
                m_LayoutNavigation = m_ButtonsParent.GetComponent<LayoutGroupNavigation>();
            }

            var actionCount = m_Actions.Count;

            for (int i = 0; i < actionCount; i++) {
                var actionButton = AddButtonAtIndex(i);

                var localIndex = i;

                m_Actions[i].Initialize(false);
                actionButton.interactable = m_Actions[i].CanInvoke();

                actionButton.SetButtonName(m_Actions[i].Name);
                actionButton.SetButtonAction(() => InvokeAction(localIndex));
            }

            if (m_AddCancelOption) {
                var actionButton = AddButtonAtIndex(actionCount);

                actionButton.SetButtonName("Cancel");
                actionButton.SetButtonAction(() => Close());

                actionCount += 1;
            }

            for (int i = actionCount; i < m_ButtonsParent.childCount; i++) {
                m_ButtonsParent.GetChild(i).gameObject.SetActive(false);
            }

            m_ItemActionButtons[0].Select();

            if (m_LayoutNavigation != null) {
                m_LayoutNavigation.RefreshNavigation();
            } else {
                Debug.LogWarning("The action panel is missing a UILayoutGroupNavigation component on the buttons parent.");
            }
        }

        /// <summary>
        /// Add the button to a specific index.
        /// </summary>
        /// <param name="i">The index to add the button to.</param>
        /// <returns>The Action button. created.</returns>
        private ActionButton AddButtonAtIndex(int i)
        {
            if (m_ButtonsParent.childCount <= i) { Instantiate(m_ActionButtonPrefab, m_ButtonsParent); }

            if (m_ItemActionButtons.Count <= i) {
                m_ItemActionButtons.Add(m_ButtonsParent.GetChild(i).GetComponent<ActionButton>());
            }

            m_ItemActionButtons[i].gameObject.SetActive(true);
            return m_ItemActionButtons[i];
        }

        /// <summary>
        /// Invoke the action on the button with the index provided.
        /// </summary>
        /// <param name="index">The index of the button.</param>
        protected virtual void InvokeAction(int index)
        {
            BeforeActionInvoke(index);

            InvokeActionInternal(index);

            AfterActionInvoke(index);
        }

        /// <summary>
        /// This function is called before the action is invoked.
        /// </summary>
        /// <param name="index">The index of the action.</param>
        protected virtual void BeforeActionInvoke(int index)
        {
            var action = m_Actions[index];

            //Close before action is invoked in case a panel needs to be selected in the action.
            if (action.CloseOnInvoke) {
                // Set the previous panel as the item action panel.
                if (action is IActionWithPanel itemActionWithPanel) {
                    itemActionWithPanel.SetParentPanel(m_PreviousPanel, m_PreviousSelectable, m_PreviousPanel.MainContent);
                }
                Close();
            } else {
                // Set this panel as item action panel.
                if (action is IActionWithPanel itemActionWithPanel) {
                    itemActionWithPanel.SetParentPanel(this, m_ItemActionButtons[index], MainContent);
                }
            }

            OnBeforeAnyItemActionInvoke?.Invoke(index);
        }

        /// <summary>
        /// Invoke the action on the button with the index provided.
        /// </summary>
        /// <param name="index">The index of the button.</param>
        protected virtual void InvokeActionInternal(int index)
        {
            var action = m_Actions[index];
            action.InvokeAction();
        }

        /// <summary>
        /// This function is called after the action was invoked.
        /// </summary>
        /// <param name="index">The action index.</param>
        protected virtual void AfterActionInvoke(int index)
        {
            OnAfterAnyItemActionInvoke?.Invoke(index);
        }
    }
}