/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.Views;

    /// <summary>
    /// Ans abstract class for Item View UI components.
    /// </summary>
    public abstract class ItemViewModule : ViewModule<ItemInfo>
    {
        public ItemInfo ItemInfo => m_View.CurrentValue;
    }
}