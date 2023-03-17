/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers
{
    using Opsive.Shared.Editor.UIElements.Managers;
    using Opsive.UltimateInventorySystem.Editor.Styles;

    /// <summary>
    /// The Manager is an abstract class which allows for various categories to the drawn to the MainManagerWindow pane.
    /// </summary>
    [System.Serializable]
    public abstract class InventoryManager : Manager
    {
        protected InventoryMainWindow m_InventoryMainWindow;

        public InventoryMainWindow InventoryMainWindow => m_InventoryMainWindow;

        /// <summary>
        /// Initializes the manager after deserialization.
        /// </summary>
        /// <param name="mainManagerWindow">A reference to the Main Manager Window.</param>
        public override void Initialize(MainManagerWindow mainManagerWindow)
        {
            base.Initialize(mainManagerWindow);
            m_ManagerContentContainer.AddToClassList(InventoryManagerStyles.ManagerContentContainer);

            m_InventoryMainWindow = mainManagerWindow as InventoryMainWindow;
        }
    }
}