/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.ItemObjectBehaviours
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.ItemActions;
    using UnityEngine;

    /// <summary>
    /// An item object action that log information about the item, used for debugging.
    /// </summary>
    public class ItemObjectBehaviourLogItem : ItemObjectBehaviour
    {
        /// <summary>
        /// Debug some information about the item.
        /// </summary>
        /// <param name="itemObject">The item Object.</param>
        /// <param name="itemObjectHandler">The item User.</param>
        public override void Use(ItemObject itemObject, ItemUser itemUser)
        {
            Debug.Log("Item user: " + itemUser);
            Debug.Log("Item : " + itemObject.Item);
            Debug.Log("Item attributes: " + AttributeCollection.AttributesToString((itemObject.Item.GetAttributeList(), 0)));
            Debug.Log("ItemDefinition attributes: " + AttributeCollection.AttributesToString((itemObject.Item.ItemDefinition.GetAttributeList(), 0)));
            Debug.Log("ItemCategory attributes: " + AttributeCollection.AttributesToString((itemObject.Item.Category.GetCategoryAttributeList(), 0)));
        }
    }
}