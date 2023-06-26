/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.ItemActions
{
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Item action With asynchronous function action panel. It lets you open a action panel and assign actions to buttons. 
    /// </summary>
    /// <typeparam name="T">The type of the action parameter.</typeparam>
    [System.Serializable]
    public abstract class ItemActionWithQuantityPickerPanel : ItemAction, IActionWithPanel
    {
        [Tooltip("The asynchronous functions action panel prefab.")]
        [SerializeField] protected GameObject m_QuantityPanelPrefab;

        protected QuantityPickerPanel m_QuantityPickerPanel;

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

            if (Application.isPlaying) {
                if (m_QuantityPanelPrefab == null ||
                    m_QuantityPanelPrefab.GetCachedComponent<QuantityPickerPanel>() == null) {
                    Debug.LogWarning($"The item Action '{Name}' is missing a reference to a Quantity Picker Panel, Make sure the quantity picker panel is on the prefab.");
                }
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

            var instance = ObjectPool.Instantiate(m_QuantityPanelPrefab, m_PanelParentTransform);
            m_QuantityPickerPanel = instance.GetCachedComponent<QuantityPickerPanel>();
            m_QuantityPickerPanel.Setup(m_PanelParentPanel.Manager, false);

            SetupQuantityPickerSettings(itemInfo, itemUser, m_QuantityPickerPanel);

            m_QuantityPickerPanel.Open(m_PanelParentPanel, m_PreviousSelected);
#pragma warning disable 4014
            AssignActionAsync(itemInfo, itemUser);
#pragma warning restore 4014
        }

        /// <summary>
        /// Setup the quantity picker before it is used.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        /// <param name="quantityPickerPanel">The quantity picker panel.</param>
        protected virtual void SetupQuantityPickerSettings(ItemInfo itemInfo, ItemUser itemUser, QuantityPickerPanel quantityPickerPanel)
        {
            quantityPickerPanel.QuantityPicker.MinQuantity = 1;
        }

        /// <summary>
        /// Assign the action.
        /// </summary>
        /// <param name="itemInfo">The item Info.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        /// <returns>The task.</returns>
        protected virtual async Task AssignActionAsync(ItemInfo itemInfo, ItemUser itemUser)
        {
            var returnedValue = await m_QuantityPickerPanel.WaitForQuantity();

            InvokeWithAwaitedValue(itemInfo, itemUser, returnedValue);
        }

        /// <summary>
        /// Invoke with the action with the awaited value. 
        /// </summary>
        /// <param name="itemInfo">The itemInfo.</param>
        /// <param name="itemUser">The item user.</param>
        /// <param name="awaitedValue">The value that was waited for.</param>
        protected abstract void InvokeWithAwaitedValue(ItemInfo itemInfo, ItemUser itemUser, int awaitedValue);
    }
}