/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.ActionPanels
{
    using Opsive.UltimateInventorySystem.UI.CompoundElements;
    using Opsive.UltimateInventorySystem.UI.Grid;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// A base class to create asynchronous actions in a panel.
    /// </summary>
    /// <typeparam name="T">The type of the action parameter.</typeparam>
    public abstract class AsyncFuncActionsPanel<T> : DisplayPanel
    {
        [Tooltip("The action button prefab. Requires an ActionButton component.")]
        [SerializeField] protected GameObject m_ActionButtonPrefab;
        [Tooltip("The parent for the buttons.")]
        [SerializeField] protected Transform m_ButtonParent;

        private List<ActionButton> m_ItemActionButtons = new List<ActionButton>();
        private LayoutGroupNavigation m_LayoutNavigation;
        private IList<AsyncFuncAction<T>> m_Actions;

        protected bool m_WaitForInput = true;
        protected T m_ValueToReturn;
        protected bool m_Canceled;

        public bool Canceled => m_Canceled;
        public T ValueToReturn => m_ValueToReturn;

        /// <summary>
        /// Wait for a return value before processing it.
        /// </summary>
        /// <returns>Returns the task.</returns>
        public virtual async Task<T> WaitForReturnedValueAsync()
        {
            while (m_WaitForInput) {

                if (m_IsOpen == false) {
                    return await Task.FromCanceled<T>(CancellationToken.None);
                }

                await Task.Yield();
            }

            return m_ValueToReturn;
        }

        /// <summary>
        /// Open the panel and setup the buttons.
        /// </summary>
        protected override void OpenInternal()
        {
            base.OpenInternal();

            if (m_ButtonParent == null) { m_ButtonParent = transform; }

            if (m_LayoutNavigation == null) {
                m_LayoutNavigation = m_ButtonParent.GetComponent<LayoutGroupNavigation>();
            }

            var count = m_Actions.Count + 1;

            m_WaitForInput = true;

            for (int i = 0; i < count; i++) {

                if (m_ButtonParent.childCount <= i) {
                    var newButton = Instantiate(m_ActionButtonPrefab, m_ButtonParent).GetComponent<ActionButton>();
                    m_ItemActionButtons.Add(newButton);
                }

                if (m_ItemActionButtons.Count <= i) {
                    m_ItemActionButtons.Add(m_ButtonParent.GetChild(i).GetComponent<ActionButton>());
                }

                m_ItemActionButtons[i].gameObject.SetActive(true);

                var localIndex = i;

                if (i == count - 1) {
                    m_ItemActionButtons[i].SetButtonName("Cancel");
                    m_ItemActionButtons[i].SetButtonAction(() => ButtonClicked(-1));
                } else {
                    m_ItemActionButtons[i].SetButtonName(m_Actions[i].Name);
                    m_ItemActionButtons[i].SetButtonAction(() => ButtonClicked(localIndex));
                }
            }

            for (int i = count; i < m_ButtonParent.childCount; i++) {
                m_ButtonParent.GetChild(i).gameObject.SetActive(false);
            }

            m_ItemActionButtons[0].Select();

            m_LayoutNavigation.RefreshNavigation();
        }

        /// <summary>
        /// Process a button being clicked.
        /// </summary>
        /// <param name="index">The index of the button clicked.</param>
        protected virtual void ButtonClicked(int index)
        {
            if (index < 0 || index >= m_Actions.Count) {
                m_ValueToReturn = GetCancelValue();
                m_Canceled = true;
            }else{
                m_ValueToReturn = m_Actions[index].Func.Invoke();
                m_Canceled = false;
            }
            Close();
            m_WaitForInput = false;
        }

        protected abstract T GetCancelValue();

        /// <summary>
        /// Assign the asynchronous actions to the buttons.
        /// </summary>
        /// <param name="funcActions">The list of actions.</param>
        public void AssignActions(IList<AsyncFuncAction<T>> funcActions)
        {
            m_Actions = funcActions;
        }
    }
}