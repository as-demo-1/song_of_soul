/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Views
{
    using Opsive.UltimateInventorySystem.Input;

    /// <summary>
    /// A box select.
    /// </summary>
    public interface IViewModuleSelectable
    {
        /// <summary>
        /// Select or unselect the box.
        /// </summary>
        /// <param name="select">select or unselect.</param>
        void Select(bool select);
    }

    /// <summary>
    /// A box which is clickable.
    /// </summary>
    public interface IViewModuleClickable
    {
        /// <summary>
        /// Click the box.
        /// </summary>
        void Click();
    }

    /// <summary>
    /// A box which is clickable.
    /// </summary>
    public interface IViewModuleMovable
    {
        /// <summary>
        /// Click the box.
        /// </summary>
        void SetAsMoving();

        /// <summary>
        /// Click the box.
        /// </summary>
        void SetAsMovingSource(bool movingSource);
    }
}