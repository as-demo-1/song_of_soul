/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.UltimateInventorySystem.Input;
    using Opsive.UltimateInventorySystem.UI.Views;

    /// <summary>
    /// Base class for Item View Modules which are selectable.
    /// </summary>
    public abstract class ItemViewModuleSelectable : ItemViewModule, IViewModuleSelectable
    {
        /// <summary>
        /// Clear.
        /// </summary>
        public override void Clear()
        {
            var view = m_BaseView;
            if (view == null) {
                Select(false);
                return;
            }

            if (view.ViewSlot == null) {
                Select(false);
                return;
            }

            var viewSlotGameObject = view.ViewSlot.gameObject;
            var @select = EventSystemManager.GetEvenSystemFor(viewSlotGameObject).currentSelectedGameObject == viewSlotGameObject;
            Select(@select);
        }

        public abstract void Select(bool @select);
    }
}