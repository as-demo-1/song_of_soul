/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.InventoryCollections
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.DataContainers;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// The overflow action.
    /// </summary>
    [Serializable]
    public enum ItemCollectionOverflowAction
    {
        RejectIncomingItem,    //Do not add the item if it will make the collection overflow.
        ForceRemovePreviousItem      //Replace an item by the new one if it will make the collection overflow.
    }

    /// <summary>
    /// The item collection overflow settings.
    /// </summary>
    [Serializable]
    public struct ItemCollectionOverflow
    {
        [Tooltip("The action that should be performed if the itemCollection is about to overflow.")]
        [SerializeField] private ItemCollectionOverflowAction m_OverflowAction;
        [Tooltip("Remove the replaced Item from the ItemCollection.")]
        [SerializeField] private bool m_RemoveReplacedItem;
        [Tooltip("Should an item that gets rejected The action that should be performed if the itemCollection is about to overflow.")]
        [SerializeField] private bool m_OverflowBackToOrigin;
        [Tooltip("The ItemCollection where the rejected or forced out items go.")]
        [SerializeField] private ItemCollectionID m_OverflowItemCollection;
        [Tooltip("The actions performed on an item that was rejected from being added.")]
        [SerializeField] private CategoryItemActionSet m_RejectedItemActions;
        [Tooltip("The actions performed on an item that was forcibly removed.")]
        [SerializeField] private CategoryItemActionSet m_ForcedRemoveItemActions;

        public ItemCollectionOverflowAction OverflowAction => m_OverflowAction;
        public bool RemoveReplacedItem => m_RemoveReplacedItem;
        public bool OverflowBackToOrigin => m_OverflowBackToOrigin;
        public ItemCollectionID OverflowItemCollection => m_OverflowItemCollection;
        public CategoryItemActionSet RejectedItemActions => m_RejectedItemActions;
        public CategoryItemActionSet ForcedRemoveItemActions => m_ForcedRemoveItemActions;

        public ItemCollectionOverflow(ItemCollectionOverflowAction overflowAction,
            bool removeReplacedItem = true,
            bool overflowBackToOrigin = false, ItemCollectionID overflowItemCollection = default,
            CategoryItemActionSet rejectedItemActions = null,
            CategoryItemActionSet forcedRemoveItemActions = null)
        {
            m_OverflowAction = overflowAction;
            m_RemoveReplacedItem = removeReplacedItem;
            m_OverflowBackToOrigin = overflowBackToOrigin;
            m_OverflowItemCollection = overflowItemCollection;
            m_RejectedItemActions = rejectedItemActions;
            m_ForcedRemoveItemActions = forcedRemoveItemActions;
        }
    }

    /// <summary>
    /// The item Collection restriction types.
    /// </summary>
    [Serializable]
    [Flags]
    public enum ItemCollectionRestrictions
    {
        //None = 0,                //No restriction.
        FullSize = 1,            //Restrict the total number of item stacks.
        CategorySize = 2,        //Restrict the number of item per category.
        DefinitionSize = 4,      //Restrict the number of item per definition.
        ItemSize = 8,            //Restrict the number of item per item.
        CategoryRestriction = 16,//Restrict the item that can be added by category.
    }

    /// <summary>
    /// Item restriction comparer.
    /// </summary>
    public class ItemRestrictionComparer : Comparer<ItemInfo>
    {
        protected ItemInfo m_ItemAmountBeingAdded;
        protected ItemCollection m_ReceivingItemCollection;
        protected IReadOnlyList<ItemCollection> m_AffectedCollections;

        /// <summary>
        /// Set the sort parameters.
        /// </summary>
        /// <param name="sortParameters">The sort parameters.</param>
        public virtual void SetSortParameters((ItemInfo, ItemCollection, IReadOnlyList<ItemCollection>) sortParameters)
        {
            m_ItemAmountBeingAdded = sortParameters.Item1;
            m_ReceivingItemCollection = sortParameters.Item2;
            m_AffectedCollections = sortParameters.Item3;
        }

        /// <summary>
        /// Compare two item infos.
        /// </summary>
        /// <param name="x">item info 1</param>
        /// <param name="y">item info 2</param>
        /// <returns>move up or down?</returns>
        public override int Compare(ItemInfo x, ItemInfo y)
        {
            var ratio = 0;

            if (m_ReceivingItemCollection.HasItem(x.Item)) {
                ratio -= 1;
            }

            if (m_ItemAmountBeingAdded.Item == x.Item) {
                ratio -= 5;
            } else if (m_ItemAmountBeingAdded.Item.ItemDefinition.DirectlyContains(x.Item)) {
                ratio -= 4;
            } else if (m_ItemAmountBeingAdded.Item.ItemDefinition.InherentlyContains(x.Item)) {
                ratio -= 3;
            } else if (m_ItemAmountBeingAdded.Item.Category.DirectlyContains(x.Item)) {
                ratio -= 2;
            } else if (m_ItemAmountBeingAdded.Item.Category.InherentlyContains(x.Item)) {
                ratio -= 1;
            }

            if (m_ReceivingItemCollection.HasItem(y.Item)) {
                ratio += 1;
            }

            if (m_ItemAmountBeingAdded.Item == y.Item) {
                ratio += 5;
            } else if (m_ItemAmountBeingAdded.Item.ItemDefinition.DirectlyContains(y.Item)) {
                ratio += 4;
            } else if (m_ItemAmountBeingAdded.Item.ItemDefinition.InherentlyContains(y.Item)) {
                ratio += 3;
            } else if (m_ItemAmountBeingAdded.Item.Category.DirectlyContains(y.Item)) {
                ratio += 2;
            } else if (m_ItemAmountBeingAdded.Item.Category.InherentlyContains(y.Item)) {
                ratio += 1;
            }

            return ratio;
        }
    }

    /// <summary>
    /// A custom ItemCollection with limited space.
    /// </summary>
    [Serializable]
    public class GroupItemRestriction : IItemRestriction
    {
        [Tooltip("The itemCollections affected by this restriction.")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_ItemCollectionIds")]
        [SerializeField] protected ItemCollectionID[] m_ItemCollectionIDs;
        [Tooltip("The action that should be performed if the itemCollection is about to overflow.")]
        [SerializeField] protected ItemCollectionOverflow m_ItemCollectionOverflow;
        [Tooltip("The action that should be performed if the itemCollection is about to overflow.")]
        [SerializeField] protected ItemCollectionRestrictions m_ItemCollectionRestrictions = 0;
        [Tooltip("The unique item full limit.")]
        [SerializeField] protected int m_FullSizeLimit = int.MaxValue;
        [Tooltip("The unique item full limit per itemCategory.")]
        [SerializeField] protected int m_DefaultCategorySizeLimit = int.MaxValue;
        [Tooltip("The attribute name used for limiting the number of items with the category in the collection.")]
        [SerializeField] protected string m_LimitPerCategoryAttributeName = "CategorySizeLimit";
        [Tooltip("The unique item full limit per item definition.")]
        [SerializeField] protected int m_DefaultDefinitionSizeLimit = int.MaxValue;
        [Tooltip("The attribute name used for limiting the number of items with the definition in the collection.")]
        [SerializeField] protected string m_LimitPerDefinitionAttributeName = "DefinitionSizeLimit";
        [Tooltip("The stack limit for a unique stack of an immutable item.")]
        [SerializeField] protected int m_DefaultItemSizeLimit = int.MaxValue;
        [Tooltip("The attribute name used for limiting the unique stack size for an immutable item.")]
        [SerializeField] protected string m_ItemSizeLimitAttributeName = "ItemSizeLimit";
        [Tooltip("Should the itemCategory list define the categories to exclude or include.")]
        [SerializeField] protected bool m_RejectCategories = true;
        [Tooltip("The attribute name used for limiting the number of items with the definition in the collection.")]
        [SerializeField] protected DynamicItemCategoryArray m_ItemCategories;

        [System.NonSerialized] protected IInventory m_Inventory;
        [System.NonSerialized] protected ResizableArray<ItemCollection> m_ItemCollections;
        [System.NonSerialized] protected Func<ItemInfo, (ItemInfo, ItemCollection, IReadOnlyList<ItemCollection>), bool> m_ForceOutFilter;
        [System.NonSerialized] protected ItemRestrictionComparer m_ForceOutSort;

        [System.NonSerialized] protected bool m_Initialized = false;

        public ItemCollectionID[] ItemCollectionIDs {
            get => m_ItemCollectionIDs;
            set => m_ItemCollectionIDs = value;
        }

        public ItemCollectionOverflow ItemCollectionOverflow {
            get => m_ItemCollectionOverflow;
            set => m_ItemCollectionOverflow = value;
        }

        public ItemCollectionRestrictions ItemCollectionRestrictions {
            get => m_ItemCollectionRestrictions;
            set => m_ItemCollectionRestrictions = value;
        }

        public int FullSizeLimit {
            get => m_FullSizeLimit;
            set {
                m_ItemCollectionRestrictions |= ItemCollectionRestrictions.FullSize;
                m_FullSizeLimit = value;
            }
        }

        public int DefaultCategorySizeLimit {
            get => m_DefaultCategorySizeLimit;
            set {
                m_ItemCollectionRestrictions |= ItemCollectionRestrictions.CategorySize;
                m_DefaultCategorySizeLimit = value;
            }
        }

        public string LimitPerCategoryAttributeName {
            get => m_LimitPerCategoryAttributeName;
            set {
                m_ItemCollectionRestrictions |= ItemCollectionRestrictions.CategorySize;
                m_LimitPerCategoryAttributeName = value;
            }
        }

        public int DefaultDefinitionSizeLimit {
            get => m_DefaultDefinitionSizeLimit;
            set {
                m_ItemCollectionRestrictions |= ItemCollectionRestrictions.DefinitionSize;
                m_DefaultDefinitionSizeLimit = value;
            }
        }

        public string LimitPerDefinitionAttributeName {
            get => m_LimitPerDefinitionAttributeName;
            set {
                m_ItemCollectionRestrictions |= ItemCollectionRestrictions.DefinitionSize;
                m_LimitPerDefinitionAttributeName = value;
            }
        }

        public int DefaultItemSizeLimit {
            get => m_DefaultItemSizeLimit;
            set {
                m_ItemCollectionRestrictions |= ItemCollectionRestrictions.ItemSize;
                m_DefaultItemSizeLimit = value;
            }
        }

        public string ItemSizeLimitAttributeName {
            get => m_ItemSizeLimitAttributeName;
            set {
                m_ItemCollectionRestrictions |= ItemCollectionRestrictions.ItemSize;
                m_ItemSizeLimitAttributeName = value;
            }
        }

        public bool RejectCategories {
            get => m_RejectCategories;
            set {
                m_ItemCollectionRestrictions |= ItemCollectionRestrictions.CategoryRestriction;
                m_RejectCategories = value;
            }
        }

        public ItemCategory[] ItemCategories {
            get => m_ItemCategories;
            set {
                m_ItemCollectionRestrictions |= ItemCollectionRestrictions.CategoryRestriction;
                m_ItemCategories = value;
            }
        }

        public Func<ItemInfo, (ItemInfo, ItemCollection, IReadOnlyList<ItemCollection>), bool> ForceOutFilter {
            get => m_ForceOutFilter;
            set => m_ForceOutFilter = value;
        }

        public ItemRestrictionComparer ForceOutSort {
            get => m_ForceOutSort;
            set => m_ForceOutSort = value;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public GroupItemRestriction()
        {

        }

        /// <summary>
        /// Duplicate the object.
        /// </summary>
        /// <param name="other">The other object to duplicate.</param>
        public GroupItemRestriction(GroupItemRestriction other)
        {
            m_ItemCollectionIDs = other.m_ItemCollectionIDs;
            m_ItemCollectionOverflow = other.m_ItemCollectionOverflow;
            m_ItemCollectionRestrictions = other.m_ItemCollectionRestrictions;
            m_FullSizeLimit = other.m_FullSizeLimit;
            m_DefaultCategorySizeLimit = other.m_DefaultCategorySizeLimit;
            m_LimitPerCategoryAttributeName = other.m_LimitPerCategoryAttributeName;
            m_DefaultDefinitionSizeLimit = other.m_DefaultDefinitionSizeLimit;
            m_LimitPerDefinitionAttributeName = other.m_LimitPerDefinitionAttributeName;
            m_DefaultItemSizeLimit = other.m_DefaultItemSizeLimit;
            m_ItemSizeLimitAttributeName = other.m_ItemSizeLimitAttributeName;
            m_RejectCategories = other.m_RejectCategories;
            m_ItemCategories = other.m_ItemCategories;
        }

        /// <summary>
        /// Initialize the item collection.
        /// </summary>
        /// <param name="inventory">The inventory.</param>
        /// <param name="force">Force to initialize.</param>
        public void Initialize(IInventory inventory, bool force)
        {
            if (m_Initialized && !force && inventory == m_Inventory) { return; }

            if (m_Inventory != inventory) {
                m_Inventory = inventory;
                if (m_ItemCollections == null) { m_ItemCollections = new ResizableArray<ItemCollection>(); }
                m_ItemCollections.Clear();
                for (int i = 0; i < m_ItemCollectionIDs.Length; i++) {
                    var match = m_Inventory.GetItemCollection(m_ItemCollectionIDs[i]);
                    if (match == null) { continue; }
                    m_ItemCollections.Add(match);
                }
            }

            m_Initialized = true;
            m_ForceOutFilter = (itemAmountToRemove, filterParam) =>
            {
                var itemAmountBeingAdded = filterParam.Item1;
                var receivingItemCollection = filterParam.Item2;
                var affectedItemCollections = filterParam.Item3;
                for (int i = 0; i < affectedItemCollections.Count; i++) {
                    if (itemAmountToRemove.ItemCollection == affectedItemCollections[i]) { return true; }
                }

                return false;
            };

            m_ForceOutSort = new ItemRestrictionComparer();
        }

        /// <summary>
        /// The add condition checks all the restrictions and changes the amount the should be added.
        /// </summary>
        /// <param name="itemInfo">The item info to add (contains the origin of the item).</param>
        /// <param name="receivingCollection">The item collection receiving the item.</param>
        /// <returns>The item info to add.</returns>
        public ItemInfo? CanAddItem(ItemInfo itemInfo, ItemCollection receivingCollection)
        {
            ItemInfo? result = itemInfo;

            if (m_ItemCollectionIDs == null) { return result; }

            var match = false;
            for (int i = 0; i < m_ItemCollectionIDs.Length; i++) {
                if (m_ItemCollectionIDs[i].Compare(receivingCollection)) {
                    match = true;
                    break;
                }
            }

            if (!match) { return result; }

            if ((m_ItemCollectionRestrictions & ItemCollectionRestrictions.CategoryRestriction) != 0) {
                if (ItemCategories != null) {
                    if (m_RejectCategories) {
                        for (int i = 0; i < ItemCategories.Length; i++) {
                            if (ItemCategories[i].InherentlyContains(itemInfo.Item)) { return null; }
                        }
                    } else {
                        var foundMatch = false;
                        for (int i = 0; i < ItemCategories.Length; i++) {
                            if (ItemCategories[i].InherentlyContains(itemInfo.Item)) {
                                foundMatch = true;
                                break;
                            }
                        }

                        if (foundMatch == false) { return null; }
                    }
                }

                itemInfo = result.Value;
            }

            if ((m_ItemCollectionRestrictions & ItemCollectionRestrictions.FullSize) != 0) {
                result = ValidateWithCollectionSize(itemInfo, receivingCollection);
                if (result.HasValue == false) { return null; }

                itemInfo = result.Value;
            }

            if ((m_ItemCollectionRestrictions & ItemCollectionRestrictions.CategorySize) != 0) {
                result = ValidateWithCategorySize(itemInfo, receivingCollection);
                if (result.HasValue == false) { return null; }

                itemInfo = result.Value;
            }

            if ((m_ItemCollectionRestrictions & ItemCollectionRestrictions.DefinitionSize) != 0) {
                result = ValidateWithDefinitionSize(itemInfo, receivingCollection);
                if (result.HasValue == false) { return null; }

                itemInfo = result.Value;
            }

            if ((m_ItemCollectionRestrictions & ItemCollectionRestrictions.ItemSize) != 0) {
                result = ValidateWithStackSize(itemInfo, receivingCollection);
                if (result.HasValue == false) { return null; }

                itemInfo = result.Value;
            }

            return itemInfo;
        }

        /// <summary>
        /// Validate that you can add the item info with the collection size restriction.
        /// </summary>
        /// <param name="itemInfo">The item info to add (contains the origin of the item).</param>
        /// <param name="receivingCollection">The item collection receiving the item.</param>
        /// <param name="count">The amount of item that exist in the collections.</param>
        /// <param name="sizeLimit">The size limit.</param>
        /// <returns>The new valid item amount.</returns>
        protected virtual ItemInfo? ValidateInternal(ItemInfo itemInfo, ItemCollection receivingCollection, int count,
            int sizeLimit)
        {
            count += itemInfo.Amount;

            if (count <= sizeLimit) { return itemInfo; }

            var overflowingAmount = count - sizeLimit;

            if (!CollectionsWillOverflow((overflowingAmount, itemInfo), receivingCollection)) { return itemInfo; }

            return (itemInfo.Amount - overflowingAmount, itemInfo);
        }

        /// <summary>
        /// Validate that you can add the item info with the collection size restriction.
        /// </summary>
        ///  <param name="itemInfo">The item info to add (contains the origin of the item).</param>
        /// <param name="receivingCollection">The item collection receiving the item.</param>
        /// <returns>The new valid item amount.</returns>
        protected virtual ItemInfo? ValidateWithCollectionSize(ItemInfo itemInfo, ItemCollection receivingCollection)
        {
            var item = itemInfo.Item;

            if (!item.IsUnique && (receivingCollection.HasItem((1, item)) && !(receivingCollection is MultiStackItemCollection))) { return itemInfo; }

            var count = 0;
            for (int i = 0; i < m_ItemCollections.Count; i++) {
                count += m_ItemCollections[i].ItemStacks.Count;
            }

            var stacksToAdd = item.IsUnique ? itemInfo.Amount : 1;

            count += stacksToAdd;

            if (count <= m_FullSizeLimit) { return itemInfo; }

            if (stacksToAdd == 1) {
                if (!CollectionsWillOverflow(itemInfo, receivingCollection)) { return itemInfo; }

                return null;
            }

            var overflowingAmount = count - m_FullSizeLimit;
            if (!CollectionsWillOverflow((overflowingAmount, itemInfo), receivingCollection)) {
                return itemInfo;
            }

            return (itemInfo.Amount - overflowingAmount, itemInfo);

        }

        /// <summary>
        /// Validate that you can add the item info with the collection size per category restriction.
        /// </summary>
        ///  <param name="itemInfo">The item info to add (contains the origin of the item).</param>
        /// <param name="receivingCollection">The item collection receiving the item.</param>
        /// <returns>The new valid item amount.</returns>
        protected virtual ItemInfo? ValidateWithCategorySize(ItemInfo itemInfo, ItemCollection receivingCollection)
        {
            var item = itemInfo.Item;

            if (item.TryGetAttributeValue<int>(m_LimitPerCategoryAttributeName, out var sizeLimit) == false) {
                sizeLimit = m_DefaultCategorySizeLimit;
            };

            var count = 0;
            for (int i = 0; i < m_ItemCollections.Count; i++) { count += m_ItemCollections[i].GetItemAmount(item.Category, true, true); }

            return ValidateInternal(itemInfo, receivingCollection, count, sizeLimit);
        }



        /// <summary>
        /// Validate that you can add the item info with the collection size per itemDefinition restriction.
        /// </summary>
        /// <param name="itemInfo">The item info to add (contains the origin of the item).</param>
        /// <param name="receivingCollection">The item collection receiving the item.</param>
        /// <returns>The new valid item amount.</returns>
        protected virtual ItemInfo? ValidateWithDefinitionSize(ItemInfo itemInfo, ItemCollection receivingCollection)
        {
            var item = itemInfo.Item;

            if (item.TryGetAttributeValue<int>(m_LimitPerDefinitionAttributeName, out var sizeLimit) == false) {
                sizeLimit = m_DefaultDefinitionSizeLimit;
            }

            var count = 0;
            for (int i = 0; i < m_ItemCollections.Count; i++) { count += m_ItemCollections[i].GetItemAmount(item.ItemDefinition, true, false); }

            return ValidateInternal(itemInfo, receivingCollection, count, sizeLimit);
        }

        /// <summary>
        /// Validate that you can add the item info with the stack size limit restriction.
        /// </summary>
        /// <param name="itemInfo">The item info to add (contains the origin of the item).</param>
        /// <param name="receivingCollection">The item collection receiving the item.</param>
        /// <returns>The new valid item amount.</returns>
        protected virtual ItemInfo? ValidateWithStackSize(ItemInfo itemInfo, ItemCollection receivingCollection)
        {
            var item = itemInfo.Item;

            if (item.TryGetAttributeValue<int>(m_ItemSizeLimitAttributeName, out var sizeLimit) == false) {
                sizeLimit = m_DefaultItemSizeLimit;
            }

            var count = 0;
            for (int i = 0; i < m_ItemCollections.Count; i++) { count += m_ItemCollections[i].GetItemAmount(item); }

            return ValidateInternal(itemInfo, receivingCollection, count, sizeLimit);
        }

        /// <summary>
        /// The collection is about to overflow. Reject the incoming item or remove an existing one.
        /// </summary>
        /// <param name="itemInfo">The item info to add (contains the origin of the item).</param>
        /// <param name="receivingCollection">The item collection receiving the item.</param>
        /// <returns>True if the collection will overflow if the item is added.</returns>
        protected virtual bool CollectionsWillOverflow(ItemInfo itemInfo, ItemCollection receivingCollection)
        {
            if (m_ItemCollectionOverflow.OverflowAction == ItemCollectionOverflowAction.ForceRemovePreviousItem) {
                var itemAmountToForceRemove = GetItemToForceRemove(itemInfo, receivingCollection);
                if (itemAmountToForceRemove.HasValue) {
                    ForceRemoveItem(itemAmountToForceRemove.Value, receivingCollection);
                    return false;
                }
            }

            RejectItem(itemInfo, receivingCollection);
            return true;
        }

        /// <summary>
        /// Reject the item.
        /// </summary>
        /// <param name="itemInfo">The item info to add (contains the origin of the item).</param>
        /// <param name="receivingCollection">The item collection receiving the item.</param>
        protected void RejectItem(ItemInfo itemInfo, ItemCollection receivingCollection)
        {
            if (m_ItemCollectionOverflow.OverflowBackToOrigin) {
                itemInfo.ItemCollection.AddItem(itemInfo);
            } else {
                var overflowCollection = receivingCollection.Inventory?.GetItemCollection(m_ItemCollectionOverflow.OverflowItemCollection);
                overflowCollection?.AddItem(itemInfo);
            }

            m_ItemCollectionOverflow.RejectedItemActions?.UseAllItemActions(itemInfo, m_Inventory.ItemUser);
            EventHandler.ExecuteEvent<ItemInfo>(m_Inventory, EventNames.c_Inventory_OnRejected_ItemInfo, itemInfo);
        }

        /// <summary>
        /// Force removes the item.
        /// </summary>
        /// <param name="itemInfo">The item to force remove.</param>
        /// <param name="receivingCollection">The item collection that was supposed to receive the item.</param>
        protected void ForceRemoveItem(ItemInfo itemInfo, ItemCollection receivingCollection)
        {
            if (m_ItemCollectionOverflow.RemoveReplacedItem) {
                itemInfo.ItemCollection.RemoveItem(itemInfo);
            }

            var overflowCollection = receivingCollection.Inventory?.GetItemCollection(m_ItemCollectionOverflow.OverflowItemCollection);
            overflowCollection?.AddItem(itemInfo);

            EventHandler.ExecuteEvent<ItemInfo>(m_Inventory, EventNames.c_Inventory_OnWillForceRemove_ItemInfo, itemInfo);
            m_ItemCollectionOverflow.ForcedRemoveItemActions?.UseAllItemActions(itemInfo, m_Inventory.ItemUser);
            EventHandler.ExecuteEvent<ItemInfo>(m_Inventory, EventNames.c_Inventory_OnForceRemove_ItemInfo, itemInfo);
        }

        /// <summary>
        /// Get the item that should be forced out next.
        /// </summary>
        /// <param name="itemInfo">The item info which is being added.</param>
        ///  <param name="receivingCollection">The item collection receiving the item.</param>
        /// <returns>The item info that should be removed.</returns>
        protected virtual ItemInfo? GetItemToForceRemove(ItemInfo itemInfo, ItemCollection receivingCollection)
        {
            var pooledArray = GenericObjectPool.Get<ItemInfo[]>();
            var filterAndSortParameters = (itemInfo, receivingCollection, m_ItemCollections);
            ForceOutSort.SetSortParameters(filterAndSortParameters);

            var filteredSlice = m_Inventory.GetItemInfos<(ItemInfo, ItemCollection, IReadOnlyList<ItemCollection>)>(ref pooledArray,
                filterAndSortParameters, m_ForceOutFilter, m_ForceOutSort);
            if (filteredSlice.Count >= 1) {
                return (itemInfo.Amount, filteredSlice[0]);
            }
            GenericObjectPool.Return(pooledArray);

            return null;
        }

        /// <summary>
        /// No restriction when removing items.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <returns>The item info to remove.</returns>
        public ItemInfo? CanRemoveItem(ItemInfo itemInfo)
        {
            return itemInfo;
        }
    }
}