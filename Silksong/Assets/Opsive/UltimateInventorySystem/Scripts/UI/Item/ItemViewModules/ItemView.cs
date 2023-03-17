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
    /// Item View UI.
    /// </summary>
    public class ItemView : View<ItemInfo>
    {
        public ItemInfo ItemInfo => m_CurrentValue;
    }
}