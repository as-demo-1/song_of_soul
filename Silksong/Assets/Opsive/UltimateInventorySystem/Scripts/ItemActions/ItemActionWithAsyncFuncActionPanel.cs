/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.ItemActions
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using Opsive.UltimateInventorySystem.UI.Panels.ActionPanels;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Item action With asynchronous function action panel. It lets you open a action panel and assign actions to buttons. 
    /// </summary>
    /// <typeparam name="T">The type of the action parameter.</typeparam>
    [System.Serializable]
    public abstract class ItemActionWithAsyncFuncActionPanel<T> : ItemAction, IActionWithPanel
    {
        [Tooltip("The asynchronous functions action panel prefab.")]
        [SerializeField] protected GameObject m_AsyncFuncActionPanelPrefab;

        protected AsyncFuncActionsPanel<T> m_AsyncFuncActionPanel;
        protected ResizableArray<AsyncFuncAction<T>> m_AsyncFuncActions;

        protected Transform m_PanelParentTransform;
        protected DisplayPanel m_PanelParentPanel;
        protected Selectable m_PreviousSelected;

        /// <summary>
        /// Initializes the Item Action.
        /// <param name="force">Force the initialization.</param>
        /// </summary>
        public override void Initialize(bool force)
        {
            if (m_Initialized && !force) { return; }

            if (m_Initialized == false) {
                m_AsyncFuncActions = new ResizableArray<AsyncFuncAction<T>>();
            }

            base.Initialize(force);
        }

        /// <summary>
        /// Set the parent panel.
        /// </summary>
        /// <param name="parentDisplayPanel">The parent panel.</param>
        /// <param name="previousSelectable">THe previous selectable.</param>
        /// <param name="parentTransform">The parent transform.</param>
        public virtual void SetParentPanel(DisplayPanel parentDisplayPanel, Selectable previousSelectable, Transform parentTransform)
        {
            m_PanelParentTransform = parentTransform;
            m_PanelParentPanel = parentDisplayPanel;
            m_PreviousSelected = previousSelectable;
        }

        /// <summary>
        /// Invoke the action.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        protected override void InvokeActionInternal(ItemInfo itemInfo, ItemUser itemUser)
        {
            if (m_AsyncFuncActionPanelPrefab == null) {
                Debug.LogWarning("Async Func Action Panel Prefab is null on the Item Action.");
                return;
            }
            var instance = ObjectPool.Instantiate(m_AsyncFuncActionPanelPrefab, m_PanelParentTransform);
            var rectTransform = instance.GetCachedComponent<RectTransform>();
            rectTransform.anchoredPosition = Vector2.zero;
            m_AsyncFuncActionPanel = instance.GetComponent<AsyncFuncActionsPanel<T>>();
            m_AsyncFuncActionPanel.Setup(m_PanelParentPanel.Manager, false);
            m_AsyncFuncActionPanel.AssignActions(m_AsyncFuncActions);
            m_AsyncFuncActionPanel.Open(m_PanelParentPanel, m_PreviousSelected);
#pragma warning disable 4014
            AssignActionAsync(itemInfo, itemUser);
#pragma warning restore 4014
        }

        /// <summary>
        /// Assign the action.
        /// </summary>
        /// <param name="itemInfo">The item Info.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        /// <returns>The task.</returns>
        protected virtual async Task AssignActionAsync(ItemInfo itemInfo, ItemUser itemUser)
        {
            var returnedValue = await m_AsyncFuncActionPanel.WaitForReturnedValueAsync();

            InvokeWithAwaitedValue(itemInfo, itemUser, returnedValue);
        }

        /// <summary>
        /// Invoke with the action with the awaited value. 
        /// </summary>
        /// <param name="itemInfo">The itemInfo.</param>
        /// <param name="itemUser">The item user.</param>
        /// <param name="awaitedValue">The value that was waited for.</param>
        protected abstract void InvokeWithAwaitedValue(ItemInfo itemInfo, ItemUser itemUser, T awaitedValue);
    }
}