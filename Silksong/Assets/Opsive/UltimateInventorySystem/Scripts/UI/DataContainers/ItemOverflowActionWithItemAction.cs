/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.InventoryCollections
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.ItemActions;
    using UnityEngine;

    [CreateAssetMenu(fileName = "ItemOverflowAction", menuName = "Ultimate Inventory System/Item Overflow Actions/Item Overflow Action With Item Action", order = 51)]
    public class ItemOverflowActionWithItemAction : ItemOverflowAction
    {
        [SerializeField] protected ItemActionSet m_ItemActionSet;

        public override void HandleItemOverflow(IInventory inventory, ItemInfo originalItemInfo, ItemInfo itemInfoAdded, ItemInfo rejectedItemInfo)
        {
            for (int i = 0; i < m_ItemActionSet.ItemActionCollection.Count; i++) {
                var itemAction = m_ItemActionSet.ItemActionCollection[i];
                itemAction.Initialize(false);
                itemAction.InvokeAction(rejectedItemInfo, inventory.ItemUser);
            }
        }
    }
}