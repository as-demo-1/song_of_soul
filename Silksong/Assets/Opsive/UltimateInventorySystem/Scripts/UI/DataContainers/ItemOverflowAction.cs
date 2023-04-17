/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.InventoryCollections
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using UnityEngine;

    [CreateAssetMenu(fileName = "ItemOverflowAction", menuName = "Ultimate Inventory System/Item Overflow Actions/...", order = 51)]
    public abstract class ItemOverflowAction : ScriptableObject
    {
        abstract public void HandleItemOverflow(IInventory inventory, ItemInfo originalItemInfo, ItemInfo itemInfoAdded, ItemInfo rejectedItemInfo);
    }
}