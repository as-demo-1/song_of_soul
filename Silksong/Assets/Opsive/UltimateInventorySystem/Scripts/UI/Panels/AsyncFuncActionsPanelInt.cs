/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.UI
{
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.UI.Panels.ActionPanels;
    using System.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// Asynchronous action panel of integers.
    /// </summary>
    public class AsyncFuncActionsPanelInt : AsyncFuncActionsPanel<int>
    {
        [Tooltip("If true the object will be returned to the object when closed.")]
        [SerializeField]
        private bool m_ReturnToObjectPoolOnClose = true;

        /// <summary>
        /// Wait for a integer.
        /// </summary>
        /// <returns>The task.</returns>
        public override async Task<int> WaitForReturnedValueAsync()
        {
            while (m_WaitForInput) {
                await Task.Yield();
            }

            return m_ValueToReturn;
        }

        /// <summary>
        /// The cancel value is -1;
        /// </summary>
        /// <returns>-1</returns>
        protected override int GetCancelValue()
        {
            return -1;
        }

        /// <summary>
        /// Close the panel.
        /// </summary>
        /// <param name="selectPrevious">Select the previous panel?</param>
        public override void Close(bool selectPrevious = true)
        {
            base.Close(selectPrevious);
            if (m_ReturnToObjectPoolOnClose) {
                ObjectPool.Destroy(gameObject);
            }
        }
    }
}