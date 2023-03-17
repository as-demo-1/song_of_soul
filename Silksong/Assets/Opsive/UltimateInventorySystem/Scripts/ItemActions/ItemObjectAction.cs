/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.ItemObjectActions
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.ItemActions;

    /// <summary>
    /// An Item Action that allows you to specify a target ItemObject.
    /// </summary>
    public abstract class ItemObjectAction : ItemAction
    {

        protected ItemObject m_ItemObject;

        /// <summary>
        /// Set the item object.
        /// </summary>
        /// <param name="itemObject">The item object.</param>
        public virtual void SetItemObject(ItemObject itemObject)
        {
            m_ItemObject = itemObject;
        }
    }
}