/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.InventoryCollections
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Exchange;
    using Opsive.UltimateInventorySystem.ItemActions;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Interface for the inventory.
    /// </summary>
    public interface IInventory
    {
        /// <summary>
        /// Get the game object attached to the inventory.
        /// </summary>
        GameObject gameObject { get; }

        /// <summary>
        /// Get the item user attached to the inventory.
        /// </summary>
        ItemUser ItemUser { get; }

        /// <summary>
        /// Get the main itemCollection.
        /// </summary>
        ItemCollection MainItemCollection { get; }

        /// <summary>
        /// Get the currency owner.
        /// </summary>
        /// <typeparam name="T">The currency owner type.</typeparam>
        /// <returns>The currency owner found.</returns>
        ICurrencyOwner<T> GetCurrencyComponent<T>();

        /// <summary>
        /// The item Collections.
        /// </summary>
        IReadOnlyList<ItemCollection> ItemCollectionsReadOnly { get; }

        /// <summary>
        /// Can the item be added to the item
        /// </summary>
        /// <param name="itemInfo">The item info to add.</param>
        /// <param name="receivingCollection">The receiving item Collection.</param>
        /// <returns>The itemInfo that can be added (can be null).</returns>
        ItemInfo? CanAddItem(ItemInfo itemInfo, ItemCollection receivingCollection);

        /// <summary>
        /// Can the item be removed from the itemCollection.
        /// </summary>
        /// <param name="itemInfo">The item info to remove (contains the itemCollection).</param>
        /// <returns>The item info that can be removed (can be null).</returns>
        ItemInfo? CanRemoveItem(ItemInfo itemInfo);

        /// <summary>
        /// Add an item to the inventory.
        /// </summary>
        /// <param name="itemInfo">The amount of item being added.</param>
        /// <param name="stackTarget">The item stack where the item should be added.</param>
        /// <returns>Returns the number of items added, 0 if no item was added.</returns>
        ItemInfo AddItem(ItemInfo itemInfo, ItemStack stackTarget = null);

        /// <summary>
        /// Remove an item from the inventory.
        /// </summary>
        /// <param name="itemInfo">The amount of item to remove.</param>
        /// <returns>Returns true if the item was removed correctly.</returns>
        ItemInfo RemoveItem(ItemInfo itemInfo);

        /// <summary>
        /// Get the item collection with the ID specified.
        /// </summary>
        /// <param name="itemCollectionID">The item Collection ID.</param>
        /// <returns>The item collection.</returns>
        ItemCollection GetItemCollection(ItemCollectionID itemCollectionID);

        /// <summary>
        /// Get the first item info for the item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The itemInfo.</returns>
        ItemInfo? GetItemInfo(Item item);

        /// <summary>
        /// Returns the first itemInfo that matches the description.
        /// </summary>
        /// <param name="itemDefinition">The Item Definition to retrieve the amount of.</param>
        /// <param name="checkInherently">Take into account the children of the item definition.</param>
        /// <returns>The first item info found in the inventory.</returns>
        ItemInfo? GetItemInfo(ItemDefinition itemDefinition, bool checkInherently = false);

        /// <summary>
        /// Returns the first itemInfo that matches the description.
        /// </summary>
        /// <param name="itemCategory">The Item Category to retrieve the amount of.</param>
        /// <param name="checkInherently">Take into account the children of the item category.</param>
        /// <returns>The first item info found in the inventory.</returns>
        ItemInfo? GetItemInfo(ItemCategory itemCategory, bool checkInherently = false);

        /// <summary>
        /// Get a filtered and sorted set of item infos in the inventory.
        /// </summary>
        /// <param name="itemInfos">The array holding the resulting itemInfos.</param>
        /// <param name="filterParam">The filter parameter.</param>
        /// <param name="filterFunc">The filter function.</param>
        /// <param name="startIndex">The start index for the result.</param>
        /// <typeparam name="T">The filter parameter type.</typeparam>
        /// <returns>The list slice of the item infos.</returns>
        ListSlice<ItemInfo> GetItemInfos<T>(ref ItemInfo[] itemInfos, T filterParam,
            Func<ItemInfo, T, bool> filterFunc, int startIndex = 0);

        /// <summary>
        /// Get a filtered and sorted set of item infos in the inventory.
        /// </summary>
        /// <param name="itemInfos">The array holding the resulting itemInfos.</param>
        /// <param name="filterParam">The filter parameter.</param>
        /// <param name="filterFunc">The filter function.</param>
        /// <param name="sortComparer">The sort comparer.</param>
        /// <param name="startIndex">The start index for the result.</param>
        /// <typeparam name="T">The filter parameter type.</typeparam>
        /// <returns>The list slice of the item infos.</returns>
        ListSlice<ItemInfo> GetItemInfos<T>(ref ItemInfo[] itemInfos, T filterParam,
            Func<ItemInfo, T, bool> filterFunc, Comparer<ItemInfo> sortComparer, int startIndex = 0);

        /// <summary>
        /// Does the inventory have this list of items.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        /// <returns>Returns true if the inventory has all the items in the list.</returns>
        bool HasItemList(ListSlice<ItemInfo> itemList);
    }
}