/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.ItemActions
{
    using UI.Panels;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// An interface for an item action panel.
    /// </summary>
    public interface IActionWithPanel
    {
        /// <summary>
        /// Set the parent panel.
        /// </summary>
        /// <param name="parentDisplayPanel">The parent panel.</param>
        /// <param name="previousSelectable">THe previous selectable.</param>
        /// <param name="parentTransform">The parent transform.</param>
        void SetParentPanel(DisplayPanel parentDisplayPanel, Selectable previousSelectable, Transform parentTransform);
    }
}